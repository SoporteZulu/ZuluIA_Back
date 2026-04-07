using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Fiscal.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class ReorganizarAsientosCommandValidatorTests
{
    private readonly ReorganizarAsientosCommandValidator _validator = new();

    [Fact]
    public void Validar_RangoInvalido_DebeTenerError()
    {
        var cmd = new ReorganizarAsientosCommand(1, null, new DateOnly(2025, 2, 1), new DateOnly(2025, 1, 31), null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Hasta);
    }
}
