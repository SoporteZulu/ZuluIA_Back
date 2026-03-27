using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Impresion.Commands;
using ZuluIA_Back.Application.Features.Impresion.Enums;

namespace ZuluIA_Back.UnitTests.Application;

public class ImprimirComprobanteFiscalCommandValidatorTests
{
    private readonly ImprimirComprobanteFiscalCommandValidator _validator = new();

    [Fact]
    public void Validar_ComprobanteInvalido_DebeTenerError()
    {
        var result = _validator.TestValidate(new ImprimirComprobanteFiscalCommand(0, MarcaImpresoraFiscal.Epson));
        result.ShouldHaveValidationErrorFor(x => x.ComprobanteId);
    }
}
