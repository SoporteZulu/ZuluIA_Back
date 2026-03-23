using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Pagos.Commands;
using ZuluIA_Back.Application.Features.Pagos.DTOs;
using ZuluIA_Back.Application.Features.Cheques.Commands;
using ZuluIA_Back.Application.Features.Cotizaciones.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;

namespace ZuluIA_Back.UnitTests.Application;

// ─────────────────────────────────────────────────────────────────────────────
// CreatePagoCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreatePagoCommandValidatorTests
{
    private readonly CreatePagoCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static CreatePagoCommand ComandoValido() => new(
        SucursalId: 1,
        TerceroId: 1,
        Fecha: Hoy,
        MonedaId: 1,
        Cotizacion: 1m,
        Observacion: null,
        Medios: [new CreatePagoMedioDto(CajaId: 1, FormaPagoId: 1, Importe: 1000m, MonedaId: 1, Cotizacion: 1m)]);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SucursalIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { SucursalId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.SucursalId);
    }

    [Fact]
    public void TerceroIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { TerceroId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.TerceroId);
    }

    [Fact]
    public void CotizacionCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Cotizacion = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Cotizacion);
    }

    [Fact]
    public void MediosVacios_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Medios = [] });
        result.ShouldHaveValidationErrorFor(x => x.Medios);
    }

    [Fact]
    public void MedioFormaPagoIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Medios = [new CreatePagoMedioDto(CajaId: 1, FormaPagoId: 0, Importe: 500m, MonedaId: 1, Cotizacion: 1m)]
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("FormaPagoId"));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateChequeCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreateChequeCommandValidatorTests
{
    private readonly CreateChequeCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static CreateChequeCommand ComandoValido() => new(
        CajaId: 1,
        TerceroId: null,
        NroCheque: "00001234",
        Banco: "Banco Nación",
        FechaEmision: Hoy,
        FechaVencimiento: Hoy.AddDays(30),
        Importe: 5000m,
        MonedaId: 1,
        Observacion: null);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CajaIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { CajaId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.CajaId);
    }

    [Fact]
    public void NroChequeVacio_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { NroCheque = "" });
        result.ShouldHaveValidationErrorFor(x => x.NroCheque);
    }

    [Fact]
    public void BancoVacio_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Banco = "" });
        result.ShouldHaveValidationErrorFor(x => x.Banco);
    }

    [Fact]
    public void ImporteCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Importe = 0m });
        result.ShouldHaveValidationErrorFor(x => x.Importe);
    }

    [Fact]
    public void MonedaIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { MonedaId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.MonedaId);
    }

    [Fact]
    public void FechaVencimientoAnteriorAEmision_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with
        {
            FechaEmision = Hoy,
            FechaVencimiento = Hoy.AddDays(-1)
        });
        result.ShouldHaveValidationErrorFor(x => x.FechaVencimiento);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// RegistrarCotizacionCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class RegistrarCotizacionCommandValidatorTests
{
    private readonly RegistrarCotizacionCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static RegistrarCotizacionCommand ComandoValido() =>
        new(MonedaId: 1, Fecha: Hoy, Cotizacion: 850m);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MonedaIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { MonedaId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.MonedaId);
    }

    [Fact]
    public void CotizacionCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Cotizacion = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Cotizacion);
    }

    [Fact]
    public void FechaFutura_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Fecha = Hoy.AddDays(2) });
        result.ShouldHaveValidationErrorFor(x => x.Fecha);
    }

    [Fact]
    public void FechaDeHoy_EsValida()
    {
        var result = _validator.TestValidate(ComandoValido() with { Fecha = Hoy });
        result.ShouldNotHaveValidationErrorFor(x => x.Fecha);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ImputarComprobanteCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class ImputarComprobanteCommandValidatorTests
{
    private readonly ImputarComprobanteCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static ImputarComprobanteCommand ComandoValido() =>
        new(ComprobanteOrigenId: 1, ComprobanteDestinoId: 2, Importe: 1000m, Fecha: Hoy);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void OrigenIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { ComprobanteOrigenId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.ComprobanteOrigenId);
    }

    [Fact]
    public void DestinoIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { ComprobanteDestinoId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.ComprobanteDestinoId);
    }

    [Fact]
    public void OrigenIgualADestino_DebeHaveError()
    {
        var result = _validator.TestValidate(
            new ImputarComprobanteCommand(ComprobanteOrigenId: 5, ComprobanteDestinoId: 5,
                Importe: 100m, Fecha: Hoy));
        result.ShouldHaveValidationErrorFor(x => x.ComprobanteDestinoId);
    }

    [Fact]
    public void ImporteCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Importe = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Importe);
    }
}
