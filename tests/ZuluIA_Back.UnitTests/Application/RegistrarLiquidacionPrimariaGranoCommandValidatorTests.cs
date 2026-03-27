using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Fiscal.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarLiquidacionPrimariaGranoCommandValidatorTests
{
    private readonly RegistrarLiquidacionPrimariaGranoCommandValidator _validator = new();

    [Fact]
    public void Validar_CantidadCero_DebeTenerError()
    {
        var cmd = new RegistrarLiquidacionPrimariaGranoCommand(1, DateOnly.FromDateTime(DateTime.Today), "LPG-1", "Maíz", 0m, 10m, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Cantidad);
    }
}
