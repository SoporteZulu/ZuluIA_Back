using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CartaPorteControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYRetornaPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cartas = MockDbSetHelper.CreateMockDbSet([
            BuildCartaPorte(1, 10, null, "20-1", "30-1", null, new DateOnly(2026, 3, 1), EstadoCartaPorte.Pendiente, "obs1", new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero)),
            BuildCartaPorte(2, 10, "CTG-2", "20-2", "30-2", "40-2", new DateOnly(2026, 3, 2), EstadoCartaPorte.Activa, "obs2", new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero)),
            BuildCartaPorte(3, 11, "CTG-3", "20-3", "30-3", "40-3", new DateOnly(2026, 3, 3), EstadoCartaPorte.Confirmada, "obs3", new DateTimeOffset(2026, 3, 3, 10, 0, 0, TimeSpan.Zero))
        ]);
        db.CartasPorte.Returns(cartas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(1, 20, 10, "Activa", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 2), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "page", 1);
        AssertAnonymousProperty(ok.Value!, "pageSize", 20);
        AssertAnonymousProperty(ok.Value!, "totalCount", 1);
        AssertAnonymousProperty(ok.Value!, "totalPages", 1);
        var items = ok.Value!.GetType().GetProperty("items")!.GetValue(ok.Value)
            .Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        items.Should().HaveCount(1);
        items[0].Should().BeOfType<ZuluIA_Back.Application.Features.Facturacion.DTOs.CartaPorteDto>();
        ((ZuluIA_Back.Application.Features.Facturacion.DTOs.CartaPorteDto)items[0]).Id.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_ConEstadoInvalido_NoFiltraPorEstado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cartas = MockDbSetHelper.CreateMockDbSet([
            BuildCartaPorte(1, 10, null, "20-1", "30-1", null, new DateOnly(2026, 3, 1), EstadoCartaPorte.Pendiente, null, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero)),
            BuildCartaPorte(2, 10, "CTG-2", "20-2", "30-2", "40-2", new DateOnly(2026, 3, 2), EstadoCartaPorte.Activa, null, new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero))
        ]);
        db.CartasPorte.Returns(cartas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(1, 20, 10, "desconocido", null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "totalCount", 2);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cartas = MockDbSetHelper.CreateMockDbSet(new List<CartaPorte>());
        db.CartasPorte.Returns(cartas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cartas = MockDbSetHelper.CreateMockDbSet([
            BuildCartaPorte(2, 10, "CTG-2", "20-2", "30-2", "40-2", new DateOnly(2026, 3, 2), EstadoCartaPorte.Activa, "obs2", new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero))
        ]);
        db.CartasPorte.Returns(cartas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(2, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ZuluIA_Back.Application.Features.Facturacion.DTOs.CartaPorteDto>();
        var dto = (ZuluIA_Back.Application.Features.Facturacion.DTOs.CartaPorteDto)ok.Value!;
        dto.Id.Should().Be(2);
        dto.Estado.Should().Be("ACTIVA");
    }

    [Fact]
    public async Task GetReimpresion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cartas = MockDbSetHelper.CreateMockDbSet(new List<CartaPorte>());
        db.CartasPorte.Returns(cartas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetReimpresion(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetReimpresion_CuandoExiste_DevuelvePayloadDedicado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cartas = MockDbSetHelper.CreateMockDbSet([
            BuildCartaPorte(2, 10, "CTG-2", "20-2", "30-2", "40-2", new DateOnly(2026, 3, 2), EstadoCartaPorte.Activa, "obs2", new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero))
        ]);
        db.CartasPorte.Returns(cartas);
        var controller = CreateController(mediator, db);
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        var result = await controller.GetReimpresion(2, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<CartaPorteReimpresionResponse>();
        var payload = (CartaPorteReimpresionResponse)ok.Value!;
        payload.EsReimpresion.Should().BeTrue();
        payload.GeneradoEn.Should().BeOnOrAfter(before);
        payload.Documento.Should().BeOfType<ZuluIA_Back.Application.Features.Facturacion.DTOs.CartaPorteDto>();
        var dto = (ZuluIA_Back.Application.Features.Facturacion.DTOs.CartaPorteDto)payload.Documento;
        dto.Id.Should().Be(2);
        dto.Estado.Should().Be("ACTIVA");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var command = new CreateCartaPorteCommand(10, "20-1", "30-1", null, new DateOnly(2026, 3, 1), "obs");
        mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El CUIT remitente es obligatorio."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var command = new CreateCartaPorteCommand(10, "20-1", "30-1", null, new DateOnly(2026, 3, 1), "obs");
        mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCartaPorteById");
        AssertAnonymousProperty(created.Value!, "id", 15L);
    }

    [Fact]
    public async Task AsignarCtg_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AsignarCtgCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Solo se puede asignar CTG a cartas de porte pendientes."));
        var controller = CreateController(mediator, db);

        var result = await controller.AsignarCtg(7, new AsignarCtgRequest("CTG-1"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AsignarCtg_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AsignarCtgCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.AsignarCtg(7, new AsignarCtgRequest("CTG-1"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AsignarCtgCommand>(command => command.CartaPorteId == 7 && command.NroCtg == "CTG-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirmar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ConfirmarCartaPorteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro la carta de porte."));
        var controller = CreateController(mediator, db);

        var result = await controller.Confirmar(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Confirmar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ConfirmarCartaPorteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Confirmar(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Carta de porte confirmada correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<ConfirmarCartaPorteCommand>(command => command.CartaPorteId == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Anular_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AnularCartaPorteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La carta de porte ya está anulada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Anular(7, new AnularCartaPorteRequest("obs"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AnularCartaPorteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Anular(7, new AnularCartaPorteRequest("obs"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "mensaje", "Carta de porte anulada correctamente.");
        await mediator.Received(1).Send(
            Arg.Is<AnularCartaPorteCommand>(command => command.CartaPorteId == 7 && command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    private static CartaPorteController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new CartaPorteController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static CartaPorte BuildCartaPorte(long id, long? comprobanteId, string? nroCtg, string cuitRemitente, string cuitDestinatario, string? cuitTransportista, DateOnly fechaEmision, EstadoCartaPorte estado, string? observacion, DateTimeOffset createdAt)
    {
        var entity = CartaPorte.Crear(comprobanteId, cuitRemitente, cuitDestinatario, cuitTransportista, fechaEmision, observacion, 1);
        entity.GetType().GetProperty(nameof(CartaPorte.Id))!.SetValue(entity, id);
        entity.GetType().GetProperty(nameof(CartaPorte.CreatedAt))!.SetValue(entity, createdAt);
        entity.GetType().GetProperty(nameof(CartaPorte.UpdatedAt))!.SetValue(entity, createdAt);

        if (estado == EstadoCartaPorte.Activa || estado == EstadoCartaPorte.Confirmada)
            entity.AsignarCtg(nroCtg ?? $"CTG-{id}", 1);
        if (estado == EstadoCartaPorte.Confirmada)
            entity.Confirmar(1);
        if (estado == EstadoCartaPorte.Anulada)
            entity.Anular(observacion, 1);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}