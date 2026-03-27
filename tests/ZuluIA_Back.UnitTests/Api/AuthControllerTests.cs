using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.UnitTests.Api;

public class AuthControllerTests
{
    private static AuthController CreateController(ICurrentUserService currentUserService)
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new AuthController(mediator, currentUserService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    // -----------------------------------------------------------
    // Me() — no autenticado → 401
    // -----------------------------------------------------------

    [Fact]
    public void Me_NoAutenticado_Devuelve401()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(false);
        var controller = CreateController(currentUser);

        var result = controller.Me();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // -----------------------------------------------------------
    // Me() — autenticado → 200 con userId y email
    // -----------------------------------------------------------

    [Fact]
    public void Me_Autenticado_DevuelveOkConUserIdYEmail()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(42L);
        currentUser.Email.Returns("user@example.com");
        var controller = CreateController(currentUser);

        var result = controller.Me();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;
        var type = value.GetType();
        type.GetProperty("userId")!.GetValue(value).Should().Be(42L);
        type.GetProperty("email")!.GetValue(value).Should().Be("user@example.com");
    }

    [Fact]
    public void Me_Autenticado_UserId_EsElCorrecto()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(7L);
        currentUser.Email.Returns("siete@test.com");
        var controller = CreateController(currentUser);

        var result = controller.Me();

        result.Should().BeOfType<OkObjectResult>();
    }

    // -----------------------------------------------------------
    // Ping() → siempre 200 con mensaje
    // -----------------------------------------------------------

    [Fact]
    public void Ping_DevuelveOkConMensaje()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        var controller = CreateController(currentUser);

        var result = controller.Ping();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;
        var type = value.GetType();
        var message = type.GetProperty("message")!.GetValue(value)!.ToString();
        message.Should().Contain("ZuluIA_Back API funcionando");
    }

    [Fact]
    public void Ping_DevuelveTimestamp_Reciente()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        var controller = CreateController(currentUser);

        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = controller.Ping();
        var after = DateTime.UtcNow.AddSeconds(1);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;
        var type = value.GetType();
        var timestamp = (DateTime)type.GetProperty("timestamp")!.GetValue(value)!;
        timestamp.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void Ping_NoRequiereAutenticacion_FuncionaSinUsuario()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(false);
        var controller = CreateController(currentUser);

        var act = () => controller.Ping();

        act.Should().NotThrow();
    }
}
