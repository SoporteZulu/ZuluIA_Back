using FluentAssertions;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.UnitTests.Application;

public class CurrentUserServiceTests
{
    [Fact]
    public void SetUser_IdNumericoValido_EstableceUserId()
    {
        var sut = new CurrentUserService();

        sut.SetUser("42", "u@test.com");

        sut.UserId.Should().Be(42L);
    }

    [Fact]
    public void SetUser_IdNoNumerico_DejaUserIdNulo()
    {
        var sut = new CurrentUserService();

        sut.SetUser("no-es-numero", "u@test.com");

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void SetUser_IdNulo_DejaUserIdNulo()
    {
        var sut = new CurrentUserService();

        sut.SetUser(null, "u@test.com");

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void SetUser_EstableceEmail()
    {
        var sut = new CurrentUserService();

        sut.SetUser("1", "usuario@empresa.com");

        sut.Email.Should().Be("usuario@empresa.com");
    }

    [Fact]
    public void SetUser_EmailNulo_DejaEmailNulo()
    {
        var sut = new CurrentUserService();

        sut.SetUser("1", null);

        sut.Email.Should().BeNull();
    }

    [Fact]
    public void IsAuthenticated_ConUserIdEstablecido_EsTrue()
    {
        var sut = new CurrentUserService();
        sut.SetUser("10", null);

        sut.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_SinUserIdEstablecido_EsFalse()
    {
        var sut = new CurrentUserService();

        sut.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_IdNoNumerico_EsFalse()
    {
        var sut = new CurrentUserService();
        sut.SetUser("invalido", "u@test.com");

        sut.IsAuthenticated.Should().BeFalse();
    }
}
