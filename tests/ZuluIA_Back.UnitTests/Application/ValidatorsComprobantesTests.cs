using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;

namespace ZuluIA_Back.UnitTests.Application;

// ─────────────────────────────────────────────────────────────────────────────
// CreateComprobanteCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreateComprobanteCommandValidatorTests
{
    private readonly CreateComprobanteCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static CreateComprobanteItemDto ItemValido() =>
        new(ItemId: 1, Descripcion: "Producto", Cantidad: 1m, PrecioUnitario: 1000,
            DescuentoPct: 0, AlicuotaIvaId: 1, PorcentajeIva: 21, DepositoId: null, Orden: 1);

    private static CreateComprobanteCommand ComandoValido() => new(
        SucursalId: 1,
        PuntoFacturacionId: null,
        TipoComprobanteId: 1,
        Prefijo: 1,
        Numero: 1,
        Fecha: Hoy,
        FechaVencimiento: Hoy.AddDays(30),
        TerceroId: 1,
        MonedaId: 1,
        Cotizacion: 1m,
        Observacion: null,
        Items: [ItemValido()]);

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
    public void TipoComprobanteIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { TipoComprobanteId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.TipoComprobanteId);
    }

    [Fact]
    public void PrefijoCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Prefijo = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Prefijo);
    }

    [Fact]
    public void NumeroCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Numero = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Numero);
    }

    [Fact]
    public void TerceroIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { TerceroId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.TerceroId);
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
    public void ItemsVacios_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Items = [] });
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void ItemItemIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Items = [ItemValido() with { ItemId = 0 }]
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ItemId"));
    }

    [Fact]
    public void ItemCantidadCero_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Items = [ItemValido() with { Cantidad = 0m }]
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Cantidad"));
    }

    [Fact]
    public void ItemDescuentoMayor100_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Items = [ItemValido() with { DescuentoPct = 101 }]
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("DescuentoPct"));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// EmitirComprobanteCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class EmitirComprobanteCommandValidatorTests
{
    private readonly EmitirComprobanteCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static ComprobanteItemInput ItemValido() =>
        new(ItemId: 1, Descripcion: "Prod", Cantidad: 2m, CantidadBonificada: 0,
            PrecioUnitario: 500, DescuentoPct: 0m, AlicuotaIvaId: 1, DepositoId: null, Orden: 1);

    private static EmitirComprobanteCommand ComandoValido() => new(
        Id: null,
        SucursalId: 1,
        PuntoFacturacionId: null,
        TipoComprobanteId: 1,
        Fecha: Hoy,
        FechaVencimiento: Hoy.AddDays(30),
        TerceroId: 1,
        MonedaId: 1,
        Cotizacion: 1m,
        Percepciones: 0m,
        Observacion: null,
        Items: [ItemValido()]);

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
    public void TipoComprobanteIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { TipoComprobanteId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.TipoComprobanteId);
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
    public void ItemsVacios_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Items = [] });
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void ItemCantidadCero_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Items = [ItemValido() with { Cantidad = 0 }]
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Cantidad"));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// RegistrarAsientoCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class RegistrarAsientoCommandValidatorTests
{
    private readonly RegistrarAsientoCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static RegistrarAsientoCommand ComandoBalanceado() => new(
        EjercicioId: 1,
        SucursalId: 1,
        Fecha: Hoy,
        Descripcion: "Apertura ejercicio",
        OrigenTabla: null,
        OrigenId: null,
        Lineas:
        [
            new LineaAsientoInput(CuentaId: 1, Debe: 1000m, Haber: 0m, Descripcion: "Caja", CentroCostoId: null),
            new LineaAsientoInput(CuentaId: 2, Debe: 0m,    Haber: 1000m, Descripcion: "Capital", CentroCostoId: null)
        ]);

    [Fact]
    public void AsientoBalanceado_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoBalanceado());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EjercicioIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoBalanceado() with { EjercicioId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.EjercicioId);
    }

    [Fact]
    public void SucursalIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoBalanceado() with { SucursalId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.SucursalId);
    }

    [Fact]
    public void DescripcionVacia_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoBalanceado() with { Descripcion = "" });
        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void LineasVacias_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoBalanceado() with { Lineas = [] });
        result.ShouldHaveValidationErrorFor(x => x.Lineas);
    }

    [Fact]
    public void AsientoNoBalanceado_DebeHaveError()
    {
        var cmd = ComandoBalanceado() with
        {
            Lineas =
            [
                new LineaAsientoInput(1, 1000m, 0m, "Debe", null),
                new LineaAsientoInput(2, 0m, 500m, "Haber", null)
            ]
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Lineas);
    }

    [Fact]
    public void LineaConDebeYHaberSimultaneo_DebeHaveError()
    {
        var cmd = new RegistrarAsientoCommand(1, 1, Hoy, "Test", null, null,
        [
            new LineaAsientoInput(1, 500m, 500m, "Ambos", null),
            new LineaAsientoInput(2, 0m,   0m,   "Cero",  null)
        ]);
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }
}
