using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.RRHH.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateLiquidacionSueldoCommandValidatorTests
{
    private readonly CreateLiquidacionSueldoCommandValidator _validator = new();

    [Fact]
    public void Validar_MesInvalido_DebeTenerError()
    {
        var cmd = new CreateLiquidacionSueldoCommand(1, 1, 2025, 13, 100m, 120m, 20m, 1, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Mes);
    }
}
