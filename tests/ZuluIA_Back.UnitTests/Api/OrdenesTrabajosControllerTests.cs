using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Produccion.Commands;
using ZuluIA_Back.Application.Features.Produccion.DTOs;
using ZuluIA_Back.Application.Features.Produccion.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Api;

public class OrdenesTrabajosControllerTests
{
    [Fact]
    public async Task GetAll_DevuelveOkYMandaQueryConEstadoParseado()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        var paged = new PagedResult<OrdenTrabajoDto>(
            [new OrdenTrabajoDto { Id = 5, SucursalId = 10, FormulaId = 20, Estado = "PENDIENTE" }],
            2,
            10,
            15);
        mediator.Send(Arg.Any<GetOrdenesTrabajoPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetAll(2, 10, 10, 20, "EnProceso", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetOrdenesTrabajoPagedQuery>(query =>
                query.Page == 2 &&
                query.PageSize == 10 &&
                query.SucursalId == 10 &&
                query.FormulaId == 20 &&
                query.Estado == EstadoOrdenTrabajo.EnProceso &&
                query.Desde == new DateOnly(2026, 3, 1) &&
                query.Hasta == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        repo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((OrdenTrabajo?)null);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConPayload()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        var ot = BuildOrdenTrabajo(5, 10, 20, 30, 40, new DateOnly(2026, 3, 21), new DateOnly(2026, 3, 31), 12m, "obs", EstadoOrdenTrabajo.EnProceso);
        repo.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(ot);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "SucursalId", 10L);
        AssertAnonymousProperty(ok.Value!, "Estado", "ENPROCESO");
        AssertAnonymousProperty(ok.Value!, "Observacion", "obs");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<CrearOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La cantidad a producir debe ser mayor a 0."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Create(new CrearOrdenTrabajoCommand(10, 20, 30, 40, new DateOnly(2026, 3, 21), null, 0m, "obs"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<CrearOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, repo);

        var result = await controller.Create(new CrearOrdenTrabajoCommand(10, 20, 30, 40, new DateOnly(2026, 3, 21), null, 12m, "obs"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetOrdenTrabajoById");
        AssertAnonymousProperty(created.Value!, "id", 15L);
    }

    [Fact]
    public async Task Iniciar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<IniciarOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro la OT."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Iniciar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Iniciar_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<IniciarOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede iniciar una OT en estado Finalizada."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Iniciar(8, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Iniciar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<IniciarOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo);

        var result = await controller.Iniciar(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value!.ToString().Should().Contain("iniciada correctamente");
    }

    [Fact]
    public async Task Finalizar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<FinalizarOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede finalizar una OT en estado Pendiente."));
        var controller = CreateController(mediator, repo);
        var fechaFin = new DateOnly(2026, 3, 25);

        var result = await controller.Finalizar(8, new FinalizarOtRequest(fechaFin), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<FinalizarOrdenTrabajoCommand>(command => command.Id == 8 && command.FechaFinReal == fechaFin),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Finalizar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<FinalizarOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo);
        var fechaFin = new DateOnly(2026, 3, 31);

        var result = await controller.Finalizar(8, new FinalizarOtRequest(fechaFin), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<FinalizarOrdenTrabajoCommand>(command => command.Id == 8 && command.FechaFinReal == fechaFin),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancelar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<CancelarOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontro la OT."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Cancelar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cancelar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        mediator.Send(Arg.Any<CancelarOrdenTrabajoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo);

        var result = await controller.Cancelar(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value!.ToString().Should().Contain("cancelada correctamente");
    }

    [Fact]
    public void GetEstados_DevuelveCatalogoEsperado()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IOrdenTrabajoRepository>();
        var controller = CreateController(mediator, repo);

        var result = controller.GetEstados();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var estados = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        estados.Should().HaveCount(4);
        AssertAnonymousProperty(estados[0], "valor", "PENDIENTE");
        AssertAnonymousProperty(estados[1], "descripcion", "En Proceso");
        AssertAnonymousProperty(estados[3], "valor", "CANCELADA");
    }

    private static OrdenesTrabajosController CreateController(IMediator mediator, IOrdenTrabajoRepository repo)
    {
        return new OrdenesTrabajosController(mediator, repo)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static OrdenTrabajo BuildOrdenTrabajo(long id, long sucursalId, long formulaId, long depositoOrigenId, long depositoDestinoId, DateOnly fecha, DateOnly? fechaFinPrevista, decimal cantidad, string? observacion, EstadoOrdenTrabajo estado)
    {
        var entity = OrdenTrabajo.Crear(sucursalId, formulaId, depositoOrigenId, depositoDestinoId, fecha, fechaFinPrevista, cantidad, observacion, null);
        typeof(OrdenTrabajo).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (estado == EstadoOrdenTrabajo.EnProceso)
            entity.Iniciar(null);
        else if (estado == EstadoOrdenTrabajo.Finalizada)
        {
            entity.Iniciar(null);
            entity.Finalizar(fechaFinPrevista ?? fecha, null);
        }
        else if (estado == EstadoOrdenTrabajo.Cancelada)
            entity.Cancelar(null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}