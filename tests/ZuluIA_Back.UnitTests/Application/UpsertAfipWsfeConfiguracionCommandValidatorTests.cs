using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class UpsertAfipWsfeConfiguracionCommandValidatorTests
{
    private readonly UpsertAfipWsfeConfiguracionCommandValidator _validator = new();

    [Fact]
    public void Validar_CuitVacio_DebeTenerError()
    {
        var cmd = new UpsertAfipWsfeConfiguracionCommand(1, true, false, false, string.Empty, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CuitEmisor);
    }
}
