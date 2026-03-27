using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class AutorizarComprobanteAfipWsfeCommandValidatorTests
{
    private readonly AutorizarComprobanteAfipWsfeCommandValidator _validator = new();

    [Fact]
    public void Validar_IdInvalido_DebeTenerError()
    {
        var cmd = new AutorizarComprobanteAfipWsfeCommand(0, false);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ComprobanteId);
    }
}
