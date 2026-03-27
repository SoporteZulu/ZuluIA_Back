using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class BaseControllerTests
{
    // -----------------------------------------------------------
    // Concrete subclass for testing protected methods
    // -----------------------------------------------------------

    private sealed class TestableController(IMediator mediator) : BaseController(mediator)
    {
        public IActionResult CallOkOrNotFound<T>(T? value) => OkOrNotFound(value);
        public IActionResult CallFromResult(Result result) => FromResult(result);
        public IActionResult CallFromResultT<T>(Result<T> result) => FromResult(result);
        public IActionResult CallCreatedFromResult<T>(Result<T> result, string routeName, object routeValues)
            => CreatedFromResult(result, routeName, routeValues);
    }

    private static TestableController CreateController()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new TestableController(mediator);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        return controller;
    }

    // -----------------------------------------------------------
    // OkOrNotFound<T>
    // -----------------------------------------------------------

    [Fact]
    public void OkOrNotFound_ValorNoNulo_RetornaOk()
    {
        var controller = CreateController();

        var result = controller.CallOkOrNotFound("valor");

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be("valor");
    }

    [Fact]
    public void OkOrNotFound_ValorNulo_RetornaNotFound()
    {
        var controller = CreateController();

        var result = controller.CallOkOrNotFound<string>(null);

        result.Should().BeOfType<NotFoundResult>();
    }

    // -----------------------------------------------------------
    // FromResult(Result)
    // -----------------------------------------------------------

    [Fact]
    public void FromResult_ResultExitoso_RetornaOk()
    {
        var controller = CreateController();
        var result = Result.Success();

        var actionResult = controller.CallFromResult(result);

        actionResult.Should().BeOfType<OkResult>();
    }

    [Fact]
    public void FromResult_ResultFallido_RetornaBadRequestConError()
    {
        var controller = CreateController();
        var result = Result.Failure("Error de negocio");

        var actionResult = controller.CallFromResult(result);

        var badRequest = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().NotBeNull();
        badRequest.Value!.ToString().Should().Contain("Error de negocio");
    }

    // -----------------------------------------------------------
    // FromResult<T>(Result<T>)
    // -----------------------------------------------------------

    [Fact]
    public void FromResultT_ExitoConValor_RetornaOkConValor()
    {
        var controller = CreateController();
        var result = Result.Success(42);

        var actionResult = controller.CallFromResultT(result);

        actionResult.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().Be(42);
    }

    [Fact]
    public void FromResultT_Fallido_RetornaBadRequestConError()
    {
        var controller = CreateController();
        var result = Result.Failure<int>("No encontrado");

        var actionResult = controller.CallFromResultT(result);

        var badRequest = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value!.ToString().Should().Contain("No encontrado");
    }

    // -----------------------------------------------------------
    // CreatedFromResult<T>
    // -----------------------------------------------------------

    [Fact]
    public void CreatedFromResult_ExitoConValor_RetornaCreatedAtRoute()
    {
        var controller = CreateController();
        var result = Result.Success(99L);

        var actionResult = controller.CallCreatedFromResult(result, "GetById", new { id = 99 });

        actionResult.Should().BeOfType<CreatedAtRouteResult>()
                    .Which.RouteName.Should().Be("GetById");
    }

    [Fact]
    public void CreatedFromResult_Fallido_RetornaBadRequestConError()
    {
        var controller = CreateController();
        var result = Result.Failure<long>("No se pudo crear");

        var actionResult = controller.CallCreatedFromResult(result, "GetById", new { id = 0 });

        var badRequest = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value!.ToString().Should().Contain("No se pudo crear");
    }
}
