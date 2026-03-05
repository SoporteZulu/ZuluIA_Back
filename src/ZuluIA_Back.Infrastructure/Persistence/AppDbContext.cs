using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    // ─── Geografía ────────────────────────────────────────────────────────────
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Provincia> Provincias => Set<Provincia>();
    public DbSet<Localidad> Localidades => Set<Localidad>();
    public DbSet<Barrio> Barrios => Set<Barrio>();

    // ─── Sucursales y Configuración ───────────────────────────────────────────
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<ConfiguracionSistema> Config => Set<ConfiguracionSistema>();

    // ─── Terceros ─────────────────────────────────────────────────────────────
    public DbSet<Tercero> Terceros => Set<Tercero>();

    // ─── Catálogos de referencia ──────────────────────────────────────────────
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<CondicionIva> CondicionesIva => Set<CondicionIva>();
    public DbSet<CategoriaTercero> CategoriasTerceros => Set<CategoriaTercero>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<TipoComprobante> TiposComprobante => Set<TipoComprobante>();
    public DbSet<AlicuotaIva> AlicuotasIva => Set<AlicuotaIva>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<FormaPago> FormasPago => Set<FormaPago>();


    // ─── Finanzas ─────────────────────────────────────────────────────────────
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<CobroMedio> CobrosMedios => Set<CobroMedio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<PagoMedio> PagosMedios => Set<PagoMedio>();

    // ─── Contabilidad ─────────────────────────────────────────────────────────
    public DbSet<Asiento> Asientos => Set<Asiento>();
    public DbSet<AsientoLinea> AsientosLineas => Set<AsientoLinea>();

    // ── Módulo 3 — Precios y Ventas ──────────────────────────────
    public DbSet<ListaPrecios> ListasPrecios => Set<ListaPrecios>();
    public DbSet<ListaPreciosItem> ListaPreciosItems => Set<ListaPreciosItem>();
    public DbSet<PlanPago> PlanesPago => Set<PlanPago>();

    // ── Módulo 4 — Financiero ────────────────────────────────────
    public DbSet<CajaCuentaBancaria> CajasCuentasBancarias => Set<CajaCuentaBancaria>();
    public DbSet<TipoCajaCuenta> TiposCajaCuenta => Set<TipoCajaCuenta>();
    public DbSet<Cheque> Cheques => Set<Cheque>();
    public DbSet<CotizacionMoneda> CotizacionesMoneda => Set<CotizacionMoneda>();
    public DbSet<FormaPagoCaja> FormasPagoCaja => Set<FormaPagoCaja>();

    // ── Módulo 5 — Facturación ────────────────────────────────────
    public DbSet<TipoPuntoFacturacion> TiposPuntoFacturacion => Set<TipoPuntoFacturacion>();
    public DbSet<PuntoFacturacion> PuntosFacturacion => Set<PuntoFacturacion>();
    public DbSet<CartaPorte> CartasPorte => Set<CartaPorte>();
    public DbSet<PeriodoIva> PeriodosIva => Set<PeriodoIva>();

    // ── Módulo 6 — Usuarios, Menú y Seguridad ────────────────────
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioSucursal> UsuariosSucursal => Set<UsuarioSucursal>();
    public DbSet<MenuItem> Menu => Set<MenuItem>();
    public DbSet<MenuUsuario> MenuUsuario => Set<MenuUsuario>();
    public DbSet<Seguridad> Seguridad => Set<Seguridad>();
    public DbSet<SeguridadUsuario> SeguridadUsuario => Set<SeguridadUsuario>();
    public DbSet<ParametroUsuario> ParametrosUsuario => Set<ParametroUsuario>();

    // ── Módulo 7 — Items, Categorías y Depósitos ──────────────────
    public DbSet<CategoriaItem> CategoriasItems => Set<CategoriaItem>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Deposito> Depositos => Set<Deposito>();
    // ── M8 — Stock ────────────────────────────────────────────────
    public DbSet<StockItem> Stock => Set<StockItem>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    // ── M9 — Comprobantes ─────────────────────────────────────────
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<ComprobanteItem> ComprobantesItems => Set<ComprobanteItem>();
    public DbSet<Imputacion> Imputaciones => Set<Imputacion>();
    public DbSet<OrdenCompraMeta> OrdenesCompraMeta => Set<OrdenCompraMeta>();

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
