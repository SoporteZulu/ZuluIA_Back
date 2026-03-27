using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Colegio.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CancelarDeudaColegioCommandValidatorTests
{
    private readonly CancelarDeudaColegioCommandValidator _validator = new();

    [Fact]
    public void Validar_SinCedulones_DebeTenerError()
    {
        var cmd = new CancelarDeudaColegioCommand(1, 1, new DateOnly(2025, 1, 1), 1, 1, 1, 1m, null, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Cedulones);
    }
}
