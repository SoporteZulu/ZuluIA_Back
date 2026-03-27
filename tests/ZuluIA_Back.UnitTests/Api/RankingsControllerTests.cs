using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Rankings.Queries;

namespace ZuluIA_Back.UnitTests.Api;

public class RankingsControllerTests
{
    [Fact]
    public async Task Clientes_DevuelveOkConResultadoYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        List<RankingClienteDto> data = [new(10, 1500m, 3)];
        mediator.Send(Arg.Any<GetRankingClientesQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator);
        var desde = new DateOnly(2026, 3, 1);
        var hasta = new DateOnly(2026, 3, 21);

        var result = await controller.Clientes(4, desde, hasta, 7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetRankingClientesQuery>(query =>
                query.SucursalId == 4 &&
                query.Desde == desde &&
                query.Hasta == hasta &&
                query.Top == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Items_DevuelveOkConResultadoYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        List<RankingItemDto> data = [new(20, 2200m, 8)];
        mediator.Send(Arg.Any<GetRankingItemsQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator);
        var desde = new DateOnly(2026, 2, 1);
        var hasta = new DateOnly(2026, 2, 28);

        var result = await controller.Items(5, desde, hasta, 9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetRankingItemsQuery>(query =>
                query.SucursalId == 5 &&
                query.Desde == desde &&
                query.Hasta == hasta &&
                query.Top == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnalisisMensual_DevuelveOkConResultadoYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        List<AnalisisMensualDto> data = [new(2026, 3, 3100m, 12)];
        mediator.Send(Arg.Any<GetAnalisisMensualQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator);

        var result = await controller.AnalisisMensual(6, 2026, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetAnalisisMensualQuery>(query =>
                query.SucursalId == 6 &&
                query.Anio == 2026),
            Arg.Any<CancellationToken>());
    }

    private static RankingsController CreateController(IMediator mediator)
    {
        return new RankingsController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}