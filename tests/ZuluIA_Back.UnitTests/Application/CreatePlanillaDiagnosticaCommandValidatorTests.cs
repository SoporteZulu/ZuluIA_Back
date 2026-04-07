using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Diagnosticos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreatePlanillaDiagnosticaCommandValidatorTests
{
    private readonly CreatePlanillaDiagnosticaCommandValidator _validator = new();

    [Fact]
    public void Validar_NombreVacio_DebeTenerError()
    {
        var cmd = new CreatePlanillaDiagnosticaCommand(1, string.Empty, DateOnly.FromDateTime(DateTime.Today), null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }
}
