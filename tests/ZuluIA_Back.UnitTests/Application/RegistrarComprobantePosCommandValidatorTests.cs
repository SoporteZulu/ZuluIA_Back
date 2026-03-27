using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.PuntoVenta.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarComprobantePosCommandValidatorTests
{
    private readonly RegistrarComprobantePosCommandValidator _validator = new();

    [Fact]
    public void Validar_SinItems_DebeTenerError()
    {
        var cmd = new RegistrarComprobantePosCommand(1, 1, 1, new DateOnly(2025, 1, 1), null, 1, 1, 1m, 0m, null, false, null, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }
}
