using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.PlanesPago.Commands;
using ZuluIA_Back.Application.Features.PlanesPago.DTOs;
using ZuluIA_Back.Application.Features.PlanesPago.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class PlanesPagoControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPlanes()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPlanesPagoQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<PlanPagoDto>
            {
                new() { Id = 1, Descripcion = "Contado", CantidadCuotas = 1, InteresPct = 0m, Activo = true },
                new() { Id = 2, Descripcion = "3 cuotas", CantidadCuotas = 3, InteresPct = 12m, Activo = false }
            });
        var controller = CreateController(mediator);

        var result = await controller.GetAll(false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<IReadOnlyList<PlanPagoDto>>().Subject;
        items.Should().HaveCount(2);
        await mediator.Received(1).Send(Arg.Is<GetPlanesPagoQuery>(q => q.SoloActivos == false), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePlanPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripción es requerida."));
        var controller = CreateController(mediator);

        var result = await controller.Create(new CreatePlanPagoCommand("", 3, 12m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripción es requerida");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePlanPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.Create(new CreatePlanPagoCommand("3 cuotas", 3, 12m), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 21L);
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequestSinInvocarMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.Update(7, new UpdatePlanPagoCommand(8, "3 cuotas", 3, 12m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ID de la URL no coincide");
        await mediator.DidNotReceive().Send(Arg.Any<UpdatePlanPagoCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlanPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el plan de pago con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Update(7, new UpdatePlanPagoCommand(7, "3 cuotas", 3, 12m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el plan de pago con ID 7");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePlanPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Update(7, new UpdatePlanPagoCommand(7, "3 cuotas", 3, 12m), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlanPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el plan de pago con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el plan de pago con ID 7");
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeletePlanPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Activar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivatePlanPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el plan de pago con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el plan de pago con ID 7");
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivatePlanPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivatePlanPagoCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Calcular_CuandoNoExistePlan_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPlanesPagoQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<PlanPagoDto>());
        var controller = CreateController(mediator);

        var result = await controller.Calcular(7, 1000m, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró el plan de pago con ID 7");
    }

    [Fact]
    public async Task Calcular_CuandoTotalEsInvalido_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPlanesPagoQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<PlanPagoDto>
            {
                new() { Id = 7, Descripcion = "3 cuotas", CantidadCuotas = 3, InteresPct = 12m, Activo = true }
            });
        var controller = CreateController(mediator);

        var result = await controller.Calcular(7, 0m, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("total debe ser mayor a 0");
    }

    [Fact]
    public async Task Calcular_CuandoPlanExiste_DevuelveImportePorCuota()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPlanesPagoQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<PlanPagoDto>
            {
                new() { Id = 7, Descripcion = "3 cuotas", CantidadCuotas = 3, InteresPct = 12m, Activo = true }
            });
        var controller = CreateController(mediator);

        var result = await controller.Calcular(7, 1000m, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "planId", 7L);
        AssertAnonymousProperty(ok.Value!, "descripcion", "3 cuotas");
        AssertAnonymousProperty(ok.Value!, "totalOriginal", 1000m);
        AssertAnonymousProperty(ok.Value!, "interesPct", 12m);
        AssertAnonymousProperty(ok.Value!, "totalConInteres", 1120m);
        AssertAnonymousProperty(ok.Value!, "cantidadCuotas", (short)3);
        AssertAnonymousProperty(ok.Value!, "importeCuota", 373.33m);
        await mediator.Received(1).Send(Arg.Is<GetPlanesPagoQuery>(q => q.SoloActivos == false), Arg.Any<CancellationToken>());
    }

    private static PlanesPagoController CreateController(IMediator mediator)
    {
        return new PlanesPagoController(mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}