using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Logistica.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class OrdenesEmpaqueControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYOrdenDescendente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet([
            BuildOrdenEmpaque(1, 10, "PENDIENTE", false, new DateOnly(2026, 3, 10), 100m),
            BuildOrdenEmpaque(2, 10, "CONFIRMADO", false, new DateOnly(2026, 3, 15), 150m),
            BuildOrdenEmpaque(3, 11, "CONFIRMADO", true, new DateOnly(2026, 3, 20), 200m)
        ]);
        db.OrdenesEmpaquesLogistica.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(10, "confirmado", false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(1);
        AssertAnonymousProperty(data[0], "Id", 2L);
        AssertAnonymousProperty(data[0], "Estado", "CONFIRMADO");
        AssertAnonymousProperty(data[0], "Total", 150m);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet(Array.Empty<OrdenEmpaque>());
        db.OrdenesEmpaquesLogistica.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Orden de empaque 99 no encontrada.");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalles()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var orden = BuildOrdenEmpaque(5, 12, "PENDIENTE", false, new DateOnly(2026, 3, 21), 242m);
        orden.AgregarDetalle(9, "Caja master", 2m, 100m, 21m, "fragil");
        orden.AgregarDetalle(null, "Etiqueta", 1m, 42m, null, null);
        var items = MockDbSetHelper.CreateMockDbSet([orden]);
        db.OrdenesEmpaquesLogistica.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "TerceroId", 12L);
        AssertAnonymousProperty(ok.Value!, "Estado", "PENDIENTE");
        var detalles = ok.Value!
            .GetType()
            .GetProperty("Detalles")!
            .GetValue(ok.Value)!
            .Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        detalles.Should().HaveCount(2);
        AssertAnonymousProperty(detalles[0], "Descripcion", "Caja master");
        AssertAnonymousProperty(detalles[0], "Total", 200m);
        AssertAnonymousProperty(detalles[1], "Descripcion", "Etiqueta");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateOrdenEmpaqueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El tercero es requerido."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(BuildRequest(), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "El tercero es requerido.");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRouteYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateOrdenEmpaqueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(33L));
        var controller = CreateController(mediator, db);
        var request = BuildRequest();

        var result = await controller.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetOrdenEmpaqueById");
        created.RouteValues!["id"].Should().Be(33L);
        AssertAnonymousProperty(created.Value!, "Id", 33L);
        await mediator.Received(1).Send(
            Arg.Is<CreateOrdenEmpaqueCommand>(command =>
                command.TerceroId == request.TerceroId &&
                command.SucursalTerceroId == request.SucursalTerceroId &&
                command.VendedorId == request.VendedorId &&
                command.DepositoId == request.DepositoId &&
                command.TransportistaId == request.TransportistaId &&
                command.AgenteId == request.AgenteId &&
                command.TipoComprobanteId == request.TipoComprobanteId &&
                command.PuntoFacturacionId == request.PuntoFacturacionId &&
                command.MonedaId == request.MonedaId &&
                command.Cotizacion == request.Cotizacion &&
                command.Fecha == request.Fecha &&
                command.FechaEmbarque == request.FechaEmbarque &&
                command.FechaVencimiento == request.FechaVencimiento &&
                command.OrigenObservacion == request.OrigenObservacion &&
                command.DestinoObservacion == request.DestinoObservacion &&
                command.Total == request.Total &&
                command.Observacion == request.Observacion &&
                command.Detalles.Count == 2 &&
                command.Detalles[0].ItemId == 7 &&
                command.Detalles[0].Descripcion == "Caja exportacion" &&
                command.Detalles[0].Cantidad == 2m &&
                command.Detalles[0].PrecioUnitario == 50m &&
                command.Detalles[0].PorcentajeIva == 21m &&
                command.Detalles[0].Observacion == "principal" &&
                command.Detalles[1].ItemId == null &&
                command.Detalles[1].Descripcion == "Servicio adicional" &&
                command.Detalles[1].Cantidad == 1m &&
                command.Detalles[1].PrecioUnitario == 25m &&
                command.Detalles[1].PorcentajeIva == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirmar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ConfirmOrdenEmpaqueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<OrdenEmpaqueEstadoResult>("Orden de empaque 9 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Confirmar(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Confirmar_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ConfirmOrdenEmpaqueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<OrdenEmpaqueEstadoResult>("La orden de empaque esta anulada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Confirmar(9, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La orden de empaque esta anulada.");
    }

    [Fact]
    public async Task Confirmar_CuandoTieneExito_DevuelveOkConEstado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ConfirmOrdenEmpaqueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new OrdenEmpaqueEstadoResult(9, "CONFIRMADO")));
        var controller = CreateController(mediator, db);

        var result = await controller.Confirmar(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 9L);
        AssertAnonymousProperty(ok.Value!, "estado", "CONFIRMADO");
    }

    [Fact]
    public async Task Anular_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CancelOrdenEmpaqueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<OrdenEmpaqueEstadoResult>("Orden de empaque 14 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Anular(14, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Anular_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CancelOrdenEmpaqueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<OrdenEmpaqueEstadoResult>("La orden de empaque ya esta anulada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Anular(14, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La orden de empaque ya esta anulada.");
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOkConEstado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CancelOrdenEmpaqueCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new OrdenEmpaqueEstadoResult(14, "ANULADO")));
        var controller = CreateController(mediator, db);

        var result = await controller.Anular(14, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 14L);
        AssertAnonymousProperty(ok.Value!, "estado", "ANULADO");
    }

    private static OrdenesEmpaqueController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new OrdenesEmpaqueController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static CrearOrdenEmpaqueRequest BuildRequest()
    {
        return new CrearOrdenEmpaqueRequest(
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            1,
            1.25m,
            new DateOnly(2026, 3, 21),
            new DateOnly(2026, 3, 25),
            new DateOnly(2026, 4, 1),
            "origen",
            "destino",
            125m,
            "observacion",
            [
                new OrdenEmpaqueDetalleRequest(7, "Caja exportacion", 2m, 50m, 21m, "principal"),
                new OrdenEmpaqueDetalleRequest(null, "Servicio adicional", 1m, 25m, null, null)
            ]);
    }

    private static OrdenEmpaque BuildOrdenEmpaque(long id, long terceroId, string estado, bool anulada, DateOnly fecha, decimal total)
    {
        var entity = OrdenEmpaque.Crear(
            terceroId,
            20,
            30,
            40,
            50,
            60,
            70,
            80,
            1,
            1.5m,
            fecha,
            fecha.AddDays(2),
            fecha.AddDays(7),
            "origen",
            "destino",
            total,
            "observacion");
        typeof(OrdenEmpaque).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (anulada)
        {
            entity.Anular();
        }
        else if (estado == "CONFIRMADO")
        {
            entity.Confirmar();
        }

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}