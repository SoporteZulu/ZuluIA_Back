using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Facturacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ConsultarCtgCartaPorteCommandValidatorTests
{
    private readonly ConsultarCtgCartaPorteCommandValidator _validator = new();

    [Fact]
    public void Validar_SinDatos_DebeTenerError()
    {
        var cmd = new ConsultarCtgCartaPorteCommand(1, DateOnly.FromDateTime(DateTime.Today), null, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
