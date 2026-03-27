using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class TimbradoControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkOrdenado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var timbrados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTimbrado(2, 1, 2, 1, "T-2", new DateOnly(2026, 2, 1), new DateOnly(2026, 12, 31), 1, 100, true),
            BuildTimbrado(1, 1, 1, 1, "T-1", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1, 100, true)
        });
        db.Timbrados.Returns(timbrados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[1], "Id", 2L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var timbrados = MockDbSetHelper.CreateMockDbSet<Timbrado>();
        db.Timbrados.Returns(timbrados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var timbrados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTimbrado(4, 1, 1, 1, "T-1", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1, 100, true)
        });
        db.Timbrados.Returns(timbrados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 4L);
        AssertAnonymousProperty(ok.Value!, "NroTimbrado", "T-1");
        AssertAnonymousProperty(ok.Value!, "Activo", true);
    }

    [Fact]
    public async Task GetVigente_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var timbrados = MockDbSetHelper.CreateMockDbSet<Timbrado>();
        db.Timbrados.Returns(timbrados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetVigente(new DateOnly(2026, 3, 20), 1, 1, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetVigente_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var timbrados = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTimbrado(4, 1, 1, 1, "T-1", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1, 100, true)
        });
        db.Timbrados.Returns(timbrados);
        var controller = CreateController(mediator, db);

        var result = await controller.GetVigente(new DateOnly(2026, 3, 20), 1, 1, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 4L);
        AssertAnonymousProperty(ok.Value!, "NroTimbrado", "T-1");
    }

    [Fact]
    public async Task ValidarEmision_DevuelveOkConResultadoDelMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        var expected = new ValidacionTimbradoParaguayDto
        {
            SucursalId = 1,
            PuntoFacturacionId = 2,
            TipoComprobanteId = 3,
            Fecha = new DateOnly(2026, 3, 20),
            NumeroComprobante = 15,
            EsValido = true,
            NroTimbrado = "T-1"
        };
        mediator.Send(Arg.Any<ValidarTimbradoParaguayQuery>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await controller.ValidarEmision(1, 2, 3, new DateOnly(2026, 3, 20), 15, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(expected);
        await mediator.Received(1).Send(
            Arg.Is<ValidarTimbradoParaguayQuery>(query =>
                query.SucursalId == 1 &&
                query.PuntoFacturacionId == 2 &&
                query.TipoComprobanteId == 3 &&
                query.Fecha == new DateOnly(2026, 3, 20) &&
                query.NumeroComprobante == 15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Rango inválido."));

        var result = await controller.Create(
            new CreateTimbradoRequest(1, 1, 1, "T-1", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1, 100),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(10L));

        var request = new CreateTimbradoRequest(1, 1, 1, "T-1", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1, 100);

        var result = await controller.Create(
            request,
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TimbradoController.GetById));
        AssertAnonymousProperty(created.Value!, "id", 10L);
        await mediator.Received(1).Send(
            Arg.Is<CreateTimbradoCommand>(command =>
                command.SucursalId == request.SucursalId &&
                command.PuntoFacturacionId == request.PuntoFacturacionId &&
                command.TipoComprobanteId == request.TipoComprobanteId &&
                command.NroTimbrado == request.NroTimbrado),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Timbrado no encontrado."));

        var result = await controller.Update(8, new UpdateTimbradoRequest(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1, 100), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Rango inválido."));

        var result = await controller.Update(8, new UpdateTimbradoRequest(new DateOnly(2026, 12, 31), new DateOnly(2026, 1, 1), 1, 100), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConIdYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var request = new UpdateTimbradoRequest(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1, 100);

        var result = await controller.Update(8, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 8L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateTimbradoCommand>(command =>
                command.Id == 8 &&
                command.FechaInicio == request.FechaInicio &&
                command.FechaFin == request.FechaFin &&
                command.NroComprobanteDesde == request.NroComprobanteDesde &&
                command.NroComprobanteHasta == request.NroComprobanteHasta),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Timbrado no encontrado."));

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateTimbradoCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateTimbradoCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateTimbradoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Timbrado no encontrado."));

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static TimbradoController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new TimbradoController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Timbrado BuildTimbrado(long id, long sucursalId, long puntoFacturacionId, long tipoComprobanteId, string nroTimbrado, DateOnly fechaInicio, DateOnly fechaFin, int nroDesde, int nroHasta, bool activo)
    {
        var entity = Timbrado.Crear(sucursalId, puntoFacturacionId, tipoComprobanteId, nroTimbrado, fechaInicio, fechaFin, nroDesde, nroHasta);
        typeof(Timbrado).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!activo)
            entity.Desactivar();

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}