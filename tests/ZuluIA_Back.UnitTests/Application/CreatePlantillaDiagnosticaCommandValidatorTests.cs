using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Diagnosticos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreatePlantillaDiagnosticaCommandValidatorTests
{
    private readonly CreatePlantillaDiagnosticaCommandValidator _validator = new();

    [Fact]
    public void Validar_SinVariables_DebeTenerError()
    {
        var cmd = new CreatePlantillaDiagnosticaCommand("PL1", "Plantilla", null, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Variables);
    }
}
