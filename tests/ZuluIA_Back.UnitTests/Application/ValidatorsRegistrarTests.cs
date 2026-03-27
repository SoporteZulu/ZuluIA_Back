using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Terceros.Commands;

namespace ZuluIA_Back.UnitTests.Application;

// ─────────────────────────────────────────────────────────────────────────────
// UpdateTerceroCommandValidator  (complejo: 15+ reglas)
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateTerceroCommandValidatorTests
{
    private readonly UpdateTerceroCommandValidator _v = new();

    private static UpdateTerceroCommand ComandoValido() => new(
        Id: 1,
        RazonSocial: "Proveedor S.A.",
        NombreFantasia: null,
        NroDocumento: null,
        CondicionIvaId: 1,
        EsCliente: true,
        EsProveedor: false,
        EsEmpleado: false,
        Calle: null,
        Nro: null,
        Piso: null,
        Dpto: null,
        CodigoPostal: null,
        LocalidadId: null,
        BarrioId: null,
        NroIngresosBrutos: null,
        NroMunicipal: null,
        Telefono: null,
        Celular: null,
        Email: null,
        Web: null,
        MonedaId: null,
        CategoriaId: null,
        LimiteCredito: null,
        Facturable: true,
        CobradorId: null,
        PctComisionCobrador: 0m,
        VendedorId: null,
        PctComisionVendedor: 0m,
        Observacion: null);

    [Fact]
    public void ComandoValido_DebeSerValido()
    {
        _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void IdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Id = 0 })
            .ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void RazonSocialVacia_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { RazonSocial = "" })
            .ShouldHaveValidationErrorFor(x => x.RazonSocial);
    }

    [Fact]
    public void CondicionIvaIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { CondicionIvaId = 0 })
            .ShouldHaveValidationErrorFor(x => x.CondicionIvaId);
    }

    [Fact]
    public void SinRolesActivos_DebeProducirError()
    {
        var cmd = ComandoValido() with
        {
            EsCliente = false,
            EsProveedor = false,
            EsEmpleado = false
        };
        var result = _v.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Roles");
    }

    [Fact]
    public void EmailInvalido_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Email = "no-es-email" })
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmailValido_DebeSerValido()
    {
        _v.TestValidate(ComandoValido() with { Email = "proveedor@empresa.com" })
            .ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LimiteCreditoNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { LimiteCredito = -100m })
            .ShouldHaveValidationErrorFor(x => x.LimiteCredito);
    }

    [Fact]
    public void PctComisionCobradorMayorA100_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { PctComisionCobrador = 101m })
            .ShouldHaveValidationErrorFor(x => x.PctComisionCobrador);
    }

    [Fact]
    public void PctComisionVendedorNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { PctComisionVendedor = -1m })
            .ShouldHaveValidationErrorFor(x => x.PctComisionVendedor);
    }

    [Fact]
    public void SoloEsProveedor_TambienEsValido()
    {
        var cmd = ComandoValido() with
        {
            EsCliente = false,
            EsProveedor = true,
            EsEmpleado = false
        };
        _v.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// RegistrarCobroCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class RegistrarCobroCommandValidatorTests
{
    private readonly RegistrarCobroCommandValidator _v = new();

    private static MedioCobroInput MedioValido() =>
        new(CajaId: 1, FormaPagoId: 1, ChequeId: null, Importe: 500m, MonedaId: 1, Cotizacion: 1m);

    private static RegistrarCobroCommand ComandoValido() => new(
        SucursalId: 1,
        TerceroId: 1,
        Fecha: new DateOnly(2025, 3, 1),
        MonedaId: 1,
        Cotizacion: 1m,
        Observacion: null,
        Medios: [MedioValido()],
        ComprobantesAImputar: []);

    [Fact]
    public void ComandoValido_DebeSerValido()
    {
        _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SucursalIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { SucursalId = 0 })
            .ShouldHaveValidationErrorFor(x => x.SucursalId);
    }

    [Fact]
    public void TerceroIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { TerceroId = 0 })
            .ShouldHaveValidationErrorFor(x => x.TerceroId);
    }

    [Fact]
    public void MonedaIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { MonedaId = 0 })
            .ShouldHaveValidationErrorFor(x => x.MonedaId);
    }

    [Fact]
    public void CotizacionCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Cotizacion = 0m })
            .ShouldHaveValidationErrorFor(x => x.Cotizacion);
    }

    [Fact]
    public void MediosVacios_DebeProducirError()
    {
        var result = _v.TestValidate(ComandoValido() with { Medios = [] });
        result.ShouldHaveValidationErrorFor(x => x.Medios);
    }

    [Fact]
    public void MedioConCajaIdCero_DebeProducirError()
    {
        var medioInvalido = MedioValido() with { CajaId = 0 };
        var result = _v.Validate(ComandoValido() with { Medios = [medioInvalido] });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("CajaId"));
    }

    [Fact]
    public void MedioConImporteCero_DebeProducirError()
    {
        var medioInvalido = MedioValido() with { Importe = 0m };
        var result = _v.Validate(ComandoValido() with { Medios = [medioInvalido] });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Importe"));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// RegistrarPagoCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class RegistrarPagoCommandValidatorTests
{
    private readonly RegistrarPagoCommandValidator _v = new();

    private static MedioPagoInput MedioValido() =>
        new(CajaId: 1, FormaPagoId: 1, ChequeId: null, Importe: 500m, MonedaId: 1, Cotizacion: 1m);

    private static RetencionInput RetencionValida() =>
        new(Tipo: "GANANCIAS", Importe: 50m, NroCertificado: "001");

    private static RegistrarPagoCommand ComandoValido() => new(
        SucursalId: 1,
        TerceroId: 1,
        Fecha: new DateOnly(2025, 3, 1),
        MonedaId: 1,
        Cotizacion: 1m,
        Observacion: null,
        Medios: [MedioValido()],
        Retenciones: [],
        ComprobantesAImputar: []);

    [Fact]
    public void ComandoValido_DebeSerValido()
    {
        _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SucursalIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { SucursalId = 0 })
            .ShouldHaveValidationErrorFor(x => x.SucursalId);
    }

    [Fact]
    public void TerceroIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { TerceroId = 0 })
            .ShouldHaveValidationErrorFor(x => x.TerceroId);
    }

    [Fact]
    public void CotizacionCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Cotizacion = 0m })
            .ShouldHaveValidationErrorFor(x => x.Cotizacion);
    }

    [Fact]
    public void MediosVacios_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Medios = [] })
            .ShouldHaveValidationErrorFor(x => x.Medios);
    }

    [Fact]
    public void MedioConFormaPagoCero_DebeProducirError()
    {
        var medioInvalido = MedioValido() with { FormaPagoId = 0 };
        var result = _v.Validate(ComandoValido() with { Medios = [medioInvalido] });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("FormaPagoId"));
    }

    [Fact]
    public void RetencionConTipoVacio_DebeProducirError()
    {
        var retencionInvalida = RetencionValida() with { Tipo = "" };
        var result = _v.Validate(ComandoValido() with { Retenciones = [retencionInvalida] });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Tipo"));
    }

    [Fact]
    public void RetencionConImporteCero_DebeProducirError()
    {
        var retencionInvalida = RetencionValida() with { Importe = 0m };
        var result = _v.Validate(ComandoValido() with { Retenciones = [retencionInvalida] });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Importe"));
    }

    [Fact]
    public void ConRetencionValida_DebeSerValido()
    {
        var cmd = ComandoValido() with { Retenciones = [RetencionValida()] };
        _v.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }
}
