using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Application.Features.Compras.DTOs;
using ZuluIA_Back.Application.Features.Compras.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class RequisicionesCompraControllerTests
{
    [Fact]
    public async Task GetAll_EnviaQueryCorrectaYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<RequisicionCompraListDto>(
        [
            new RequisicionCompraListDto { Id = 5, SucursalId = 10, SolicitanteId = 20, Descripcion = "Req", Estado = "Borrador", CantidadItems = 2 }
        ],
            2,
            25,
            30);
        mediator.Send(Arg.Any<GetRequisicionesCompraPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = new RequisicionesCompraController(mediator);

        var result = await controller.GetAll(2, 25, 10, 20, "Borrador", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetRequisicionesCompraPagedQuery>(q =>
                q.Page == 2 &&
                q.PageSize == 25 &&
                q.SucursalId == 10 &&
                q.SolicitanteId == 20 &&
                q.Estado == "Borrador"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetRequisicionCompraDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((RequisicionCompraDto?)null);
        var controller = new RequisicionesCompraController(mediator);

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new RequisicionCompraDto
        {
            Id = 9,
            SucursalId = 10,
            SolicitanteId = 20,
            Descripcion = "Req detalle",
            Estado = "Enviada",
            Items = [new RequisicionCompraItemDto { Id = 1, Descripcion = "Item", Cantidad = 3, UnidadMedida = "u" }]
        };
        mediator.Send(Arg.Any<GetRequisicionCompraDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = new RequisicionesCompraController(mediator);

        var result = await controller.GetById(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<GetRequisicionCompraDetalleQuery>(q => q.Id == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La requisicion es invalida."));
        var controller = new RequisicionesCompraController(mediator);
        var command = BuildCommand();

        var result = await controller.Crear(command, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La requisicion es invalida.");
    }

    [Fact]
    public async Task Crear_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(14L));
        var controller = new RequisicionesCompraController(mediator);
        var command = BuildCommand();

        var result = await controller.Crear(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetRequisicionCompraById");
        created.RouteValues!["id"].Should().Be(14L);
        AssertAnonymousProperty(created.Value!, "id", 14L);
    }

    [Fact]
    public async Task Enviar_CuandoTieneExito_DevuelveOkYEnviaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<EnviarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = new RequisicionesCompraController(mediator);

        var result = await controller.Enviar(11, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<EnviarRequisicionCompraCommand>(c => c.Id == 11),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Aprobar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AprobarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede aprobar."));
        var controller = new RequisicionesCompraController(mediator);

        var result = await controller.Aprobar(11, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "No se puede aprobar.");
    }

    [Fact]
    public async Task Aprobar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AprobarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = new RequisicionesCompraController(mediator);

        var result = await controller.Aprobar(11, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AprobarRequisicionCompraCommand>(c => c.Id == 11),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rechazar_EnviaMotivoYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RechazarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = new RequisicionesCompraController(mediator);

        var result = await controller.Rechazar(11, "Sin presupuesto", CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<RechazarRequisicionCompraCommand>(c => c.Id == 11 && c.Motivo == "Sin presupuesto"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancelar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede cancelar."));
        var controller = new RequisicionesCompraController(mediator);

        var result = await controller.Cancelar(11, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "No se puede cancelar.");
    }

    private static CrearRequisicionCompraCommand BuildCommand()
    {
        return new CrearRequisicionCompraCommand(
            10,
            20,
            new DateOnly(2026, 3, 21),
            "Req compra",
            "Obs",
            [new CrearRequisicionItemDto(100, "Item", 2, "u", null)]);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object? expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}