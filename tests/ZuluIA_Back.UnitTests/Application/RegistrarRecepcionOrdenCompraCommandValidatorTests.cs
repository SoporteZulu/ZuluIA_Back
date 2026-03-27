using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Compras.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarRecepcionOrdenCompraCommandValidatorTests
{
    private readonly RegistrarRecepcionOrdenCompraCommandValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_NoDebeTenerErrores()
    {
        var cmd = new RegistrarRecepcionOrdenCompraCommand(1, DateOnly.FromDateTime(DateTime.Today), 3m, 2, true, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_CantidadCero_DebeTenerError()
    {
        var cmd = new RegistrarRecepcionOrdenCompraCommand(1, DateOnly.FromDateTime(DateTime.Today), 0m, null, false, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CantidadRecibida);
    }
}
