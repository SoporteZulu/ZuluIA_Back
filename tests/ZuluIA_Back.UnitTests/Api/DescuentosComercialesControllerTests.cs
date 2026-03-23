using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;
using ZuluIA_Back.Application.Features.DescuentosComerciales.DTOs;
using ZuluIA_Back.Application.Features.DescuentosComerciales.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class DescuentosComercialesControllerTests
{
    [Fact]
    public async Task Get_DevuelveOkYMandaFiltrosCorrectos()
    {
        var mediator = Substitute.For<IMediator>();
        IReadOnlyList<DescuentoComercialDto> data =
        [
            new()
            {
                Id = 1,
                TerceroId = 10,
                ItemId = 20,
                FechaDesde = new DateOnly(2026, 3, 1),
                FechaHasta = new DateOnly(2026, 3, 31),
                Porcentaje = 15m
            }
        ];
        mediator.Send(Arg.Any<GetDescuentosComercialesQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator);
        var vigenteEn = new DateOnly(2026, 3, 21);

        var result = await controller.Get(10, 20, vigenteEn, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetDescuentosComercialesQuery>(query =>
                query.TerceroId == 10 &&
                query.ItemId == 20 &&
                query.VigenteEn == vigenteEn),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Get_CuandoNoHayFiltros_EnviaQueryConValoresNulos()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetDescuentosComercialesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<DescuentoComercialDto>());
        var controller = CreateController(mediator);

        var result = await controller.Get(null, null, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<GetDescuentosComercialesQuery>(query =>
                query.TerceroId == null &&
                query.ItemId == null &&
                query.VigenteEn == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateDescuentoComercialCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El porcentaje debe ser mayor a cero."));
        var controller = CreateController(mediator);
        var command = new CreateDescuentoComercialCommand(10, 20, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 0m);

        var result = await controller.Create(command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateDescuentoComercialCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(11L));
        var controller = CreateController(mediator);
        var command = new CreateDescuentoComercialCommand(10, 20, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 12m);

        var result = await controller.Create(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be("api/descuentos-comerciales/11");
        AssertAnonymousProperty(created.Value!, "id", 11L);
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequestYSinLlamarMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        var command = new UpdateDescuentoComercialCommand(99, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 15m);

        var result = await controller.Update(12, command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.DidNotReceiveWithAnyArgs().Send(default(UpdateDescuentoComercialCommand)!, default);
    }

    [Fact]
    public async Task Update_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateDescuentoComercialCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Descuento no encontrado."));
        var controller = CreateController(mediator);
        var command = new UpdateDescuentoComercialCommand(12, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 15m);

        var result = await controller.Update(12, command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateDescuentoComercialCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);
        var command = new UpdateDescuentoComercialCommand(12, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 15m);

        var result = await controller.Update(12, command, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteDescuentoComercialCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Descuento no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.Delete(13, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteDescuentoComercialCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Delete(13, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteDescuentoComercialCommand>(command => command.Id == 13),
            Arg.Any<CancellationToken>());
    }

    private static DescuentosComercialesController CreateController(IMediator mediator)
    {
        return new DescuentosComercialesController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}