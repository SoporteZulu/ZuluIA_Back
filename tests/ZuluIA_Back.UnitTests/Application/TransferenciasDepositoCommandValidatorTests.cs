using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class TransferenciasDepositoCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DespacharValidator_IdInvalido_DebeProducirError(long id)
    {
        var validator = new DespacharTransferenciaDepositoCommandValidator();

        var result = validator.TestValidate(new DespacharTransferenciaDepositoCommand(id));

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConfirmarValidator_IdInvalido_DebeProducirError(long id)
    {
        var validator = new ConfirmarTransferenciaDepositoCommandValidator();

        var result = validator.TestValidate(new ConfirmarTransferenciaDepositoCommand(id));

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AnularValidator_IdInvalido_DebeProducirError(long id)
    {
        var validator = new AnularTransferenciaDepositoCommandValidator();

        var result = validator.TestValidate(new AnularTransferenciaDepositoCommand(id));

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void CreateValidator_DetalleConCantidadCero_DebeProducirError()
    {
        var validator = new CreateTransferenciaDepositoCommandValidator();
        var command = new CreateTransferenciaDepositoCommand(
            1,
            10,
            20,
            new DateOnly(2025, 1, 1),
            null,
            [new CreateTransferenciaDepositoDetalleInput(1, 0m, null)]);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Detalles[0].Cantidad");
    }
}
