using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprobantesControllerOperativosTests
{
    [Fact]
    public async Task GetTipos_CuandoFiltraSoloVentas_DevuelveActivosOrdenados()
    {
        var db = BuildDb(
            tiposComprobante:
            [
                BuildTipoComprobante(1, "FCB", "Factura B", true, true, false),
                BuildTipoComprobante(2, "FCA", "Factura A", true, true, true),
                BuildTipoComprobante(3, "NCI", "Nota de Credito Interna", false, true, false),
                BuildTipoComprobante(4, "OCP", "Orden de Compra", true, false, false)
            ]);
        var controller = CreateController(Substitute.For<IMediator>(), db);

        var result = await controller.GetTipos(true, false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "FCA");
        AssertAnonymousProperty(items[1], "Codigo", "FCB");
    }

    [Fact]
    public async Task GetSaldoPendiente_CuandoFiltraPorTerceroYSucursal_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSaldoPendienteTerceroQuery>(), Arg.Any<CancellationToken>())
            .Returns([
                new SaldoPendienteDto
                {
                    ComprobanteId = 10,
                    NumeroFormateado = "FAC-0001-00000010",
                    TipoComprobante = "Factura A",
                    Fecha = new DateOnly(2026, 4, 1),
                    FechaVencimiento = new DateOnly(2026, 4, 15),
                    Total = 1500m,
                    Saldo = 500m,
                    Vencido = false
                }
            ]);
        var controller = CreateController(mediator);

        var result = await controller.GetSaldoPendiente(25, 3, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<SaldoPendienteDto>>().Subject.Should().ContainSingle();
        await mediator.Received(1).Send(
            Arg.Is<GetSaldoPendienteTerceroQuery>(query => query.TerceroId == 25 && query.SucursalId == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSaldoPendiente_CuandoNoRecibeSucursal_PropagaNullEnLaQuery()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSaldoPendienteTerceroQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<SaldoPendienteDto>());
        var controller = CreateController(mediator);

        var result = await controller.GetSaldoPendiente(41, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<GetSaldoPendienteTerceroQuery>(query => query.TerceroId == 41 && query.SucursalId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImputarMasivo_CuandoTieneExito_DevuelveOkConCantidadEIds()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ImputarComprobantesMasivosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<long>>([101L, 102L]));
        var controller = CreateController(mediator);
        var command = new ImputarComprobantesMasivosCommand(
            new DateOnly(2026, 4, 10),
            [
                new ImputacionMasivaItemDto(1, 2, 100m),
                new ImputacionMasivaItemDto(3, 4, 50m)
            ]);

        var result = await controller.ImputarMasivo(command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "imputacionesCreadas", 2);
        AssertAnonymousProperty(ok.Value!, "ids", new long[] { 101L, 102L });
        await mediator.Received(1).Send(
            Arg.Is<ImputarComprobantesMasivosCommand>(sent =>
                sent.Fecha == command.Fecha &&
                sent.Items.Count == 2 &&
                sent.Items[0].ComprobanteOrigenId == 1 &&
                sent.Items[1].ComprobanteDestinoId == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImputarMasivo_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ImputarComprobantesMasivosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<long>>("Los importes deben ser mayores a cero."));
        var controller = CreateController(mediator);

        var result = await controller.ImputarMasivo(
            new ImputarComprobantesMasivosCommand(new DateOnly(2026, 4, 10), [new ImputacionMasivaItemDto(1, 2, 0m)]),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("mayores a cero");
    }

    [Fact]
    public async Task GetEstadisticas_CuandoHayComprobantesValidos_AgrupaYTotalizaPorTipo()
    {
        var db = BuildDb(
            comprobantes:
            [
                BuildComprobante(1, 5, 10, new DateOnly(2026, 4, 1), EstadoComprobante.Emitido, 100m, 0m, 21m, 0m, 121m, 21m),
                BuildComprobante(2, 5, 10, new DateOnly(2026, 4, 5), EstadoComprobante.Pagado, 40m, 0m, 10m, 0m, 50m, 0m),
                BuildComprobante(3, 5, 11, new DateOnly(2026, 4, 7), EstadoComprobante.PagadoParcial, 200m, 0m, 42m, 0m, 242m, 42m),
                BuildComprobante(4, 5, 11, new DateOnly(2026, 4, 8), EstadoComprobante.Borrador, 10m, 0m, 2m, 0m, 12m, 12m),
                BuildComprobante(5, 6, 10, new DateOnly(2026, 4, 9), EstadoComprobante.Emitido, 60m, 0m, 12m, 0m, 72m, 72m)
            ],
            tiposComprobante:
            [
                BuildTipoComprobante(10, "FCA", "Factura A", true, true, true),
                BuildTipoComprobante(11, "NDB", "Nota de Debito B", true, true, false)
            ]);
        var controller = CreateController(Substitute.For<IMediator>(), db);

        var result = await controller.GetEstadisticas(5, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "sucursalId", 5L);
        AssertAnonymousProperty(ok.Value!, "desde", new DateOnly(2026, 4, 1));
        AssertAnonymousProperty(ok.Value!, "hasta", new DateOnly(2026, 4, 30));

        var porTipo = ((IEnumerable)ok.Value!.GetType().GetProperty("porTipo")!.GetValue(ok.Value)!).Cast<object>().ToList();
        porTipo.Should().HaveCount(2);

        var facturaA = porTipo.Single(x => Equals(x.GetType().GetProperty("TipoComprobanteId")!.GetValue(x), 10L));
        AssertAnonymousProperty(facturaA, "TipoComprobanteDescripcion", "Factura A");
        AssertAnonymousProperty(facturaA, "Cantidad", 2);
        AssertAnonymousProperty(facturaA, "TotalNeto", 140m);
        AssertAnonymousProperty(facturaA, "TotalIva", 31m);
        AssertAnonymousProperty(facturaA, "Total", 171m);
        AssertAnonymousProperty(facturaA, "SaldoPendiente", 21m);

        var notaDebito = porTipo.Single(x => Equals(x.GetType().GetProperty("TipoComprobanteId")!.GetValue(x), 11L));
        AssertAnonymousProperty(notaDebito, "TipoComprobanteDescripcion", "Nota de Debito B");
        AssertAnonymousProperty(notaDebito, "Cantidad", 1);
        AssertAnonymousProperty(notaDebito, "TotalNeto", 200m);
        AssertAnonymousProperty(notaDebito, "TotalIva", 42m);
        AssertAnonymousProperty(notaDebito, "Total", 242m);
        AssertAnonymousProperty(notaDebito, "SaldoPendiente", 42m);
    }

    private static ComprobantesController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new ComprobantesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<Comprobante>? comprobantes = null,
        IEnumerable<TipoComprobante>? tiposComprobante = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var comprobantesDbSet = MockDbSetHelper.CreateMockDbSet(comprobantes);
        var tiposComprobanteDbSet = MockDbSetHelper.CreateMockDbSet(tiposComprobante);
        db.Comprobantes.Returns(comprobantesDbSet);
        db.TiposComprobante.Returns(tiposComprobanteDbSet);
        return db;
    }

    private static Comprobante BuildComprobante(
        long id,
        long sucursalId,
        long tipoComprobanteId,
        DateOnly fecha,
        EstadoComprobante estado,
        decimal netoGravado,
        decimal netoNoGravado,
        decimal ivaRi,
        decimal ivaRni,
        decimal total,
        decimal saldo)
    {
        var entity = Comprobante.Crear(sucursalId, 1, tipoComprobanteId, 1, id, fecha, null, 1, 1, 1m, null, null);
        SetProperty(entity, nameof(Comprobante.Id), id);
        SetProperty(entity, nameof(Comprobante.Estado), estado);
        SetProperty(entity, nameof(Comprobante.NetoGravado), netoGravado);
        SetProperty(entity, nameof(Comprobante.NetoNoGravado), netoNoGravado);
        SetProperty(entity, nameof(Comprobante.IvaRi), ivaRi);
        SetProperty(entity, nameof(Comprobante.IvaRni), ivaRni);
        SetProperty(entity, nameof(Comprobante.Total), total);
        SetProperty(entity, nameof(Comprobante.Saldo), saldo);
        return entity;
    }

    private static TipoComprobante BuildTipoComprobante(long id, string codigo, string descripcion, bool activo, bool esVenta, bool afectaStock)
    {
        var entity = CreateInstance<TipoComprobante>();
        SetProperty(entity, nameof(TipoComprobante.Id), id);
        SetProperty(entity, nameof(TipoComprobante.Codigo), codigo);
        SetProperty(entity, nameof(TipoComprobante.Descripcion), descripcion);
        SetProperty(entity, nameof(TipoComprobante.Activo), activo);
        SetProperty(entity, nameof(TipoComprobante.EsVenta), esVenta);
        SetProperty(entity, nameof(TipoComprobante.EsCompra), !esVenta);
        SetProperty(entity, nameof(TipoComprobante.EsInterno), false);
        SetProperty(entity, nameof(TipoComprobante.AfectaStock), afectaStock);
        SetProperty(entity, nameof(TipoComprobante.AfectaCuentaCorriente), true);
        SetProperty(entity, nameof(TipoComprobante.GeneraAsiento), true);
        SetProperty(entity, nameof(TipoComprobante.TipoAfip), (short?)1);
        SetProperty(entity, nameof(TipoComprobante.LetraAfip), 'A');
        return entity;
    }

    private static T CreateInstance<T>() where T : class
    {
        return (T)Activator.CreateInstance(typeof(T), true)!;
    }

    private static void SetProperty(object entity, string propertyName, object? value)
    {
        entity.GetType().GetProperty(propertyName)!.SetValue(entity, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().BeEquivalentTo(expectedValue);
    }
}