using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class AnularCartaPorteWorkflowCommandValidatorTests
{
    private readonly AnularCartaPorteWorkflowCommandValidator _validator = new();

    [Fact]
    public void Validar_IdInvalido_DebeTenerError()
    {
        var cmd = new AnularCartaPorteWorkflowCommand(0, DateOnly.FromDateTime(DateTime.Today), null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CartaPorteId);
    }
}
