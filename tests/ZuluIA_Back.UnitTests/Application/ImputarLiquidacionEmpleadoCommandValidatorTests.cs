using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.RRHH.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ImputarLiquidacionEmpleadoCommandValidatorTests
{
    private readonly ImputarLiquidacionEmpleadoCommandValidator _validator = new();

    [Fact]
    public void Validar_ImporteCero_DebeTenerError()
    {
        var cmd = new ImputarLiquidacionEmpleadoCommand(1, 1, new DateOnly(2025, 1, 31), 0m, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Importe);
    }
}
