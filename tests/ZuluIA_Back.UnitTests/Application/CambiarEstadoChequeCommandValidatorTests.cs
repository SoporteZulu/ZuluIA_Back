using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Cheques.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CambiarEstadoChequeCommandValidatorTests
{
    private readonly CambiarEstadoChequeCommandValidator _validator = new();

    [Fact]
    public void Validar_DepositoSinFecha_DebeTenerError()
    {
        var cmd = new CambiarEstadoChequeCommand(1, AccionCheque.Depositar, null, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Fecha);
    }

    [Fact]
    public void Validar_EntregaSinFecha_NoDebeTenerError()
    {
        var cmd = new CambiarEstadoChequeCommand(1, AccionCheque.Entregar, null, null, null, 10);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Fecha);
    }
}
