using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class OrdenesCompraControllerTests
{
    [Fact]
    public async Task GetAll_CuandoNoFiltra_DevuelveOrdenadasPorCreatedAtDesc()
    {
        var controller = CreateController(db: BuildDb(
            BuildOrdenes(
                BuildOrdenCompra(1, 10, 100, new DateOnly(2026, 3, 20), "Entrega 1", EstadoOrdenCompra.Pendiente, true, new DateTimeOffset(2026, 3, 20, 9, 0, 0, TimeSpan.Zero)),
                BuildOrdenCompra(2, 11, 101, new DateOnly(2026, 3, 22), "Entrega 2", EstadoOrdenCompra.Cancelada, false, new DateTimeOffset(2026, 3, 21, 9, 0, 0, TimeSpan.Zero)),
                BuildOrdenCompra(3, 12, 102, new DateOnly(2026, 3, 21), "Entrega 3", EstadoOrdenCompra.Recibida, true, new DateTimeOffset(2026, 3, 19, 9, 0, 0, TimeSpan.Zero)))));

        var result = await controller.GetAll(null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(3);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[1], "Id", 1L);
        AssertAnonymousProperty(items[2], "Id", 3L);
    }

    [Fact]
    public async Task GetAll_CuandoFiltraPorProveedorEstadoYHabilitada_DevuelveCoincidencias()
    {
        var controller = CreateController(db: BuildDb(
            BuildOrdenes(
                BuildOrdenCompra(1, 10, 10, new DateOnly(2026, 3, 20), "Entrega 1", EstadoOrdenCompra.Pendiente, true, DateTimeOffset.UtcNow),
                BuildOrdenCompra(2, 10, 10, new DateOnly(2026, 3, 22), "Entrega 2", EstadoOrdenCompra.Recibida, true, DateTimeOffset.UtcNow.AddMinutes(-1)),
                BuildOrdenCompra(3, 11, 102, new DateOnly(2026, 3, 21), "Entrega 3", EstadoOrdenCompra.Recibida, false, DateTimeOffset.UtcNow.AddMinutes(-2)))));

        var result = await controller.GetAll(10, "recibida", true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().ContainSingle();
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "EstadoOc", "RECIBIDA");
        AssertAnonymousProperty(items[0], "Habilitada", true);
    }

    [Fact]
    public async Task GetAll_CuandoEstadoEsInvalido_NoFiltraPorEstado()
    {
        var controller = CreateController(db: BuildDb(
            BuildOrdenes(
                BuildOrdenCompra(1, 10, 10, new DateOnly(2026, 3, 20), "Entrega 1", EstadoOrdenCompra.Pendiente, true, DateTimeOffset.UtcNow),
                BuildOrdenCompra(2, 10, 10, new DateOnly(2026, 3, 22), "Entrega 2", EstadoOrdenCompra.Recibida, true, DateTimeOffset.UtcNow.AddMinutes(-1)))));

        var result = await controller.GetAll(10, "desconocido", null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_CuandoFiltraPendientes_IncluyeParcialmenteRecibidasYNormalizaEstado()
    {
        var controller = CreateController(db: BuildDb(
            BuildOrdenes(
                BuildOrdenCompra(1, 10, 10, new DateOnly(2026, 3, 20), "Entrega 1", EstadoOrdenCompra.Pendiente, true, DateTimeOffset.UtcNow),
                BuildOrdenCompra(2, 10, 10, new DateOnly(2026, 3, 22), "Entrega 2", EstadoOrdenCompra.ParcialmenteRecibida, true, DateTimeOffset.UtcNow.AddMinutes(-1), 8m, 3m, new DateOnly(2026, 3, 21)),
                BuildOrdenCompra(3, 11, 10, new DateOnly(2026, 3, 21), "Entrega 3", EstadoOrdenCompra.Recibida, true, DateTimeOffset.UtcNow.AddMinutes(-2)))));

        var result = await controller.GetAll(10, "pendiente", null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        var parcial = items.Single(x => Equals(x.GetType().GetProperty("Id")!.GetValue(x), 2L));
        var pendiente = items.Single(x => Equals(x.GetType().GetProperty("Id")!.GetValue(x), 1L));
        AssertAnonymousProperty(parcial, "EstadoOc", "PENDIENTE");
        AssertAnonymousProperty(parcial, "EstadoOperativo", "PARCIALMENTERECIBIDA");
        AssertAnonymousProperty(parcial, "RecepcionParcial", true);
        AssertAnonymousProperty(pendiente, "EstadoOc", "PENDIENTE");
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(db: BuildDb(BuildOrdenes()));

        var result = await controller.GetById(77, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConProjection()
    {
        var controller = CreateController(db: BuildDb(
            BuildOrdenes(BuildOrdenCompra(7, 10, 100, new DateOnly(2026, 3, 20), "Entrega express", EstadoOrdenCompra.Pendiente, true, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero)))));

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "ProveedorId", 100L);
        AssertAnonymousProperty(ok.Value!, "EstadoOc", "PENDIENTE");
    }

    [Fact]
    public async Task GetById_CuandoEstaParcialmenteRecibida_ExponeEstadoCompatibleYEstadoOperativo()
    {
        var controller = CreateController(db: BuildDb(
            BuildOrdenes(BuildOrdenCompra(7, 10, 100, new DateOnly(2026, 3, 20), "Entrega express", EstadoOrdenCompra.ParcialmenteRecibida, true, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), 10m, 4m, new DateOnly(2026, 3, 22)))));

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "EstadoOc", "PENDIENTE");
        AssertAnonymousProperty(ok.Value!, "EstadoOperativo", "PARCIALMENTERECIBIDA");
        AssertAnonymousProperty(ok.Value!, "RecepcionParcial", true);
        AssertAnonymousProperty(ok.Value!, "CantidadRecibida", 4m);
        AssertAnonymousProperty(ok.Value!, "SaldoPendiente", 6m);
    }

    [Fact]
    public async Task Crear_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearOrdenCompraDesdeComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Crear(
            new CrearOrdenCompraCompatRequest(15, 9, new DateOnly(2026, 4, 20), "Entrega pactada"),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(OrdenesCompraController.GetById));
        AssertAnonymousProperty(created.Value!, "id", 21L);
        await mediator.Received(1).Send(
            Arg.Is<CrearOrdenCompraDesdeComprobanteCommand>(x =>
                x.ComprobanteId == 15 &&
                x.ProveedorId == 9 &&
                x.FechaEntregaReq == new DateOnly(2026, 4, 20) &&
                x.CondicionesEntrega == "Entrega pactada"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Recibir_CuandoNoSeEncuentra_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator: mediator, db: BuildDb(BuildOrdenes()));

        var result = await controller.Recibir(7, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontro la OC con ID 7");
    }

    [Fact]
    public async Task Recibir_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(
            mediator: mediator,
            db: BuildDb(BuildOrdenes(
                BuildOrdenCompra(7, 10, 100, new DateOnly(2026, 3, 20), "Entrega express", EstadoOrdenCompra.Recibida, true, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), 1m, 1m, null))));

        var result = await controller.Recibir(7, null, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("recibida completamente");
    }

    [Fact]
    public async Task Recibir_CuandoTieneExito_DevuelveOkConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarRecepcionOrdenCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(7L));
        var controller = CreateController(
            mediator: mediator,
            db: BuildDb(
                BuildOrdenes(BuildOrdenCompra(7, 10, 100, new DateOnly(2026, 3, 20), "Entrega express", EstadoOrdenCompra.Pendiente, true, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), 4m, 1.5m, null)),
                tipos: [BuildTipoComprobante(12, "REMITO-COMPRA", "Remito compra", true, true)]));

        var result = await controller.Recibir(7, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Orden de compra marcada como recibida.");
        await mediator.Received(1).Send(
            Arg.Is<RegistrarRecepcionOrdenCompraCommand>(x =>
                x.OrdenCompraId == 7 &&
                x.CantidadRecibida == 2.5m &&
                x.TipoComprobanteRemitoId == 12 &&
                x.RemitoValorizado == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Recibir_CuandoNoHayTipoRemito_Default_RegistraRecepcionSinTipo()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarRecepcionOrdenCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(7L));
        var controller = CreateController(
            mediator: mediator,
            db: BuildDb(
                BuildOrdenes(BuildOrdenCompra(7, 10, 100, new DateOnly(2026, 3, 20), "Entrega express", EstadoOrdenCompra.Pendiente, true, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), 4m, 1m, null))));

        var result = await controller.Recibir(7, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<RegistrarRecepcionOrdenCompraCommand>(x =>
                x.OrdenCompraId == 7 &&
                x.CantidadRecibida == 3m &&
                x.TipoComprobanteRemitoId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Recibir_CuandoTienePayloadDetallado_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarRecepcionOrdenCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(55L));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Recibir(
            7,
            new RecibirOrdenCompraCompatRequest(new DateOnly(2026, 4, 21), 2.5m, 12, true, "Parcial"),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 55L);
        await mediator.Received(1).Send(
            Arg.Is<RegistrarRecepcionOrdenCompraCommand>(x =>
                x.OrdenCompraId == 7 &&
                x.FechaRecepcion == new DateOnly(2026, 4, 21) &&
                x.CantidadRecibida == 2.5m &&
                x.TipoComprobanteRemitoId == 12 &&
                x.RemitoValorizado &&
                x.Observacion == "Parcial"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancelar_CuandoNoSeEncuentra_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelarOrdenCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Orden no se encontro."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Cancelar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("no se encontro");
    }

    [Fact]
    public async Task Cancelar_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelarOrdenCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La orden ya está cancelada."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Cancelar(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya está cancelada");
    }

    [Fact]
    public async Task Cancelar_CuandoTieneExito_DevuelveOkConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelarOrdenCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator: mediator);

        var result = await controller.Cancelar(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Orden de compra cancelada.");
    }

    private static OrdenesCompraController CreateController(IMediator? mediator = null, IApplicationDbContext? db = null)
    {
        var controller = new OrdenesCompraController(mediator ?? Substitute.For<IMediator>(), db ?? BuildDb(BuildOrdenes()))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(IEnumerable<OrdenCompraMeta> ordenes, IEnumerable<TipoComprobante>? tipos = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var ordenesDbSet = MockDbSetHelper.CreateMockDbSet(ordenes);
        db.OrdenesCompraMeta.Returns(ordenesDbSet);
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet(tipos ?? []));
        return db;
    }

    private static IEnumerable<OrdenCompraMeta> BuildOrdenes(params OrdenCompraMeta[] ordenes)
    {
        return ordenes;
    }

    private static OrdenCompraMeta BuildOrdenCompra(long id, long comprobanteId, long proveedorId, DateOnly? fechaEntregaReq, string? condicionesEntrega, EstadoOrdenCompra estado, bool habilitada, DateTimeOffset createdAt, decimal cantidadTotal = 1m, decimal cantidadRecibida = 0m, DateOnly? fechaUltimaRecepcion = null)
    {
        var entity = OrdenCompraMeta.Crear(comprobanteId, proveedorId, fechaEntregaReq, condicionesEntrega, cantidadTotal);
        SetProperty(entity, nameof(OrdenCompraMeta.Id), id);
        SetProperty(entity, nameof(OrdenCompraMeta.EstadoOc), estado);
        SetProperty(entity, nameof(OrdenCompraMeta.Habilitada), habilitada);
        SetProperty(entity, nameof(OrdenCompraMeta.CantidadRecibida), cantidadRecibida);
        SetProperty(entity, nameof(OrdenCompraMeta.FechaUltimaRecepcion), fechaUltimaRecepcion);
        SetProperty(entity, nameof(OrdenCompraMeta.CreatedAt), createdAt);
        return entity;
    }

    private static TipoComprobante BuildTipoComprobante(long id, string codigo, string descripcion, bool esCompra, bool activo)
    {
        var entity = (TipoComprobante)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TipoComprobante));
        SetProperty(entity, nameof(TipoComprobante.Id), id);
        SetProperty(entity, nameof(TipoComprobante.Codigo), codigo);
        SetProperty(entity, nameof(TipoComprobante.Descripcion), descripcion);
        SetProperty(entity, nameof(TipoComprobante.EsCompra), esCompra);
        SetProperty(entity, nameof(TipoComprobante.Activo), activo);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}