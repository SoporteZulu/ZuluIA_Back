using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class TransportistasControllerTests
{
    [Fact]
    public async Task GetAll_CuandoSoloActivosYPatente_FiltraYEnriquece()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var transportistas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTransportista(1, 21, "20-123", "Cordoba", "abc123", "Iveco", true),
            BuildTransportista(2, 22, "20-456", "Rosario", "zzz999", "Scania", true),
            BuildTransportista(3, 23, "20-789", "Mendoza", "ABC777", "Volvo", false)
        });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTercero(21, "TR001", "Trans Uno", "20111111111")
        });
        db.Transportistas.Returns(transportistas);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(true, "abc", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "Patente", "ABC123");
        AssertAnonymousProperty(items[0], "TerceroRazonSocial", "Trans Uno");
        AssertAnonymousProperty(items[0], "TerceroCuit", "20111111111");
    }

    [Fact]
    public async Task GetAll_CuandoFaltaTercero_UsaFallbacks()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var transportistas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTransportista(1, 99, null, null, "abc123", null, true)
        });
        var terceros = MockDbSetHelper.CreateMockDbSet<Tercero>(Array.Empty<Tercero>());
        db.Transportistas.Returns(transportistas);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var item = ((IEnumerable)ok.Value!).Cast<object>().Single();
        AssertAnonymousProperty(item, "TerceroRazonSocial", "—");
        AssertAnonymousProperty(item, "TerceroCuit", "—");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var transportistas = MockDbSetHelper.CreateMockDbSet<Transportista>(Array.Empty<Transportista>());
        db.Transportistas.Returns(transportistas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el transportista con ID 7");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkEnriquecido()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var transportistas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTransportista(7, 21, "20-123", "Cordoba", "abc123", "Iveco", true)
        });
        var terceros = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTercero(21, "TR001", "Trans Uno", "20111111111")
        });
        db.Transportistas.Returns(transportistas);
        db.Terceros.Returns(terceros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "TerceroRazonSocial", "Trans Uno");
        AssertAnonymousProperty(ok.Value!, "Patente", "ABC123");
        AssertAnonymousProperty(ok.Value!, "Activo", true);
    }

    [Fact]
    public async Task Create_CuandoFallaPorDuplicado_DevuelveConflictConError()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateTransportistaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un transportista para el tercero ID 21."));
        var controller = CreateController(mediator);

        var result = await controller.Create(new CreateTransportistaRequest(21, "20-123", "Cordoba", "abc123", "Iveco"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe un transportista");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateTransportistaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));
        var controller = CreateController(mediator);

        var result = await controller.Create(new CreateTransportistaRequest(21, "20-123", "Cordoba", "abc123", "Iveco"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetTransportistaById");
        AssertAnonymousProperty(created.Value!, "id", 9L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTransportistaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el transportista con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Update(7, new UpdateTransportistaRequest("Cordoba", "abc123", "Iveco"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el transportista con ID 7");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTransportistaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Update(7, new UpdateTransportistaRequest("Cordoba", "abc123", "Iveco"), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value!.ToString().Should().Contain("Transportista actualizado correctamente");
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeactivateTransportistaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el transportista con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el transportista con ID 7");
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeactivateTransportistaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value!.ToString().Should().Contain("Transportista desactivado correctamente");
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateTransportistaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el transportista con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el transportista con ID 7");
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateTransportistaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value!.ToString().Should().Contain("Transportista activado correctamente");
        await mediator.Received(1).Send(
            Arg.Is<ActivateTransportistaCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static TransportistasController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new TransportistasController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Transportista BuildTransportista(
        long id,
        long terceroId,
        string? nroCuit,
        string? domicilioPartida,
        string? patente,
        string? marcaVehiculo,
        bool activo)
    {
        var entity = Transportista.Crear(terceroId, nroCuit, domicilioPartida, patente, marcaVehiculo);
        if (!activo)
            entity.Desactivar();
        SetEntityId(entity, id);
        return entity;
    }

    private static Tercero BuildTercero(long id, string legajo, string razonSocial, string nroDocumento)
    {
        var entity = Tercero.Crear(legajo, razonSocial, 1, nroDocumento, 1, true, false, false, null, null);
        SetEntityId(entity, id);
        return entity;
    }

    private static void SetEntityId(object entity, long id)
    {
        var type = entity.GetType();
        while (type is not null)
        {
            var property = type.GetProperty("Id");
            if (property is not null)
            {
                property.SetValue(entity, id);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException("No se pudo localizar la propiedad Id.");
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}