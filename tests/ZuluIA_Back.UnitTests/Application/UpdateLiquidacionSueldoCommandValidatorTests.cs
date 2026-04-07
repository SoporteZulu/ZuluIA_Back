using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.RRHH.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class UpdateLiquidacionSueldoCommandValidatorTests
{
    private readonly UpdateLiquidacionSueldoCommandValidator _validator = new();

    [Fact]
    public void Validar_MonedaInvalida_DebeTenerError()
    {
        var cmd = new UpdateLiquidacionSueldoCommand(1, 100m, 120m, 20m, 0, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.MonedaId);
    }
}
