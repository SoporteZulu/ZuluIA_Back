using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Items;
//using ZuluIA_Back.Domain.Entities.Referencia;   // <- descomentar cuando los agregues
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    // ─── Geografía ────────────────────────────────────────────────────────────
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Provincia> Provincias => Set<Provincia>();
    public DbSet<Localidad> Localidades => Set<Localidad>();
    public DbSet<Barrio> Barrios => Set<Barrio>();

    // ─── Terceros ─────────────────────────────────────────────────────────────
    public DbSet<Tercero> Terceros => Set<Tercero>();

    // ─── Catálogos de referencia ──────────────────────────────────────────────
    // Descomenta cuando existan los modelos reales
    // public DbSet<TipoDocumento>    TiposDocumento    => Set<TipoDocumento>();
    // public DbSet<CondicionIva>     CondicionesIva    => Set<CondicionIva>();
    // public DbSet<CategoriaTercero> CategoriasTerceros=> Set<CategoriaTercero>();
    // public DbSet<Moneda>           Monedas           => Set<Moneda>();
    // public DbSet<Usuario>          Usuarios          => Set<Usuario>();
    // public DbSet<Sucursal>         Sucursales        => Set<Sucursal>();

    // Métodos temporales de consulta raw (NO los dejes cuando tengas los modelos)
    public async Task<IReadOnlyList<string>> GetTipoDocumentosRawAsync(CancellationToken ct = default)
        => await Database.SqlQueryRaw<string>("SELECT descripcion FROM tipos_documento").ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetCondicionesIvaRawAsync(CancellationToken ct = default)
        => await Database.SqlQueryRaw<string>("SELECT descripcion FROM condiciones_iva").ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetCategoriasTerceroRawAsync(CancellationToken ct = default)
        => await Database.SqlQueryRaw<string>("SELECT descripcion FROM categorias_terceros").ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetMonedasRawAsync(CancellationToken ct = default)
        => await Database.SqlQueryRaw<string>("SELECT descripcion FROM monedas").ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetUsuariosRawAsync(CancellationToken ct = default)
        => await Database.SqlQueryRaw<string>("SELECT usuario FROM usuarios").ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetSucursalesRawAsync(CancellationToken ct = default)
        => await Database.SqlQueryRaw<string>("SELECT nombre_fantasia FROM sucursales").ToListAsync(ct);

    // ─── Items ────────────────────────────────────────────────────────────────
    public DbSet<Item> Items => Set<Item>();

    // ─── Stock ────────────────────────────────────────────────────────────────
    public DbSet<StockItem> Stock => Set<StockItem>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();

    // ─── Comprobantes ─────────────────────────────────────────────────────────
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<ComprobanteItem> ComprobantesItems => Set<ComprobanteItem>();

    // ─── Finanzas ─────────────────────────────────────────────────────────────
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<CobroMedio> CobrosMedios => Set<CobroMedio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<PagoMedio> PagosMedios => Set<PagoMedio>();

    // ─── Contabilidad ────────────────────────────────���────────────────────────
    public DbSet<Asiento> Asientos => Set<Asiento>();
    public DbSet<AsientoLinea> AsientosLineas => Set<AsientoLinea>();

    // ─── EF Core / Dominio ────────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        DispatchDomainEvents();
        return await base.SaveChangesAsync(ct);
    }

    private void DispatchDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
            entity.ClearDomainEvents();
    }
}