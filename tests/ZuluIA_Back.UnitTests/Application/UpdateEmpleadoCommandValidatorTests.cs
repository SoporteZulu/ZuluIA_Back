using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.RRHH.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class UpdateEmpleadoCommandValidatorTests
{
    private readonly UpdateEmpleadoCommandValidator _validator = new();

    [Fact]
    public void Validar_SueldoNegativo_DebeTenerError()
    {
        var cmd = new UpdateEmpleadoCommand(1, "Analista", null, -1m, 1);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.SueldoBasico);
    }
}
