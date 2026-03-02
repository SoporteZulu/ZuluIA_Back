using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Items;
//using ZuluIA_Back.Domain.Entities.Referencia;   // <- descomentar cuando tengas esos modelos
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;

// SÓLO la interfaz, nunca implementaciones aquí
namespace ZuluIA_Back.Application.Common.Interfaces;

/// <summary>
/// Abstracción del DbContext para que la capa Application
/// no dependa directamente de EF Core ni de Infrastructure.
/// Cada Handler que necesite acceso a catálogos de referencia
/// lo hace a través de esta interfaz.
/// </summary>
public interface IApplicationDbContext
{
    // ─── Geografía ────────────────────────────────────────────────────────────
    DbSet<Pais> Paises { get; }
    DbSet<Provincia> Provincias { get; }
    DbSet<Localidad> Localidades { get; }
    DbSet<Barrio> Barrios { get; }

    // ─── Terceros ─────────────────────────────────────────────────────────────
    DbSet<Tercero> Terceros { get; }

    // ─── Catálogos de referencia ──────────────────────────────────────────────
    // Descomenta cuando agregues los modelos en Domain.Entities.Referencia
    // DbSet<TipoDocumento>    TiposDocumento    { get; }
    // DbSet<CondicionIva>     CondicionesIva    { get; }
    // DbSet<CategoriaTercero> CategoriasTerceros{ get; }
    // DbSet<Moneda>           Monedas           { get; }
    // DbSet<Usuario>          Usuarios          { get; }
    // DbSet<Sucursal>         Sucursales        { get; }

    // Métodos temporales para acceso raw, elimina cuando agregues los modelos reales
    Task<IReadOnlyList<string>> GetTipoDocumentosRawAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCondicionesIvaRawAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCategoriasTerceroRawAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetMonedasRawAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetUsuariosRawAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetSucursalesRawAsync(CancellationToken ct = default);

    // ─── Items ────────────────────────────────────────────────────────────────
    DbSet<Item> Items { get; }

    // ─── Stock ────────────────────────────────────────────────────────────────
    DbSet<StockItem> Stock { get; }
    DbSet<MovimientoStock> MovimientosStock { get; }

    // ─── Comprobantes ─────────────────────────────────────────────────────────
    DbSet<Comprobante> Comprobantes { get; }
    DbSet<ComprobanteItem> ComprobantesItems { get; }

    // ─── Finanzas ─────────────────────────────────────────────────────────────
    DbSet<Cobro> Cobros { get; }
    DbSet<CobroMedio> CobrosMedios { get; }
    DbSet<Pago> Pagos { get; }
    DbSet<PagoMedio> PagosMedios { get; }

    // ─── Contabilidad ─────────────────────────────────────────────────────────
    DbSet<Asiento> Asientos { get; }
    DbSet<AsientoLinea> AsientosLineas { get; }

    // ─── Persistencia ─────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}