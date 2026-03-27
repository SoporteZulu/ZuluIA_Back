using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Tesoreria.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarReintegroTesoreriaCommandValidatorTests
{
    private readonly RegistrarReintegroTesoreriaCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var cmd = new RegistrarReintegroTesoreriaCommand(1, 1, DateOnly.FromDateTime(DateTime.Today), 10m, 1, 1m, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
