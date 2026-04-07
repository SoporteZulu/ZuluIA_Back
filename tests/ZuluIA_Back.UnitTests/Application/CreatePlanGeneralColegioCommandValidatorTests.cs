using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Colegio.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreatePlanGeneralColegioCommandValidatorTests
{
    private readonly CreatePlanGeneralColegioCommandValidator _validator = new();

    [Fact]
    public void Validar_ImporteBaseCero_DebeTenerError()
    {
        var cmd = new CreatePlanGeneralColegioCommand(1, 1, 1, 1, 1, "PG1", "Plan", 0m, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ImporteBase);
    }
}
