using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Contratos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class FinalizarContratosVencidosCommandValidatorTests
{
    private readonly FinalizarContratosVencidosCommandValidator _validator = new();

    [Fact]
    public void Validar_SucursalInvalida_DebeTenerError()
    {
        var cmd = new FinalizarContratosVencidosCommand(0, new DateOnly(2025, 1, 1));
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.SucursalId);
    }
}
