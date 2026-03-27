using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;
using ZuluIA_Back.Api.Middleware;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.UnitTests.Api;

public class CurrentUserMiddlewareTests
{
    // -----------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------

    private static (CurrentUserMiddleware middleware, DefaultHttpContext context, CurrentUserService userService)
        Build(bool isAuthenticated, IEnumerable<Claim>? claims = null)
    {
        RequestDelegate next = _ => Task.CompletedTask;

        var middleware = new CurrentUserMiddleware(next);
        var userService = new CurrentUserService();

        var context = new DefaultHttpContext();

        if (isAuthenticated)
        {
            var identity = new ClaimsIdentity(claims ?? Enumerable.Empty<Claim>(), "TestAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        return (middleware, context, userService);
    }

    // -----------------------------------------------------------
    // Siempre llama al siguiente middleware
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_SiempreLlamaSiguienteMiddleware_Autenticado()
    {
        var nextCalled = false;
        var middleware = new CurrentUserMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        var userService = new CurrentUserService();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth");
        context.User = new ClaimsPrincipal(identity);

        await middleware.InvokeAsync(context, userService);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SiempreLlamaSiguienteMiddleware_NoAutenticado()
    {
        var nextCalled = false;
        var middleware = new CurrentUserMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        var userService = new CurrentUserService();

        await middleware.InvokeAsync(context, userService);

        nextCalled.Should().BeTrue();
    }

    // -----------------------------------------------------------
    // No autenticado — SetUser no se llama
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_NoAutenticado_UserServiceSinDatos()
    {
        var (middleware, context, userService) = Build(isAuthenticated: false);

        await middleware.InvokeAsync(context, userService);

        userService.UserId.Should().BeNull();
        userService.Email.Should().BeNull();
        userService.IsAuthenticated.Should().BeFalse();
    }

    // -----------------------------------------------------------
    // Autenticado con ClaimTypes.NameIdentifier
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_Autenticado_EstablecreUserIdDesdeNameIdentifier()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "99"),
            new Claim(ClaimTypes.Email, "user@test.com")
        };
        var (middleware, context, userService) = Build(isAuthenticated: true, claims);

        await middleware.InvokeAsync(context, userService);

        userService.UserId.Should().Be(99);
        userService.Email.Should().Be("user@test.com");
        userService.IsAuthenticated.Should().BeTrue();
    }

    // -----------------------------------------------------------
    // Autenticado con claim "sub" cuando no hay NameIdentifier
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_Autenticado_EstableceUserIdDesdeSub_SiNoHayNameIdentifier()
    {
        var claims = new[]
        {
            new Claim("sub", "77"),
            new Claim(ClaimTypes.Email, "sub@test.com")
        };
        var (middleware, context, userService) = Build(isAuthenticated: true, claims);

        await middleware.InvokeAsync(context, userService);

        userService.UserId.Should().Be(77);
        userService.Email.Should().Be("sub@test.com");
    }

    // -----------------------------------------------------------
    // Autenticado con claim "email" cuando no hay ClaimTypes.Email
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_Autenticado_EstableceEmailDeClaimEmail_SiNoHayClaimTypesEmail()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "5"),
            new Claim("email", "fallback@test.com")
        };
        var (middleware, context, userService) = Build(isAuthenticated: true, claims);

        await middleware.InvokeAsync(context, userService);

        userService.Email.Should().Be("fallback@test.com");
    }

    // -----------------------------------------------------------
    // Autenticado pero sin claims de ID — UserId queda nulo
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_Autenticado_SinClaimsDeId_UserIdNulo()
    {
        var claims = new[] { new Claim(ClaimTypes.Email, "only@email.com") };
        var (middleware, context, userService) = Build(isAuthenticated: true, claims);

        await middleware.InvokeAsync(context, userService);

        userService.UserId.Should().BeNull();
        userService.Email.Should().Be("only@email.com");
    }

    // -----------------------------------------------------------
    // ICurrentUserService que no es CurrentUserService concreto — no lanza
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_ServiceNoEsCurrentUserServiceConcreto_NoLanza()
    {
        var nextCalled = false;
        var middleware = new CurrentUserMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth");
        context.User = new ClaimsPrincipal(identity);

        // Using a different implementation (not the concrete CurrentUserService)
        var otherService = new OtherCurrentUserService();

        var act = async () => await middleware.InvokeAsync(context, otherService);

        await act.Should().NotThrowAsync();
        nextCalled.Should().BeTrue();
    }

    // -----------------------------------------------------------
    // Stub for alternative ICurrentUserService implementation
    // -----------------------------------------------------------

    private sealed class OtherCurrentUserService : ICurrentUserService
    {
        public long? UserId => null;
        public string? Email => null;
        public bool IsAuthenticated => false;
    }
}
