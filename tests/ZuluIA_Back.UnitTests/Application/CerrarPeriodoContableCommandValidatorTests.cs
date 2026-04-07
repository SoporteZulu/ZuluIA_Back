using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Fiscal.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CerrarPeriodoContableCommandValidatorTests
{
    private readonly RegistrarCierrePeriodoContableCommandValidator _validator = new();

    [Fact]
    public void Validar_EjercicioInvalido_DebeTenerError()
    {
        var cmd = new RegistrarCierrePeriodoContableCommand(0, null, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.EjercicioId);
    }
}
