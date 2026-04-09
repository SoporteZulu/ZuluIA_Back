using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Runtime.Serialization;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprobantesControllerTests
{
    [Fact]
    public async Task Emitir_CuandoRecibeContratoMinimo_CompletaDefaultsYMapeaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<EmitirComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(33L));
        var db = Substitute.For<IApplicationDbContext>();
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet([CrearMoneda(7, true)]));
        var controller = CreateController(mediator, db);

        var result = await controller.Emitir(
            new EmitirComprobanteCompatRequest(
                null,
                3,
                null,
                22,
                new DateOnly(2026, 4, 25),
                null,
                new DateOnly(2026, 5, 10),
                99,
                null,
                null,
                null,
                "Factura de compra",
                [new EmitirComprobanteCompatItemRequest(5, "INSUMO", 2m, null, 1000, null, 5m, 1, null, null)],
                null,
                null,
                null),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        AssertAnonymousProperty(created.Value!, "id", 33L);
        await mediator.Received(1).Send(
            Arg.Is<EmitirComprobanteCommand>(x =>
                x.SucursalId == 3 &&
                x.TipoComprobanteId == 22 &&
                x.TerceroId == 99 &&
                x.MonedaId == 7 &&
                x.Cotizacion == 1m &&
                x.Percepciones == 0m &&
                x.FechaVencimiento == new DateOnly(2026, 5, 10) &&
                x.Items.Count == 1 &&
                x.Items[0].ItemId == 5 &&
                x.Items[0].Descripcion == "INSUMO" &&
                x.Items[0].DescuentoPct == 5m &&
                x.Items[0].Orden == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Emitir_CuandoNoHayMonedaActiva_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([]));
        var controller = CreateController(mediator, db);

        var result = await controller.Emitir(
            new EmitirComprobanteCompatRequest(
                null,
                3,
                null,
                22,
                new DateOnly(2026, 4, 25),
                null,
                null,
                99,
                null,
                null,
                null,
                null,
                [new EmitirComprobanteCompatItemRequest(5, null, 2m, null, 1000, null, null, 1, null, null)],
                null,
                null,
                null),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("moneda activa");
        await mediator.DidNotReceive().Send(Arg.Any<EmitirComprobanteCommand>(), Arg.Any<CancellationToken>());
    }

    private static ComprobantesController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        var controller = new ComprobantesController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Moneda CrearMoneda(long id, bool activa)
    {
        var moneda = (Moneda)FormatterServices.GetUninitializedObject(typeof(Moneda));
        SetProperty(moneda, nameof(Moneda.Id), id);
        SetProperty(moneda, nameof(Moneda.Activa), activa);
        SetProperty(moneda, nameof(Moneda.Codigo), "ARS");
        SetProperty(moneda, nameof(Moneda.Descripcion), "Peso");
        SetProperty(moneda, nameof(Moneda.Simbolo), "$");
        return moneda;
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
