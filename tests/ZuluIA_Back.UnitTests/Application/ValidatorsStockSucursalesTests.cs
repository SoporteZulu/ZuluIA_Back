using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Cajas.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.ListasPrecios.Commands;
using ZuluIA_Back.Application.Features.Stock.Commands;
using ZuluIA_Back.Application.Features.Sucursales.Commands;
using ZuluIA_Back.Application.Features.Usuarios.Commands;

namespace ZuluIA_Back.UnitTests.Application;

// ─────────────────────────────────────────────────────────────────────────────
// AjusteStockCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class AjusteStockCommandValidatorTests
{
    private readonly AjusteStockCommandValidator _validator = new();

    private static AjusteStockCommand ComandoValido() =>
        new(ItemId: 1, DepositoId: 1, NuevaCantidad: 50m, Observacion: null);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ItemIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { ItemId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public void DepositoIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { DepositoId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.DepositoId);
    }

    [Fact]
    public void CantidadNegativa_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { NuevaCantidad = -1m });
        result.ShouldHaveValidationErrorFor(x => x.NuevaCantidad);
    }

    [Fact]
    public void CantidadCero_EsValida()
    {
        // Ajuste a 0 debe permitirse (anular stock)
        var result = _validator.TestValidate(ComandoValido() with { NuevaCantidad = 0m });
        result.ShouldNotHaveValidationErrorFor(x => x.NuevaCantidad);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TransferenciaStockCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class TransferenciaStockCommandValidatorTests
{
    private readonly TransferenciaStockCommandValidator _validator = new();

    private static TransferenciaStockCommand ComandoValido() =>
        new(ItemId: 1, DepositoOrigenId: 10, DepositoDestinoId: 20, Cantidad: 5m, Observacion: null);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ItemIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { ItemId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public void OrigenIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { DepositoOrigenId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.DepositoOrigenId);
    }

    [Fact]
    public void DestinoIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { DepositoDestinoId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.DepositoDestinoId);
    }

    [Fact]
    public void OrigenIgualADestino_DebeHaveError()
    {
        var result = _validator.TestValidate(
            ComandoValido() with { DepositoOrigenId = 10, DepositoDestinoId = 10 });
        result.ShouldHaveValidationErrorFor(x => x.DepositoDestinoId);
    }

    [Fact]
    public void CantidadCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Cantidad = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Cantidad);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// StockInicialCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class StockInicialCommandValidatorTests
{
    private readonly StockInicialCommandValidator _validator = new();

    private static StockInicialCommand ComandoValido() =>
        new(Items: [new StockInicialItemDto(ItemId: 1, DepositoId: 1, Cantidad: 10m)],
            Observacion: null);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
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
            Items = [new StockInicialItemDto(0, 1, 10m)]
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ItemId"));
    }

    [Fact]
    public void ItemDepositoIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Items = [new StockInicialItemDto(1, 0, 10m)]
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("DepositoId"));
    }

    [Fact]
    public void ItemCantidadNegativa_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Items = [new StockInicialItemDto(1, 1, -5m)]
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Cantidad"));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateCajaCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreateCajaCommandValidatorTests
{
    private readonly CreateCajaCommandValidator _validator = new();

    private static CreateCajaCommand ComandoValido() =>
        new(SucursalId: 1, TipoId: 1, Descripcion: "Caja Principal",
            MonedaId: 1, EsCaja: true, Banco: null, NroCuenta: null, Cbu: null, UsuarioId: null);

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
    public void TipoIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { TipoId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.TipoId);
    }

    [Fact]
    public void DescripcionVacia_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Descripcion = "" });
        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void MonedaIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { MonedaId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.MonedaId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreatePuntoFacturacionCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreatePuntoFacturacionCommandValidatorTests
{
    private readonly CreatePuntoFacturacionCommandValidator _validator = new();

    private static CreatePuntoFacturacionCommand ComandoValido() =>
        new(SucursalId: 1, TipoId: 1, Numero: 1, Descripcion: null);

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
    public void TipoIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { TipoId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.TipoId);
    }

    [Fact]
    public void NumeroCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Numero = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Numero);
    }

    [Fact]
    public void NumeroMayor9999_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Numero = 10000 });
        result.ShouldHaveValidationErrorFor(x => x.Numero);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateDepositoCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreateDepositoCommandValidatorTests
{
    private readonly CreateDepositoCommandValidator _validator = new();

    private static CreateDepositoCommand ComandoValido() =>
        new(SucursalId: 1, Descripcion: "Depósito Central", EsDefault: false);

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
    public void DescripcionVacia_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Descripcion = "" });
        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateEjercicioCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreateEjercicioCommandValidatorTests
{
    private readonly CreateEjercicioCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static CreateEjercicioCommand ComandoValido() => new(
        Descripcion: "Ejercicio 2026",
        FechaInicio: new DateOnly(2026, 1, 1),
        FechaFin: new DateOnly(2026, 12, 31),
        Mascara: "X.XX.XXX.XXXX",
        SucursalIds: [1L]);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DescripcionVacia_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Descripcion = "" });
        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void FechaFinAnteriorAInicio_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            FechaInicio = new DateOnly(2026, 6, 1),
            FechaFin    = new DateOnly(2026, 1, 1)
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FechaFin);
    }

    [Fact]
    public void MascaraVacia_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Mascara = "" });
        result.ShouldHaveValidationErrorFor(x => x.Mascara);
    }

    [Fact]
    public void SucursalIdsVaciosList_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { SucursalIds = [] });
        result.ShouldHaveValidationErrorFor(x => x.SucursalIds);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateListaPreciosCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreateListaPreciosCommandValidatorTests
{
    private readonly CreateListaPreciosCommandValidator _validator = new();
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static CreateListaPreciosCommand ComandoValido() =>
        new(Descripcion: "Lista Enero 2026", MonedaId: 1,
            VigenciaDesde: Hoy, VigenciaHasta: Hoy.AddMonths(1));

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DescripcionVacia_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Descripcion = "" });
        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
    }

    [Fact]
    public void MonedaIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { MonedaId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.MonedaId);
    }

    [Fact]
    public void VigenciaHastaAnteriorADesde_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            VigenciaDesde = Hoy.AddDays(10),
            VigenciaHasta = Hoy
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.VigenciaHasta);
    }

    [Fact]
    public void SinVigencia_EsValido()
    {
        var result = _validator.TestValidate(
            new CreateListaPreciosCommand("Lista", 1, null, null));
        result.ShouldNotHaveAnyValidationErrors();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateSucursalCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreateSucursalCommandValidatorTests
{
    private readonly CreateSucursalCommandValidator _validator = new();

    private static CreateSucursalCommand ComandoValido() => new(
        RazonSocial: "Empresa SA",
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
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RazonSocialVacia_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { RazonSocial = "" });
        result.ShouldHaveValidationErrorFor(x => x.RazonSocial);
    }

    [Fact]
    public void CuitVacio_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Cuit = "" });
        result.ShouldHaveValidationErrorFor(x => x.Cuit);
    }

    [Fact]
    public void CondicionIvaIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { CondicionIvaId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.CondicionIvaId);
    }

    [Fact]
    public void MonedaIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { MonedaId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.MonedaId);
    }

    [Fact]
    public void PaisIdCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { PaisId = 0 });
        result.ShouldHaveValidationErrorFor(x => x.PaisId);
    }

    [Fact]
    public void EmailInvalido_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Email = "no-es-email" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmailValido_NoDebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Email = "info@empresa.com" });
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void PuertoAfipCero_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { PuertoAfip = 0 });
        result.ShouldHaveValidationErrorFor(x => x.PuertoAfip);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateUsuarioCommandValidator
// ─────────────────────────────────────────────────────────────────────────────
public class CreateUsuarioCommandValidatorTests
{
    private readonly CreateUsuarioCommandValidator _validator = new();

    private static CreateUsuarioCommand ComandoValido() =>
        new(UserName: "juan.perez", NombreCompleto: "Juan Perez",
            Email: "juan.perez@empresa.com", SupabaseUserId: null,
            SucursalIds: [1L]);

    [Fact]
    public void ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UserNameVacio_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { UserName = "" });
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void UserNameConEspacios_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { UserName = "juan perez" });
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void EmailInvalido_DebeHaveError()
    {
        var result = _validator.TestValidate(ComandoValido() with { Email = "no-valido" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmailNulo_EsValido()
    {
        var result = _validator.TestValidate(ComandoValido() with { Email = null });
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void UserNameConCaracteresValidos_NoDebeHaveError()
    {
        // Dashes, dots, underscores, alphanumerics
        var result = _validator.TestValidate(ComandoValido() with { UserName = "user.name-01_test" });
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }
}
