using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Referencia.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ZonasControllerTests
{
    [Fact]
    public async Task GetAll_CuandoFiltraActivas_DevuelveOrdenadasPorDescripcion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var zonas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildZona(2, "Norte", true),
            BuildZona(1, "Centro", true),
            BuildZona(3, "Sur", false)
        });
        db.Zonas.Returns(zonas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Centro");
        AssertAnonymousProperty(items[1], "Descripcion", "Norte");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var zonas = MockDbSetHelper.CreateMockDbSet(Array.Empty<Zona>());
        db.Zonas.Returns(zonas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Zona 7 no encontrada");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var zonas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildZona(7, "Centro", true)
        });
        db.Zonas.Returns(zonas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Centro");
        AssertAnonymousProperty(ok.Value!, "Activo", true);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new ZonaRequest(""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task Create_CuandoTieneExitoYSePuedeReleer_DevuelveCreatedAtRouteConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var zonas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildZona(21, "Centro", true)
        });
        db.Zonas.Returns(zonas);
        mediator.Send(Arg.Any<CreateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new ZonaRequest("Centro"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetZonaById");
        AssertAnonymousProperty(created.Value!, "Id", 21L);
        AssertAnonymousProperty(created.Value!, "Descripcion", "Centro");
        AssertAnonymousProperty(created.Value!, "Activo", true);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFoundConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Zona no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new ZonaRequest("Centro"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Zona no encontrada");
    }

    [Fact]
    public async Task Update_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La descripción es requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new ZonaRequest(""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task Update_CuandoTieneExitoYSePuedeReleer_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var zonas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildZona(7, "Centro", true)
        });
        db.Zonas.Returns(zonas);
        mediator.Send(Arg.Any<UpdateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Update(7, new ZonaRequest("Centro"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "Descripcion", "Centro");
        AssertAnonymousProperty(ok.Value!, "Activo", true);
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Zona no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateZonaCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Zona no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateZonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Desactivar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateZonaCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static ZonasController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ZonasController(mediator, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static Zona BuildZona(long id, string descripcion, bool activo)
    {
        var entity = Zona.Crear(descripcion);
        SetEntityId(entity, id);
        if (!activo)
            entity.Desactivar();
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