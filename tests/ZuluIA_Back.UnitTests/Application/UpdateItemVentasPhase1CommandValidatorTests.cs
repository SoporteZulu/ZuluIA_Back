using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Items.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class UpdateItemVentasPhase1CommandValidatorTests
{
    [Fact]
    public void Validar_UpdateItemConfiguracionVentasCommand_ConDescuentoFueraDeRango_DebeHaveError()
    {
        var validator = new UpdateItemConfiguracionVentasCommandValidator();
        var command = new UpdateItemConfiguracionVentasCommand(1, true, true, 101m, false);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PorcentajeMaximoDescuento);
    }

    [Fact]
    public void Validar_UpdateItemPorcentajeGananciaCommand_ConGananciaNegativa_DebeHaveError()
    {
        var validator = new UpdateItemPorcentajeGananciaCommandValidator();
        var command = new UpdateItemPorcentajeGananciaCommand(1, -1m);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PorcentajeGanancia);
    }
}
