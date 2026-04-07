using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Diagnosticos.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateVariableDiagnosticaCommandValidatorTests
{
    private readonly CreateVariableDiagnosticaCommandValidator _validator = new();

    [Fact]
    public void Validar_VariableOpcionSinOpciones_DebeTenerError()
    {
        var cmd = new CreateVariableDiagnosticaCommand(1, "VAR", "Variable", TipoVariableDiagnostica.Opcion, true, 1m, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Opciones);
    }
}
