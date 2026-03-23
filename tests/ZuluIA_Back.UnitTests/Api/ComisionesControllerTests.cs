using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Comisiones.Commands;
using ZuluIA_Back.Application.Features.Comisiones.DTOs;
using ZuluIA_Back.Application.Features.Comisiones.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class ComisionesControllerTests
{
    [Fact]
    public async Task GetPaged_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<ComisionVendedorListDto>(
            [
                new ComisionVendedorListDto(8, 3, 10, 202603, 1000m, 5m, 50m, "Pendiente")
            ],
            2,
            15,
            21);
        mediator.Send(Arg.Any<GetComisionesVendedorQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetPaged(3, 10, 202603, "Pendiente", 2, 15, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PagedResult<ComisionVendedorListDto>>().Subject;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(15);
        payload.TotalCount.Should().Be(21);
        payload.Items.Should().ContainSingle().Which.Id.Should().Be(8);

        await mediator.Received(1).Send(
            Arg.Is<GetComisionesVendedorQuery>(q =>
                q.SucursalId == 3 &&
                q.VendedorId == 10 &&
                q.Periodo == 202603 &&
                q.Estado == "Pendiente" &&
                q.Page == 2 &&
                q.PageSize == 15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetComisionDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((ComisionVendedorDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetComisionDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ComisionVendedorDto(9, 3, 10, 202603, 1000m, 5m, 50m, "Pendiente", new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), null));
        var controller = CreateController(mediator);

        var result = await controller.GetById(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ComisionVendedorDto>().Which.Id.Should().Be(9);
    }

    [Fact]
    public async Task Registrar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarComisionVendedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una comisión para este vendedor en el período indicado."));
        var controller = CreateController(mediator);

        var result = await controller.Registrar(BuildRegistrarComisionVendedorCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe una comisión");
    }

    [Fact]
    public async Task Registrar_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarComisionVendedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.Registrar(BuildRegistrarComisionVendedorCommand(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(21L);
    }

    [Fact]
    public async Task Aprobar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AprobarComisionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Comisión no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Aprobar(7, 55, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Comisión no encontrada");
    }

    [Fact]
    public async Task Aprobar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AprobarComisionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Aprobar(7, 55, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularComisionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Comisión no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, 55, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Comisión no encontrada");
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularComisionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, 55, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static ComisionesController CreateController(IMediator mediator)
    {
        var controller = new ComisionesController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static RegistrarComisionVendedorCommand BuildRegistrarComisionVendedorCommand()
    {
        return new RegistrarComisionVendedorCommand(3, 10, 202603, 1000m, 5m, "Comisión marzo", 55);
    }
}