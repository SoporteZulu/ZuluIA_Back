using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Tesoreria.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarDepositoOperarCommandValidatorTests
{
    private readonly RegistrarDepositoOperarCommandValidator _validator = new();

    [Fact]
    public void Validar_OrigenIgualDestino_DebeTenerError()
    {
        var cmd = new RegistrarDepositoOperarCommand(1, 2, 2, DateOnly.FromDateTime(DateTime.Today), 100m, 1, 1m, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }
}
