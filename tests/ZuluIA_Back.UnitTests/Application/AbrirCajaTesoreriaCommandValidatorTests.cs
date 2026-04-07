using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Tesoreria.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class AbrirCajaTesoreriaCommandValidatorTests
{
    private readonly AbrirCajaTesoreriaCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var result = _validator.TestValidate(new AbrirCajaTesoreriaCommand(1, DateOnly.FromDateTime(DateTime.Today), 0m, null));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
