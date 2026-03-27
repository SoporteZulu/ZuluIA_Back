using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.ObjetivosVenta.Commands;
using ZuluIA_Back.Application.Features.ObjetivosVenta.DTOs;
using ZuluIA_Back.Application.Features.ObjetivosVenta.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class ObjetivosVentaControllerTests
{
    [Fact]
    public async Task GetPaged_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<ObjetivoVentaDto>(
            [new ObjetivoVentaDto(1, 3, 7, 202603, 1000m, 250m, 25m, "Marzo", false)],
            2,
            15,
            40);
        mediator.Send(Arg.Any<GetObjetivosVentaQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetPaged(3, 7, 202603, false, 2, 15, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetObjetivosVentaQuery>(query =>
                query.SucursalId == 3 &&
                query.VendedorId == 7 &&
                query.Periodo == 202603 &&
                query.Cerrado == false &&
                query.Page == 2 &&
                query.PageSize == 15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearObjetivoVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un objetivo para este vendedor en el periodo indicado."));
        var controller = CreateController(mediator);

        var result = await controller.Crear(new CrearObjetivoVentaCommand(3, 7, 202603, 1000m, "Marzo", 2), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Crear_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearObjetivoVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator);

        var result = await controller.Crear(new CrearObjetivoVentaCommand(3, 7, 202603, 1000m, "Marzo", 2), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(12L);
    }

    [Fact]
    public async Task Actualizar_UsaIdDeRutaYCuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActualizarObjetivoVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Actualizar(25, new ActualizarObjetivoVentaCommand(1, 1500m, 9), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActualizarObjetivoVentaCommand>(command =>
                command.Id == 25 &&
                command.NuevoMonto == 1500m &&
                command.UserId == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CerrarPeriodo_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarPeriodoObjetivoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Objetivo no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.CerrarPeriodo(30, 5, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CerrarPeriodo_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarPeriodoObjetivoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.CerrarPeriodo(30, 5, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<CerrarPeriodoObjetivoCommand>(command =>
                command.Id == 30 &&
                command.UserId == 5),
            Arg.Any<CancellationToken>());
    }

    private static ObjetivosVentaController CreateController(IMediator mediator)
    {
        return new ObjetivosVentaController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}