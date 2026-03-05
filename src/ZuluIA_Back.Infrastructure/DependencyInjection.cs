using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.Infrastructure.Persistence;
using ZuluIA_Back.Infrastructure.Persistence.Repositories;
using ZuluIA_Back.Infrastructure.Services;

namespace ZuluIA_Back.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string no configurada.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(60);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Módulo 1 — Terceros
        services.AddScoped<ITerceroRepository, TerceroRepository>();
        // Módulo Stock/Comprobantes/Contabilidad
        services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IAsientoRepository, AsientoRepository>();
        // Módulo 2 — Sucursales y Configuración
        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
        // Módulo 3 — Precios y Planes de Pago
        services.AddScoped<IListaPreciosRepository, ListaPreciosRepository>();
        services.AddScoped<IPlanPagoRepository, PlanPagoRepository>();
        // Módulo 4 — Financiero
        services.AddScoped<ICajaRepository, CajaRepository>();
        services.AddScoped<IChequeRepository, ChequeRepository>();
        services.AddScoped<ICotizacionMonedaRepository, CotizacionMonedaRepository>();
        services.AddScoped(typeof(IRepository<FormaPagoCaja>),
                           typeof(BaseRepository<FormaPagoCaja>));
        services.AddScoped(typeof(IRepository<TipoCajaCuenta>),
                           typeof(BaseRepository<TipoCajaCuenta>));
        // Módulo 5 — Facturación
        services.AddScoped<IPuntoFacturacionRepository, PuntoFacturacionRepository>();
        services.AddScoped<ICartaPorteRepository, CartaPorteRepository>();
        services.AddScoped<IPeriodoIvaRepository, PeriodoIvaRepository>();
        // ── M6 — Usuarios, Menú y Seguridad ───────────────────────
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<ISeguridadRepository, SeguridadRepository>();
        services.AddScoped<IParametroUsuarioRepository, ParametroUsuarioRepository>();
        // ── Servicios de Dominio ──────────────────────────────────
        services.AddScoped<NumeracionComprobanteService>();
        services.AddScoped<PermisoService>();
        // ── M7 — Items, Categorías y Depósitos ────────────────────
        services.AddScoped<ICategoriaItemRepository, CategoriaItemRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IDepositoRepository, DepositoRepository>();
        // ── M8 — Stock ────────────────────────────────────────────
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IMovimientoStockRepository, MovimientoStockRepository>();
        // ── M9 — Comprobantes ─────────────────────────────────────
        services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
        services.AddScoped<IImputacionRepository, ImputacionRepository>();
        services.AddScoped(typeof(IRepository<OrdenCompraMeta>),
                           typeof(BaseRepository<OrdenCompraMeta>));
        // ── M10 ───────────────────────────────────────────────────
        services.AddScoped<ICobroRepository, CobroRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        services.AddScoped<ICuentaCorrienteRepository, CuentaCorrienteRepository>();
        services.AddScoped<IMovimientoCtaCteRepository, MovimientoCtaCteRepository>();
        services.AddScoped<ICedulonRepository, CedulonRepository>();
        services.AddScoped(typeof(IRepository<Retencion>),
                           typeof(BaseRepository<Retencion>));

        services.AddScoped(typeof(IRepository<Cobro>), typeof(BaseRepository<Cobro>));
        services.AddScoped(typeof(IRepository<CobroMedio>), typeof(BaseRepository<CobroMedio>));
        services.AddScoped(typeof(IRepository<Pago>), typeof(BaseRepository<Pago>));
        services.AddScoped(typeof(IRepository<PagoMedio>), typeof(BaseRepository<PagoMedio>));
        services.AddScoped(typeof(IRepository<AsientoLinea>), typeof(BaseRepository<AsientoLinea>));
        services.AddScoped(typeof(IRepository<ComprobanteItem>), typeof(BaseRepository<ComprobanteItem>));

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICurrentUserService, HttpCurrentUserService>();

        return services;
    }
}