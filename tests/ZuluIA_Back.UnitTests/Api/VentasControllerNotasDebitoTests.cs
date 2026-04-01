using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Api;

public class VentasControllerNotasDebitoTests
{
    [Fact]
    public async Task CrearNotaDebito_CuandoTieneExito_EnviaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<RegistrarNotaDebitoVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(77L));
        var controller = new VentasController(mediator, db);
        var request = new CreateNotaDebitoVentaRequest(
            SucursalId: 1,
            PuntoFacturacionId: 2,
            TipoComprobanteId: 3,
            Fecha: new DateOnly(2026, 3, 31),
            FechaVencimiento: new DateOnly(2026, 4, 15),
            TerceroId: 10,
            MonedaId: 1,
            Cotizacion: 1m,
            Percepciones: 0m,
            Observacion: "ND de prueba",
            ComprobanteOrigenId: 55,
            MotivoDebitoId: 8,
            MotivoDebitoObservacion: "Ajuste comercial",
            Items:
            [
                new ComprobanteItemInput(
                    ItemId: 100,
                    Descripcion: "Servicio",
                    Cantidad: 1m,
                    CantidadBonificada: 0,
                    PrecioUnitario: 1200,
                    DescuentoPct: 0m,
                    AlicuotaIvaId: 5,
                    DepositoId: null,
                    Orden: 0)
            ],
            ListaPreciosId: 4,
            VendedorId: 9,
            CanalVentaId: 11,
            CondicionPagoId: 12,
            PlazoDias: 30,
            Emitir: true);

        var result = await controller.CrearNotaDebito(request, CancellationToken.None);

        result.Should().BeOfType<CreatedAtRouteResult>();
        await mediator.Received(1).Send(
            Arg.Is<RegistrarNotaDebitoVentaCommand>(x =>
                x.SucursalId == 1 &&
                x.PuntoFacturacionId == 2 &&
                x.TipoComprobanteId == 3 &&
                x.TerceroId == 10 &&
                x.MonedaId == 1 &&
                x.ComprobanteOrigenId == 55 &&
                x.MotivoDebitoId == 8 &&
                x.Percepciones == 0m &&
                x.Items.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMotivosDebito_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        IReadOnlyList<MotivoDebitoDto> payload =
        [
            new MotivoDebitoDto
            {
                Id = 1,
                Codigo = "ND-AJ",
                Descripcion = "Ajuste de precio",
                EsFiscal = false,
                RequiereDocumentoOrigen = true,
                AfectaCuentaCorriente = true,
                Activo = true
            }
        ];
        mediator.Send(Arg.Any<ZuluIA_Back.Application.Features.Ventas.Queries.GetMotivosDebitoQuery>(), Arg.Any<CancellationToken>())
            .Returns(payload);
        var controller = new VentasController(mediator, db);

        var result = await controller.GetMotivosDebito(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(payload);
        await mediator.Received(1).Send(
            Arg.Is<ZuluIA_Back.Application.Features.Ventas.Queries.GetMotivosDebitoQuery>(x => x.SoloActivos),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EmitirNotaDebito_SinRequest_EmiteConDebitoCuentaCorriente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<EmitirDocumentoVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(44L));
        var controller = new VentasController(mediator, db);

        var result = await controller.EmitirNotaDebito(44, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<EmitirDocumentoVentaCommand>(x =>
                x.ComprobanteId == 44 &&
                x.OperacionStock == OperacionStockVenta.Ninguna &&
                x.OperacionCuentaCorriente == OperacionCuentaCorrienteVenta.Debito),
            Arg.Any<CancellationToken>());
    }
}
