using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Contratos.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateContratoCommandValidatorTests
{
    private readonly CreateContratoCommandValidator _validator = new();

    [Fact]
    public void Validar_CodigoVacio_DebeTenerError()
    {
        var cmd = new CreateContratoCommand(1, 1, 1, string.Empty, "Contrato", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), 100m, false, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Codigo);
    }
}
