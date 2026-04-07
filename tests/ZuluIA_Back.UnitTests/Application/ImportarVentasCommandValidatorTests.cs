using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Integraciones.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ImportarVentasCommandValidatorTests
{
    private readonly ImportarVentasCommandValidator _validator = new();

    [Fact]
    public void Validar_SinVentas_DebeTenerError()
    {
        var result = _validator.TestValidate(new ImportarVentasCommand([], null));
        result.ShouldHaveValidationErrorFor(x => x.Ventas);
    }
}
