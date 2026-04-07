using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.RRHH.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class GenerarLiquidacionesMasivasCommandValidatorTests
{
    private readonly GenerarLiquidacionesMasivasCommandValidator _validator = new();

    [Fact]
    public void Validar_PorcentajeInvalido_DebeTenerError()
    {
        var cmd = new GenerarLiquidacionesMasivasCommand(1, 2025, 1, -101m);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PorcentajeAjuste);
    }
}
