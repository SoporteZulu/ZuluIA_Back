using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class DespacharOrdenPreparacionCommandValidatorTests
{
    private readonly DespacharOrdenPreparacionCommandValidator _validator = new();

    [Fact]
    public void Validar_DepositoDestinoInvalido_DebeTenerError()
    {
        var cmd = new DespacharOrdenPreparacionCommand(1, 0, new DateOnly(2025, 1, 1), null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DepositoDestinoId);
    }
}
