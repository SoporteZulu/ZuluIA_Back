using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ConsultarComprobanteAfipWsfeCommandValidatorTests
{
    private readonly ConsultarComprobanteAfipWsfeCommandValidator _validator = new();

    [Fact]
    public void Validar_IdInvalido_DebeTenerError()
    {
        var cmd = new ConsultarComprobanteAfipWsfeCommand(0);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ComprobanteId);
    }
}
