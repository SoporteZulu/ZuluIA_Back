using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.ListasPrecios.Commands;
using ZuluIA_Back.Application.Features.PlanesPago.Commands;
using ZuluIA_Back.Application.Features.Sucursales.Commands;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using FluentAssertions;

namespace ZuluIA_Back.UnitTests.Application;

// ─────────────────────────────────────────────────────────────────────────────
// CreateCategoriaItemCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class CreateCategoriaItemCommandValidatorTests
{
    private readonly CreateCategoriaItemCommandValidator _v = new();

    private static CreateCategoriaItemCommand ComandoValido() =>
        new(null, "CAT001", "Categoría Principal", 1, null);

    [Fact]
    public void ComandoValido_DebeSerValido()
    {
        var result = _v.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CodigoVacio_DebeProducirError()
    {
        var cmd = ComandoValido() with { Codigo = "" };
        _v.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Codigo);
    }

    [Fact]
    public void DescripcionVacia_DebeProducirError()
    {
        var cmd = ComandoValido() with { Descripcion = "" };
        _v.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void NivelCero_DebeProducirError()
    {
        var cmd = ComandoValido() with { Nivel = 0 };
        _v.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Nivel);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreatePlanPagoCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class CreatePlanPagoCommandValidatorTests
{
    private readonly CreatePlanPagoCommandValidator _v = new();

    private static CreatePlanPagoCommand ComandoValido() =>
        new("Contado", 1, 0m);

    [Fact]
    public void ComandoValido_DebeSerValido()
    {
        var result = _v.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DescripcionVacia_DebeProducirError()
    {
        var cmd = ComandoValido() with { Descripcion = "" };
        _v.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void CantidadCuotasCero_DebeProducirError()
    {
        var cmd = ComandoValido() with { CantidadCuotas = 0 };
        _v.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.CantidadCuotas);
    }

    [Fact]
    public void InteresNegativo_DebeProducirError()
    {
        var cmd = ComandoValido() with { InteresPct = -1m };
        _v.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.InteresPct);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DeleteTerceroCommandValidator + ActivarTerceroCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class DeleteTerceroCommandValidatorTests
{
    private readonly DeleteTerceroCommandValidator _v = new();

    [Fact]
    public void IdPositivo_DebeSerValido()
    {
        _v.TestValidate(new DeleteTerceroCommand(1))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void IdCero_DebeProducirError()
    {
        _v.TestValidate(new DeleteTerceroCommand(0))
            .ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void IdNegativo_DebeProducirError()
    {
        _v.TestValidate(new DeleteTerceroCommand(-5))
            .ShouldHaveValidationErrorFor(x => x.Id);
    }
}

public class ActivarTerceroCommandValidatorTests
{
    private readonly ActivarTerceroCommandValidator _v = new();

    [Fact]
    public void IdPositivo_DebeSerValido()
    {
        _v.TestValidate(new ActivarTerceroCommand(1))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void IdCero_DebeProducirError()
    {
        _v.TestValidate(new ActivarTerceroCommand(0))
            .ShouldHaveValidationErrorFor(x => x.Id);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// GetTercerosPagedQueryValidator
// ─────────────────────────────────────────────────────────────────────────────

public class GetTercerosPagedQueryValidatorTests
{
    private readonly GetTercerosPagedQueryValidator _v = new();

    private static GetTercerosPagedQuery QueryValido() =>
        new(Page: 1, PageSize: 20, Search: null);

    [Fact]
    public void QueryValido_DebeSerValido()
    {
        _v.TestValidate(QueryValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PageCero_DebeProducirError()
    {
        var q = QueryValido() with { Page = 0 };
        _v.TestValidate(q).ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void PageSizeCero_DebeProducirError()
    {
        var q = QueryValido() with { PageSize = 0 };
        _v.TestValidate(q).ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void PageSizeMayorA100_DebeProducirError()
    {
        var q = QueryValido() with { PageSize = 101 };
        _v.TestValidate(q).ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void SearchMayorDe100Caracteres_DebeProducirError()
    {
        var q = QueryValido() with { Search = new string('x', 101) };
        _v.TestValidate(q).ShouldHaveValidationErrorFor(x => x.Search);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdateItemCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateItemCommandValidatorTests
{
    private readonly UpdateItemCommandValidator _v = new();

    private static UpdateItemCommand ComandoValido() => new(
        Id: 1,
        Descripcion: "Producto A",
        DescripcionAdicional: null,
        CodigoBarras: null,
        UnidadMedidaId: 1,
        AlicuotaIvaId: 1,
        AlicuotaIvaCompraId: null,
        MonedaId: 1,
        EsProducto: true,
        EsServicio: false,
        EsFinanciero: false,
        ManejaStock: true,
        CategoriaId: null,
        PrecioCosto: 100m,
        PrecioVenta: 150m,
        StockMinimo: 5m,
        StockMaximo: 100m,
        PuntoReposicion: 8m,
        StockSeguridad: 3m,
        Peso: 1.2m,
        Volumen: 0.6m,
        CodigoAfip: null);

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
    public void DescripcionVacia_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Descripcion = "" })
            .ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void PrecioCostoNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { PrecioCosto = -1m })
            .ShouldHaveValidationErrorFor(x => x.PrecioCosto);
    }

    [Fact]
    public void PrecioVentaNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { PrecioVenta = -1m })
            .ShouldHaveValidationErrorFor(x => x.PrecioVenta);
    }

    [Fact]
    public void StockMinimoNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { StockMinimo = -1m })
            .ShouldHaveValidationErrorFor(x => x.StockMinimo);
    }

    [Fact]
    public void StockMaximoMenorAlMinimo_DebeProducirError()
    {
        var cmd = ComandoValido() with { StockMinimo = 50m, StockMaximo = 10m };
        _v.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.StockMaximo);
    }

    [Fact]
    public void StockMaximoNull_DebeSerValido()
    {
        var cmd = ComandoValido() with { StockMaximo = null };
        _v.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PorcentajeGananciaNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { PorcentajeGanancia = -1m })
            .ShouldHaveValidationErrorFor(x => x.PorcentajeGanancia);
    }

    [Fact]
    public void PorcentajeMaximoDescuentoFueraDeRango_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { PorcentajeMaximoDescuento = 101m })
            .ShouldHaveValidationErrorFor(x => x.PorcentajeMaximoDescuento);
    }

    [Fact]
    public void DepositoDefaultSinStock_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { ManejaStock = false, DepositoDefaultId = 7L })
            .ShouldHaveAnyValidationError();
    }

    [Fact]
    public void AlicuotaIvaCompraIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { AlicuotaIvaCompraId = 0 })
            .ShouldHaveValidationErrorFor(x => x.AlicuotaIvaCompraId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdateListaPreciosCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateListaPreciosCommandValidatorTests
{
    private readonly UpdateListaPreciosCommandValidator _v = new();

    private static UpdateListaPreciosCommand ComandoValido() =>
        new(1, "Lista Mayorista", 1, null, null);

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
    public void DescripcionVacia_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Descripcion = "" })
            .ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void MonedaIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { MonedaId = 0 })
            .ShouldHaveValidationErrorFor(x => x.MonedaId);
    }

    [Fact]
    public void VigenciaHastaAnteriorADesde_DebeProducirError()
    {
        var cmd = ComandoValido() with
        {
            VigenciaDesde = new DateOnly(2026, 6, 1),
            VigenciaHasta = new DateOnly(2026, 1, 1)
        };
        var result = _v.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdatePlanPagoCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdatePlanPagoCommandValidatorTests
{
    private readonly UpdatePlanPagoCommandValidator _v = new();

    private static UpdatePlanPagoCommand ComandoValido() =>
        new(1, "Plan 3 cuotas", 3, 0m);

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
    public void DescripcionVacia_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Descripcion = "" })
            .ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void CantidadCuotasCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { CantidadCuotas = 0 })
            .ShouldHaveValidationErrorFor(x => x.CantidadCuotas);
    }

    [Fact]
    public void InteresNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { InteresPct = -0.5m })
            .ShouldHaveValidationErrorFor(x => x.InteresPct);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdateSucursalCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateSucursalCommandValidatorTests
{
    private readonly UpdateSucursalCommandValidator _v = new();

    private static UpdateSucursalCommand ComandoValido() => new(
        Id: 1,
        RazonSocial: "Mi Empresa S.A.",
        NombreFantasia: null,
        Cuit: "30-12345678-9",
        NroIngresosBrutos: null,
        CondicionIvaId: 1,
        MonedaId: 1,
        PaisId: 1,
        Calle: null,
        Nro: null,
        Piso: null,
        Dpto: null,
        CodigoPostal: null,
        LocalidadId: null,
        BarrioId: null,
        Telefono: null,
        Email: null,
        Web: null,
        Cbu: null,
        AliasCbu: null,
        Cai: null,
        PuertoAfip: 443,
        CasaMatriz: true);

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
    public void CuitVacio_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Cuit = "" })
            .ShouldHaveValidationErrorFor(x => x.Cuit);
    }

    [Fact]
    public void CondicionIvaIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { CondicionIvaId = 0 })
            .ShouldHaveValidationErrorFor(x => x.CondicionIvaId);
    }

    [Fact]
    public void MonedaIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { MonedaId = 0 })
            .ShouldHaveValidationErrorFor(x => x.MonedaId);
    }

    [Fact]
    public void PaisIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { PaisId = 0 })
            .ShouldHaveValidationErrorFor(x => x.PaisId);
    }

    [Fact]
    public void EmailInvalido_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Email = "no-es-un-email" })
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmailValido_DebeSerValido()
    {
        _v.TestValidate(ComandoValido() with { Email = "admin@empresa.com" })
            .ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void PuertoAfipCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { PuertoAfip = 0 })
            .ShouldHaveValidationErrorFor(x => x.PuertoAfip);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdateUsuarioCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateUsuarioCommandValidatorTests
{
    private readonly UpdateUsuarioCommandValidator _v = new();

    private static UpdateUsuarioCommand ComandoValido() =>
        new(1, "Juan Pérez", "juan@empresa.com", []);

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
    public void EmailInvalido_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Email = "no-es-email" })
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmailNulo_DebeSerValido()
    {
        _v.TestValidate(ComandoValido() with { Email = null })
            .ShouldNotHaveAnyValidationErrors();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpsertItemEnListaCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public class UpsertItemEnListaCommandValidatorTests
{
    private readonly UpsertItemEnListaCommandValidator _v = new();

    private static UpsertItemEnListaCommand ComandoValido() =>
        new(1, 10, 150m, 10m);

    [Fact]
    public void ComandoValido_DebeSerValido()
    {
        _v.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ListaIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { ListaId = 0 })
            .ShouldHaveValidationErrorFor(x => x.ListaId);
    }

    [Fact]
    public void ItemIdCero_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { ItemId = 0 })
            .ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public void PrecioNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { Precio = -1m })
            .ShouldHaveValidationErrorFor(x => x.Precio);
    }

    [Fact]
    public void DescuentoMayorA100_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { DescuentoPct = 101m })
            .ShouldHaveValidationErrorFor(x => x.DescuentoPct);
    }

    [Fact]
    public void DescuentoNegativo_DebeProducirError()
    {
        _v.TestValidate(ComandoValido() with { DescuentoPct = -1m })
            .ShouldHaveValidationErrorFor(x => x.DescuentoPct);
    }
}
