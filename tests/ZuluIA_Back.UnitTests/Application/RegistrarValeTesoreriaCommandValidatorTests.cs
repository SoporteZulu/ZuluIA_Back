using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Tesoreria.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarValeTesoreriaCommandValidatorTests
{
    private readonly RegistrarValeTesoreriaCommandValidator _validator = new();

    [Fact]
    public void Validar_ImporteCero_DebeTenerError()
    {
        var cmd = new RegistrarValeTesoreriaCommand(1, 1, DateOnly.FromDateTime(DateTime.Today), 0m, 1, 1m, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Importe);
    }
}
