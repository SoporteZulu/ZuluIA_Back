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

    [Fact]
    public void Validar_EntregaSinTercero_DebeTenerError()
    {
        var cmd = new CambiarEstadoChequeCommand(1, AccionCheque.Entregar, null, null, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.TerceroId);
    }

    [Fact]
    public void Validar_RechazoSinConcepto_DebeTenerError()
    {
        var cmd = new CambiarEstadoChequeCommand(1, AccionCheque.Rechazar, DateOnly.FromDateTime(DateTime.Today), null, null, 10, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ConceptoRechazo);
    }

    [Fact]
    public void Validar_RechazoSinConcepto_DebeTenerError_SinFecha()
    {
        var cmd = new CambiarEstadoChequeCommand(1, AccionCheque.Rechazar, null, null, "obs", null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ConceptoRechazo);
    }

    [Fact]
    public void Validar_EndosoSinTercero_DebeTenerError()
    {
        var cmd = new CambiarEstadoChequeCommand(1, AccionCheque.Endosar, null, null, "obs");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.TerceroId);
    }

    [Fact]
    public void Validar_AnulacionSinMotivo_DebeTenerError()
    {
        var cmd = new CambiarEstadoChequeCommand(1, AccionCheque.Anular, null, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Observacion);
    }
}
