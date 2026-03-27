using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprobantesControllerFormasPagoTests
{
    [Fact]
    public async Task GetFormasPago_CuandoHayDatos_DevuelveOkConSoloItemsDelComprobante()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var formasPago = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildFormaPago(1, 7, 11, new DateOnly(2026, 3, 20), 1500m, true, 1, 1m),
            BuildFormaPago(2, 7, 12, new DateOnly(2026, 3, 21), 500m, false, 2, 3.5m),
            BuildFormaPago(3, 8, 13, new DateOnly(2026, 3, 22), 900m, true, null, 1m)
        });
        db.ComprobantesFormasPago.Returns(formasPago);
        var controller = CreateController(mediator, db);

        var result = await controller.GetFormasPago(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "FormaPagoId", 11L);
        AssertAnonymousProperty(items[0], "Valido", true);
        AssertAnonymousProperty(items[1], "Id", 2L);
        AssertAnonymousProperty(items[1], "Cotizacion", 3.5m);
    }

    [Fact]
    public async Task AddFormaPago_CuandoFallaPorValidacion_DevuelveBadRequestConError()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddComprobanteFormaPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El importe debe ser mayor a cero."));
        var controller = CreateController(mediator);

        var result = await controller.AddFormaPago(7, BuildRequest(importe: 0m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("importe debe ser mayor a cero");
    }

    [Fact]
    public async Task AddFormaPago_CuandoTieneExito_DevuelveCreatedConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddComprobanteFormaPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.AddFormaPago(7, BuildRequest(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().BeEmpty();
        AssertAnonymousProperty(created.Value!, "Id", 21L);
    }

    [Fact]
    public async Task AnularFormaPago_CuandoNoExiste_DevuelveNotFoundConMensajeNormalizado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularComprobanteFormaPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Forma de pago 9 no encontrada en comprobante 7."));
        var controller = CreateController(mediator);

        var result = await controller.AnularFormaPago(7, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Forma de pago 9 no encontrada en comprobante 7");
    }

    [Fact]
    public async Task AnularFormaPago_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularComprobanteFormaPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.AnularFormaPago(7, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static ComprobantesController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new ComprobantesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static AddFormaPagoRequest BuildRequest(
        long formaPagoId = 11,
        decimal importe = 1500m,
        decimal cotizacion = 1m)
        => new(
            formaPagoId,
            new DateOnly(2026, 3, 20),
            importe,
            "Transferencia",
            "Acredita en 24hs",
            1,
            cotizacion);

    private static ComprobanteFormaPago BuildFormaPago(
        long id,
        long comprobanteId,
        long formaPagoId,
        DateOnly fecha,
        decimal importe,
        bool valido,
        long? monedaId,
        decimal cotizacion)
    {
        var entity = ComprobanteFormaPago.Crear(
            comprobanteId,
            formaPagoId,
            fecha,
            importe,
            "Transferencia",
            "Acredita en 24hs",
            monedaId,
            cotizacion);
        if (!valido)
            entity.Anular();
        typeof(ComprobanteFormaPago).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}