using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Infrastructure.Services;

namespace ZuluIA_Back.UnitTests.Infrastructure;

public class HttpCurrentUserServiceTests
{
    private static HttpCurrentUserService BuildService(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext { User = user };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);
        return new HttpCurrentUserService(accessor);
    }

    private static ClaimsPrincipal Principal(IEnumerable<Claim> claims, bool authenticated = true) =>
        new(new ClaimsIdentity(claims, authenticated ? "Bearer" : null));

    [Fact]
    public void UserId_ConNameIdentifierClaim_RetornaId()
    {
        var sut = BuildService(Principal([new Claim(ClaimTypes.NameIdentifier, "42")]));

        sut.UserId.Should().Be(42L);
    }

    [Fact]
    public void UserId_ConSubClaim_RetornaId()
    {
        var sut = BuildService(Principal([new Claim("sub", "99")]));

        sut.UserId.Should().Be(99L);
    }

    [Fact]
    public void UserId_SinClaim_RetornaNull()
    {
        var sut = BuildService(Principal([]));

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_ClaimNoNumerico_RetornaNull()
    {
        var sut = BuildService(Principal([new Claim(ClaimTypes.NameIdentifier, "no-es-numero")]));

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void Email_ConEmailClaim_RetornaEmail()
    {
        var sut = BuildService(Principal([new Claim(ClaimTypes.Email, "usuario@prueba.com")]));

        sut.Email.Should().Be("usuario@prueba.com");
    }

    [Fact]
    public void Email_SinClaim_RetornaNull()
    {
        var sut = BuildService(Principal([]));

        sut.Email.Should().BeNull();
    }

    [Fact]
    public void IsAuthenticated_ConIdentidadAutenticada_RetornaTrue()
    {
        var sut = BuildService(Principal([], authenticated: true));

        sut.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_SinContextoHttp_RetornaFalse()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var sut = new HttpCurrentUserService(accessor);

        sut.IsAuthenticated.Should().BeFalse();
    }
}
