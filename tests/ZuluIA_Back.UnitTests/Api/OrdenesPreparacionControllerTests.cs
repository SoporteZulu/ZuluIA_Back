using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Api;

public class OrdenesPreparacionControllerTests
{
    [Fact]
    public async Task GetPaged_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<OrdenPreparacionListDto>(
            [new OrdenPreparacionListDto { Id = 1, SucursalId = 3, TerceroId = 7, Fecha = new DateOnly(2026, 3, 1), Estado = EstadoOrdenPreparacion.Pendiente, CantidadRenglones = 2 }],
            2,
            15,
            30);
        mediator.Send(Arg.Any<GetOrdenesPreparacionPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);
        var desde = new DateOnly(2026, 3, 1);
        var hasta = new DateOnly(2026, 3, 31);

        var result = await controller.GetPaged(2, 15, 3, 7, EstadoOrdenPreparacion.Pendiente, desde, hasta, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetOrdenesPreparacionPagedQuery>(query =>
                query.Page == 2 &&
                query.PageSize == 15 &&
                query.SucursalId == 3 &&
                query.TerceroId == 7 &&
                query.Estado == EstadoOrdenPreparacion.Pendiente &&
                query.Desde == desde &&
                query.Hasta == hasta),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetOrdenPreparacionByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<OrdenPreparacionDto>("Orden no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new OrdenPreparacionDto
        {
            Id = 5,
            SucursalId = 3,
            TerceroId = 7,
            Fecha = new DateOnly(2026, 3, 1),
            Estado = EstadoOrdenPreparacion.Completada,
            Detalles = [new OrdenPreparacionDetalleDto(1, 10, 2, 5m, 5m, true, null)]
        };
        mediator.Send(Arg.Any<GetOrdenPreparacionByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(dto));
        var controller = CreateController(mediator);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateOrdenPreparacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Debe incluir al menos un detalle."));
        var controller = CreateController(mediator);
        var command = new CreateOrdenPreparacionCommand(3, null, 7, new DateOnly(2026, 3, 1), null, []);

        var result = await controller.Create(command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateOrdenPreparacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator);
        var command = new CreateOrdenPreparacionCommand(3, null, 7, new DateOnly(2026, 3, 1), null, [new CreateOrdenPreparacionDetalleDto(10, 2, 5m, null)]);

        var result = await controller.Create(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetOrdenPreparacionById");
        AssertAnonymousProperty(created.Value!, "id", 12L);
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirmar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ConfirmarOrdenPreparacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Confirmar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ConfirmarOrdenPreparacionCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirmar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ConfirmarOrdenPreparacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Orden no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Confirmar(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularOrdenPreparacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Orden no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularOrdenPreparacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AnularOrdenPreparacionCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static OrdenesPreparacionController CreateController(IMediator mediator)
    {
        var reporteExportacionService = new ZuluIA_Back.Application.Features.Reportes.Services.ReporteExportacionService();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(ZuluIA_Back.Application.Features.Reportes.Services.ReporteExportacionService)).Returns(reporteExportacionService);

        return new OrdenesPreparacionController(
            mediator,
            Substitute.For<IApplicationDbContext>(),
            serviceProvider)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}