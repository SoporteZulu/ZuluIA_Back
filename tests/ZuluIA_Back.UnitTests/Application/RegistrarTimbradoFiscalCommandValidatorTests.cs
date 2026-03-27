using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.PuntoVenta.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarTimbradoFiscalCommandValidatorTests
{
    private readonly RegistrarTimbradoFiscalCommandValidator _validator = new();

    [Fact]
    public void Validar_RangoInvalido_DebeTenerError()
    {
        var cmd = new RegistrarTimbradoFiscalCommand(1, 1, "TIM-1", new DateOnly(2025, 2, 1), new DateOnly(2025, 1, 31), null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.VigenciaHasta);
    }
}
