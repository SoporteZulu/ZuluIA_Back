using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Contratos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RenovarContratoCommandValidatorTests
{
    private readonly RenovarContratoCommandValidator _validator = new();

    [Fact]
    public void Validar_IdInvalido_DebeTenerError()
    {
        var cmd = new RenovarContratoCommand(0, new DateOnly(2025, 2, 28), 120m, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
