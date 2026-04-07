using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Produccion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarAjusteProduccionCommandValidatorTests
{
    private readonly RegistrarAjusteProduccionCommandValidator _validator = new();

    [Fact]
    public void Validar_CantidadCero_DebeTenerError()
    {
        var cmd = new RegistrarAjusteProduccionCommand(1, 1, 2, 0m, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Cantidad);
    }
}
