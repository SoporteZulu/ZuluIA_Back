using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_DebeCrearResultadoExitoso()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_DebeCrearResultadoFallido()
    {
        var result = Result.Failure("Error de prueba");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Error de prueba");
    }

    [Fact]
    public void SuccessGeneric_DebeCrearResultadoConValor()
    {
        var result = Result.Success(42L);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42L);
    }

    [Fact]
    public void FailureGeneric_DebeCrearResultadoFallido()
    {
        var result = Result.Failure<long>("Error");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Value_EnResultadoFallido_DebeLanzarExcepcion()
    {
        var result = Result.Failure<long>("Error");

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_DebeCrearResultadoExitoso()
    {
        Result<string> result = "valor";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("valor");
    }
}