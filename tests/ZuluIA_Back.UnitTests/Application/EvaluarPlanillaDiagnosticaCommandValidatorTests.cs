using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Diagnosticos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class EvaluarPlanillaDiagnosticaCommandValidatorTests
{
    private readonly EvaluarPlanillaDiagnosticaCommandValidator _validator = new();

    [Fact]
    public void Validar_SinRespuestas_DebeTenerError()
    {
        var cmd = new EvaluarPlanillaDiagnosticaCommand(1, [], null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Respuestas);
    }
}
