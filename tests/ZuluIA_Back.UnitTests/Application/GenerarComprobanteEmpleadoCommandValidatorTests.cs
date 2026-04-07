using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.RRHH.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class GenerarComprobanteEmpleadoCommandValidatorTests
{
    private readonly GenerarComprobanteEmpleadoCommandValidator _validator = new();

    [Fact]
    public void Validar_IdInvalido_DebeTenerError()
    {
        var cmd = new GenerarComprobanteEmpleadoCommand(0, new DateOnly(2025, 1, 31));
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.LiquidacionSueldoId);
    }
}
