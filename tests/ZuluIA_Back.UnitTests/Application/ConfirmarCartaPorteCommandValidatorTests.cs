using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ConfirmarCartaPorteCommandValidatorTests
{
    private readonly ConfirmarCartaPorteCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var cmd = new ConfirmarCartaPorteCommand(1, DateOnly.FromDateTime(DateTime.Today), null);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
