using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Diagnosticos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateRegionDiagnosticaCommandValidatorTests
{
    private readonly CreateRegionDiagnosticaCommandValidator _validator = new();

    [Fact]
    public void Validar_CodigoVacio_DebeTenerError()
    {
        var result = _validator.TestValidate(new CreateRegionDiagnosticaCommand(string.Empty, "desc"));
        result.ShouldHaveValidationErrorFor(x => x.Codigo);
    }
}
