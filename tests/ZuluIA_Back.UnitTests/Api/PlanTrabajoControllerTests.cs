using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.PlanTrabajo.Commands;
using ZuluIA_Back.Application.Features.PlanTrabajo.DTOs;
using ZuluIA_Back.Application.Features.PlanTrabajo.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class PlanTrabajoControllerTests
{
    [Fact]
    public async Task GetPaged_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<PlanTrabajoListDto>(
            [new PlanTrabajoListDto(1, "Plan Marzo", 3, 202603, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), "Abierto")],
            2,
            15,
            25);
        mediator.Send(Arg.Any<GetPlanesTrabajoPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetPaged(3, 202603, "Abierto", 2, 15, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetPlanesTrabajoPagedQuery>(query =>
                query.SucursalId == 3 &&
                query.Periodo == 202603 &&
                query.Estado == "Abierto" &&
                query.Page == 2 &&
                query.PageSize == 15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPlanTrabajoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((PlanTrabajoDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new PlanTrabajoDto(9, "Plan Marzo", 3, 202603, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), "Abierto", "Plan mensual", []);
        mediator.Send(Arg.Any<GetPlanTrabajoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.GetById(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task GetEvaluacion_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new EvaluacionFranquiciaDto(4, 1, 3, 202603, 90m, new DateOnly(2026, 3, 20), "Ok", [new EvaluacionDetalleDto(1, 11, 100m, 90m, null)]);
        mediator.Send(Arg.Any<GetEvaluacionDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.GetEvaluacion(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task GetEvaluacion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetEvaluacionDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((EvaluacionFranquiciaDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetEvaluacion(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Crear_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearPlanTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Nombre requerido."));
        var controller = CreateController(mediator);

        var result = await controller.Crear(new CrearPlanTrabajoCommand("", 3, 202603, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), null, 2), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Crear_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearPlanTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(18L));
        var controller = CreateController(mediator);
        var command = new CrearPlanTrabajoCommand("Plan Marzo", 3, 202603, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), null, 2);

        var result = await controller.Crear(command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(18L);
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AgregarKpi_UsaIdDeRutaYCuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AgregarKpiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator);

        var result = await controller.AgregarKpi(8, new AgregarKpiCommand(1, 4, 5, "Ventas", 100m, 20m, 9), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(12L);
        await mediator.Received(1).Send(
            Arg.Is<AgregarKpiCommand>(command =>
                command.PlanTrabajoId == 8 &&
                command.AspectoId == 4 &&
                command.VariableId == 5 &&
                command.Descripcion == "Ventas"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AgregarKpi_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AgregarKpiCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Aspecto requerido."));
        var controller = CreateController(mediator);

        var result = await controller.AgregarKpi(8, new AgregarKpiCommand(1, 4, 5, "Ventas", 100m, 20m, 9), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RegistrarEvaluacion_UsaIdDeRutaYCuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarEvaluacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator);
        var command = new RegistrarEvaluacionCommand(1, 3, 202603, new DateOnly(2026, 3, 20), "Ok", [new DetalleEvaluacionInput(11, 100m, 90m, null)], 9);

        var result = await controller.RegistrarEvaluacion(10, command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(15L);
        await mediator.Received(1).Send(
            Arg.Is<RegistrarEvaluacionCommand>(item => item.PlanTrabajoId == 10 && item.SucursalId == 3 && item.Detalles.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegistrarEvaluacion_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarEvaluacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Detalle requerido."));
        var controller = CreateController(mediator);
        var command = new RegistrarEvaluacionCommand(1, 3, 202603, new DateOnly(2026, 3, 20), "Ok", [new DetalleEvaluacionInput(11, 100m, 90m, null)], 9);

        var result = await controller.RegistrarEvaluacion(10, command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cerrar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarPlanTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Plan de trabajo no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.Cerrar(11, 9, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cerrar_CuandoTieneExito_DevuelveOkYUsaParametros()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarPlanTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Cerrar(11, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<CerrarPlanTrabajoCommand>(command => command.Id == 11 && command.UserId == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularPlanTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Anular(12, 7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AnularPlanTrabajoCommand>(command => command.Id == 12 && command.UserId == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularPlanTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Plan de trabajo no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(12, 7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private static PlanTrabajoController CreateController(IMediator mediator)
    {
        return new PlanTrabajoController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}