using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Api;

public class AuthControllerTests
{
    private static AuthController CreateController(ICurrentUserService currentUserService, IUsuarioRepository? usuarioRepository = null)
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new AuthController(
            mediator,
            currentUserService,
            usuarioRepository ?? Substitute.For<IUsuarioRepository>(),
            Substitute.For<IPasswordHasherService>(),
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build());
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
    public async Task Me_NoAutenticado_Devuelve401()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(false);
        var controller = CreateController(currentUser);

        var result = await controller.Me(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // -----------------------------------------------------------
    // Me() — autenticado → 200 con userId y email
    // -----------------------------------------------------------

    [Fact]
    public async Task Me_Autenticado_DevuelveOkConUserIdYEmail()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        var repo = Substitute.For<IUsuarioRepository>();
        var usuario = Usuario.Crear("ada", "Ada Lovelace", "user@example.com", null, null);
        SetEntityId(usuario, 42);
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(42L);
        repo.GetByIdAsync(42L, Arg.Any<CancellationToken>()).Returns(usuario);
        var controller = CreateController(currentUser, repo);

        var result = await controller.Me(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;
        var type = value.GetType();
        type.GetProperty("Id")!.GetValue(value).Should().Be(42L);
        type.GetProperty("Email")!.GetValue(value).Should().Be("user@example.com");
    }

    [Fact]
    public async Task Me_Autenticado_UserId_EsElCorrecto()
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        var repo = Substitute.For<IUsuarioRepository>();
        var usuario = Usuario.Crear("siete", "Siete", "siete@test.com", null, null);
        SetEntityId(usuario, 7);
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(7L);
        repo.GetByIdAsync(7L, Arg.Any<CancellationToken>()).Returns(usuario);
        var controller = CreateController(currentUser, repo);

        var result = await controller.Me(CancellationToken.None);

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

    private static void SetEntityId(object entity, long id)
        => entity.GetType().BaseType!.GetProperty("Id")!.SetValue(entity, id);
}
