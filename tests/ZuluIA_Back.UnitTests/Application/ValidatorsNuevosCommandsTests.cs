using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Cajas.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Application.Features.Retenciones.Commands;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;
using ZuluIA_Back.Application.Features.RRHH.Commands;

namespace ZuluIA_Back.UnitTests.Application;

// ─────────────────────────────────────────────────────────────────────────────
// UpdateCajaCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateCajaCommandValidatorTests
{
    private readonly UpdateCajaCommandValidator _v = new();

    private static UpdateCajaCommand ComandoValido() =>
        new(Id: 1, Descripcion: "Caja Principal", TipoId: 1, MonedaId: 1,
            EsCaja: true, Banco: null, NroCuenta: null, Cbu: null, UsuarioId: null);

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void IdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Id = 0 }).ShouldHaveValidationErrorFor(x => x.Id);

    [Fact]
    public void DescripcionVacia_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Descripcion = "" }).ShouldHaveValidationErrorFor(x => x.Descripcion);

    [Fact]
    public void TipoIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { TipoId = 0 }).ShouldHaveValidationErrorFor(x => x.TipoId);

    [Fact]
    public void MonedaIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { MonedaId = 0 }).ShouldHaveValidationErrorFor(x => x.MonedaId);
}

// ─────────────────────────────────────────────────────────────────────────────
// CreatePlanCuentaCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class CreatePlanCuentaCommandValidatorTests
{
    private readonly CreatePlanCuentaCommandValidator _v = new();

    private static CreatePlanCuentaCommand ComandoValido() =>
        new(EjercicioId: 1, IntegradoraId: null, CodigoCuenta: "1.1.01",
            Denominacion: "Caja y Bancos", Nivel: 3, OrdenNivel: "001",
            Imputable: true, Tipo: "A", SaldoNormal: 'D');

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EjercicioIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { EjercicioId = 0 }).ShouldHaveValidationErrorFor(x => x.EjercicioId);

    [Fact]
    public void CodigoCuentaVacio_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { CodigoCuenta = "" }).ShouldHaveValidationErrorFor(x => x.CodigoCuenta);

    [Fact]
    public void DenominacionVacia_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Denominacion = "" }).ShouldHaveValidationErrorFor(x => x.Denominacion);

    [Fact]
    public void NivelCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Nivel = 0 }).ShouldHaveValidationErrorFor(x => x.Nivel);

    [Fact]
    public void OrdenNivelVacio_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { OrdenNivel = "" }).ShouldHaveValidationErrorFor(x => x.OrdenNivel);
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateDescuentoComercialCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class CreateDescuentoComercialCommandValidatorTests
{
    private readonly CreateDescuentoComercialCommandValidator _v = new();

    private static CreateDescuentoComercialCommand ComandoValido() =>
        new(TerceroId: 1, ItemId: 1,
            FechaDesde: new DateOnly(2025, 1, 1), FechaHasta: null,
            Porcentaje: 10m);

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void TerceroIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { TerceroId = 0 }).ShouldHaveValidationErrorFor(x => x.TerceroId);

    [Fact]
    public void ItemIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { ItemId = 0 }).ShouldHaveValidationErrorFor(x => x.ItemId);

    [Fact]
    public void PorcentajeFueraDeRango_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Porcentaje = 110m }).ShouldHaveValidationErrorFor(x => x.Porcentaje);

    [Fact]
    public void FechaHastaMenorQueFechaDesde_DebeProducirError()
    {
        var cmd = ComandoValido() with
        {
            FechaDesde = new DateOnly(2025, 6, 1),
            FechaHasta = new DateOnly(2025, 1, 1)
        };
        _v.TestValidate(cmd).ShouldHaveAnyValidationError();
    }

    [Fact]
    public void FechaHastaIgualAFechaDesde_DebeSerValido()
    {
        var fecha = new DateOnly(2025, 6, 1);
        var cmd = ComandoValido() with { FechaDesde = fecha, FechaHasta = fecha };
        _v.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdateDescuentoComercialCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateDescuentoComercialCommandValidatorTests
{
    private readonly UpdateDescuentoComercialCommandValidator _v = new();

    private static UpdateDescuentoComercialCommand ComandoValido() =>
        new(Id: 1, FechaDesde: new DateOnly(2025, 1, 1), FechaHasta: null, Porcentaje: 15m);

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void IdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Id = 0 }).ShouldHaveValidationErrorFor(x => x.Id);

    [Fact]
    public void PorcentajeNegativo_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Porcentaje = -1m }).ShouldHaveValidationErrorFor(x => x.Porcentaje);

    [Fact]
    public void FechaHastaMenorQueFechaDesde_DebeProducirError()
    {
        var cmd = ComandoValido() with
        {
            FechaDesde = new DateOnly(2025, 6, 1),
            FechaHasta = new DateOnly(2025, 1, 1)
        };
        _v.TestValidate(cmd).ShouldHaveAnyValidationError();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateCartaPorteCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class CreateCartaPorteCommandValidatorTests
{
    private readonly CreateCartaPorteCommandValidator _v = new();

    private static CreateCartaPorteCommand ComandoValido() =>
        new(ComprobanteId: null,
            CuitRemitente: "20123456789",
            CuitDestinatario: "30987654321",
            CuitTransportista: null,
            FechaEmision: new DateOnly(2025, 6, 1),
            Observacion: null);

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void CuitRemitenteVacio_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { CuitRemitente = "" }).ShouldHaveValidationErrorFor(x => x.CuitRemitente);

    [Fact]
    public void CuitRemitenteConLongitudIncorrecta_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { CuitRemitente = "201234" }).ShouldHaveValidationErrorFor(x => x.CuitRemitente);

    [Fact]
    public void CuitDestinatarioVacio_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { CuitDestinatario = "" }).ShouldHaveValidationErrorFor(x => x.CuitDestinatario);

    [Fact]
    public void CuitTransportistaConLongitudIncorrecta_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { CuitTransportista = "123" }).ShouldHaveValidationErrorFor(x => x.CuitTransportista);

    [Fact]
    public void CuitTransportistaNulo_DebeSerValido()
        => _v.TestValidate(ComandoValido() with { CuitTransportista = null }).ShouldNotHaveAnyValidationErrors();
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdateItemPreciosCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateItemPreciosCommandValidatorTests
{
    private readonly UpdateItemPreciosCommandValidator _v = new();

    private static UpdateItemPreciosCommand ComandoValido() =>
        new(Id: 1, PrecioCosto: 100m, PrecioVenta: 200m);

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void IdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Id = 0 }).ShouldHaveValidationErrorFor(x => x.Id);

    [Fact]
    public void PrecioCostoNegativo_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { PrecioCosto = -1m }).ShouldHaveValidationErrorFor(x => x.PrecioCosto);

    [Fact]
    public void PrecioVentaNegativo_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { PrecioVenta = -0.01m }).ShouldHaveValidationErrorFor(x => x.PrecioVenta);

    [Fact]
    public void PreciosCeroAmbos_DebeSerValido()
        => _v.TestValidate(ComandoValido() with { PrecioCosto = 0m, PrecioVenta = 0m }).ShouldNotHaveAnyValidationErrors();
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateOrdenPreparacionCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class CreateOrdenPreparacionCommandValidatorTests
{
    private readonly CreateOrdenPreparacionCommandValidator _v = new();

    private static CreateOrdenPreparacionDetalleDto DetalleValido() =>
        new(ItemId: 1, DepositoId: 1, Cantidad: 5m, Observacion: null);

    private static CreateOrdenPreparacionCommand ComandoValido() =>
        new(SucursalId: 1, ComprobanteOrigenId: null, TerceroId: null,
            Fecha: new DateOnly(2025, 6, 1), Observacion: null,
            Detalles: [DetalleValido()]);

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void SucursalIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { SucursalId = 0 }).ShouldHaveValidationErrorFor(x => x.SucursalId);

    [Fact]
    public void DetallesVacios_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Detalles = [] }).ShouldHaveValidationErrorFor(x => x.Detalles);

    [Fact]
    public void DetalleConItemIdCero_DebeProducirError()
    {
        var detalleInvalido = DetalleValido() with { ItemId = 0 };
        var cmd = ComandoValido() with { Detalles = [detalleInvalido] };
        _v.TestValidate(cmd).ShouldHaveAnyValidationError();
    }

    [Fact]
    public void DetalleConCantidadCero_DebeProducirError()
    {
        var detalleInvalido = DetalleValido() with { Cantidad = 0m };
        var cmd = ComandoValido() with { Detalles = [detalleInvalido] };
        _v.TestValidate(cmd).ShouldHaveAnyValidationError();
    }

    [Fact]
    public void DetalleConDepositoIdCero_DebeProducirError()
    {
        var detalleInvalido = DetalleValido() with { DepositoId = 0 };
        var cmd = ComandoValido() with { Detalles = [detalleInvalido] };
        _v.TestValidate(cmd).ShouldHaveAnyValidationError();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdateTipoRetencionCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateTipoRetencionCommandValidatorTests
{
    private readonly UpdateTipoRetencionCommandValidator _v = new();

    private static UpdateTipoRetencionCommand ComandoValido() =>
        new(Id: 1, Descripcion: "Ganancias", Regimen: "GANANCIAS",
            MinimoNoImponible: 0m, AcumulaPago: false,
            TipoComprobanteId: null, ItemId: null, Escalas: []);

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void IdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Id = 0 }).ShouldHaveValidationErrorFor(x => x.Id);

    [Fact]
    public void DescripcionVacia_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Descripcion = "" }).ShouldHaveValidationErrorFor(x => x.Descripcion);

    [Fact]
    public void RegimenVacio_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Regimen = "" }).ShouldHaveValidationErrorFor(x => x.Regimen);

    [Fact]
    public void MinimoNoImponibleNegativo_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { MinimoNoImponible = -1m }).ShouldHaveValidationErrorFor(x => x.MinimoNoImponible);

    [Fact]
    public void EscalaConPorcentajeFueraDeRango_DebeProducirError()
    {
        var escalaInvalida = new EscalaRetencionInputDto("Tramo 1", 0m, 50_000m, 110m);
        var cmd = ComandoValido() with { Escalas = [escalaInvalida] };
        _v.TestValidate(cmd).ShouldHaveAnyValidationError();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateLiquidacionSueldoCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class CreateLiquidacionSueldoCommandValidatorRegressionTests
{
    private readonly CreateLiquidacionSueldoCommandValidator _v = new();

    private static CreateLiquidacionSueldoCommand ComandoValido() =>
        new(EmpleadoId: 1, SucursalId: 1, Anio: 2025, Mes: 6,
            SueldoBasico: 500_000m, TotalHaberes: 520_000m,
            TotalDescuentos: 80_000m, MonedaId: 1, Observacion: null);

    [Fact]
    public void ComandoValido_DebeSerValido()
        => _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EmpleadoIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { EmpleadoId = 0 }).ShouldHaveValidationErrorFor(x => x.EmpleadoId);

    [Fact]
    public void SucursalIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { SucursalId = 0 }).ShouldHaveValidationErrorFor(x => x.SucursalId);

    [Fact]
    public void MonedaIdCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { MonedaId = 0 }).ShouldHaveValidationErrorFor(x => x.MonedaId);

    [Fact]
    public void AnioFueraDeRango_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Anio = 1999 }).ShouldHaveValidationErrorFor(x => x.Anio);

    [Fact]
    public void MesCero_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Mes = 0 }).ShouldHaveValidationErrorFor(x => x.Mes);

    [Fact]
    public void Mes13_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { Mes = 13 }).ShouldHaveValidationErrorFor(x => x.Mes);

    [Fact]
    public void SueldoBasicoNegativo_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { SueldoBasico = -1m }).ShouldHaveValidationErrorFor(x => x.SueldoBasico);

    [Fact]
    public void TotalHaberesNegativo_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { TotalHaberes = -1m }).ShouldHaveValidationErrorFor(x => x.TotalHaberes);

    [Fact]
    public void TotalDescuentosNegativo_DebeProducirError()
        => _v.TestValidate(ComandoValido() with { TotalDescuentos = -1m }).ShouldHaveValidationErrorFor(x => x.TotalDescuentos);
}
