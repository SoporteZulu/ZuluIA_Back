using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateTransferenciaDepositoCommandValidatorTests
{
    private readonly CreateTransferenciaDepositoCommandValidator _validator = new();

    [Fact]
    public void Validar_MismoDeposito_DebeTenerError()
    {
        var cmd = new CreateTransferenciaDepositoCommand(1, 10, 10, new DateOnly(2025, 1, 1), null, [new CreateTransferenciaDepositoDetalleInput(1, 1m, null)]);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DepositoDestinoId);
    }
}
