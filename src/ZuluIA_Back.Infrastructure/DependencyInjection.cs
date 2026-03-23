using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Agro;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Franquicias;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
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
        services.AddMemoryCache();

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
                    maxRetryCount: 1,
                    maxRetryDelay: TimeSpan.FromSeconds(3),
                    errorCodesToAdd: null);
            });

            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Módulo 1 — Terceros
        services.AddScoped<ITerceroRepository, TerceroRepository>();
        services.AddScoped(typeof(IRepository<CategoriaCliente>),
                   typeof(BaseRepository<CategoriaCliente>));
        services.AddScoped(typeof(IRepository<CategoriaProveedor>),
                   typeof(BaseRepository<CategoriaProveedor>));
        services.AddScoped(typeof(IRepository<EstadoCliente>),
                   typeof(BaseRepository<EstadoCliente>));
        services.AddScoped(typeof(IRepository<EstadoProveedor>),
                   typeof(BaseRepository<EstadoProveedor>));
        services.AddScoped(typeof(IRepository<Seguridad>),
                   typeof(BaseRepository<Seguridad>));
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
        services.AddScoped(typeof(IRepository<Chequera>),
                   typeof(BaseRepository<Chequera>));
        services.AddScoped(typeof(IRepository<SeguimientoOrdenPago>),
                   typeof(BaseRepository<SeguimientoOrdenPago>));
        services.AddScoped(typeof(IRepository<Banco>),
               typeof(BaseRepository<Banco>));
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
        services.AddScoped(typeof(IRepository<RetencionXPersona>),
                           typeof(BaseRepository<RetencionXPersona>));
        // ── M11 ───────────────────────────────────────────────────
        services.AddScoped<IEjercicioRepository, EjercicioRepository>();
        services.AddScoped<IPlanCuentasRepository, PlanCuentasRepository>();
        services.AddScoped<IAsientoRepository, AsientoRepository>();
        services.AddScoped(typeof(IRepository<EjercicioSucursal>),
                           typeof(BaseRepository<EjercicioSucursal>));
        services.AddScoped(typeof(IRepository<PlanCuentaParametro>),
                           typeof(BaseRepository<PlanCuentaParametro>));
        services.AddScoped(typeof(IRepository<CentroCosto>),
                           typeof(BaseRepository<CentroCosto>));
        services.AddScoped(typeof(IRepository<AsientoLinea>),
                           typeof(BaseRepository<AsientoLinea>));
        // ── M12 ── Producción ─────────────────────────────────────
        services.AddScoped<IFormulaProduccionRepository, FormulaProduccionRepository>();
        services.AddScoped<IOrdenTrabajoRepository, OrdenTrabajoRepository>();
        services.AddScoped(typeof(IRepository<FormulaIngrediente>),
                           typeof(BaseRepository<FormulaIngrediente>));
        // ── M12 ── RRHH ───────────────────────────────────────────
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped(typeof(IRepository<LiquidacionSueldo>),
                           typeof(BaseRepository<LiquidacionSueldo>));
        // ── M12 ── Extras ─────────────────────────────────────────
        services.AddScoped(typeof(IRepository<Transportista>),
                           typeof(BaseRepository<Transportista>));
        services.AddScoped(typeof(IRepository<Matricula>),
                   typeof(BaseRepository<Matricula>));
        services.AddScoped(typeof(IRepository<Integradora>),
                           typeof(BaseRepository<Integradora>));
        services.AddScoped(typeof(IRepository<Atributo>),
                           typeof(BaseRepository<Atributo>));
        services.AddScoped(typeof(IRepository<AtributoItem>),
                   typeof(BaseRepository<AtributoItem>));
        services.AddScoped(typeof(IRepository<Marca>),
                   typeof(BaseRepository<Marca>));
        services.AddScoped(typeof(IRepository<Jurisdiccion>),
                   typeof(BaseRepository<Jurisdiccion>));
        services.AddScoped(typeof(IRepository<UnidadMedida>),
                   typeof(BaseRepository<UnidadMedida>));
        services.AddScoped(typeof(IRepository<Zona>),
                   typeof(BaseRepository<Zona>));
        services.AddScoped(typeof(IRepository<Timbrado>),
                   typeof(BaseRepository<Timbrado>));
        services.AddScoped(typeof(IRepository<Impuesto>),
                   typeof(BaseRepository<Impuesto>));
        services.AddScoped(typeof(IRepository<ImpuestoPorPersona>),
               typeof(BaseRepository<ImpuestoPorPersona>));
        services.AddScoped(typeof(IRepository<ImpuestoPorItem>),
               typeof(BaseRepository<ImpuestoPorItem>));
        services.AddScoped(typeof(IRepository<ImpuestoPorSucursal>),
               typeof(BaseRepository<ImpuestoPorSucursal>));
        services.AddScoped(typeof(IRepository<ImpuestoPorTipoComprobante>),
               typeof(BaseRepository<ImpuestoPorTipoComprobante>));
        services.AddScoped(typeof(IRepository<Busqueda>),
                           typeof(BaseRepository<Busqueda>));
        services.AddScoped(typeof(IRepository<OrdenPreparacion>),
                           typeof(BaseRepository<OrdenPreparacion>));
        services.AddScoped(typeof(IRepository<DescuentoComercial>),
                           typeof(BaseRepository<DescuentoComercial>));
        services.AddScoped(typeof(IRepository<Region>),
                   typeof(BaseRepository<Region>));
        services.AddScoped(typeof(IRepository<Cubo>),
                   typeof(BaseRepository<Cubo>));
        services.AddScoped(typeof(IRepository<CuboCampo>),
                   typeof(BaseRepository<CuboCampo>));
        services.AddScoped(typeof(IRepository<CuboFiltro>),
                   typeof(BaseRepository<CuboFiltro>));

        // ── Servicios de Dominio ──────────────────────────────────
        services.AddScoped<NumeracionComprobanteService>();
        services.AddScoped<PermisoService>();
        services.AddScoped<CuentaCorrienteService>();
        services.AddScoped<ContabilidadService>();
        services.AddScoped<StockService>();
        services.AddScoped<ComprobanteService>();
        services.AddScoped<ProduccionService>();

        services.AddScoped(typeof(IRepository<Cobro>), typeof(BaseRepository<Cobro>));
        services.AddScoped(typeof(IRepository<CobroMedio>), typeof(BaseRepository<CobroMedio>));
        services.AddScoped(typeof(IRepository<Pago>), typeof(BaseRepository<Pago>));
        services.AddScoped(typeof(IRepository<PagoMedio>), typeof(BaseRepository<PagoMedio>));
        services.AddScoped(typeof(IRepository<ComprobanteItem>), typeof(BaseRepository<ComprobanteItem>));
        services.AddScoped(typeof(IRepository<PlanTarjeta>), typeof(BaseRepository<PlanTarjeta>));
        services.AddScoped(typeof(IRepository<TarjetaTipo>), typeof(BaseRepository<TarjetaTipo>));

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
        services.AddHttpClient<AfipWsaaAuthService>();
        services.TryAddScoped<IAfipWsaaAuthService, CachedAfipWsaaAuthService>();
        services.AddHttpClient<IAfipWsfeCaeService, AfipWsfeCaeService>();
        services.AddHttpClient<IAfipWsfeCaeaService, AfipWsfeCaeaService>();
        services.AddHttpClient<IParaguaySifenService, ParaguaySifenService>();

        // ── Nuevas Features ──────────────────────────────────────────────────────
        services.AddScoped<IReciboRepository, ReciboRepository>();
        services.AddScoped<IRequisicionCompraRepository, RequisicionCompraRepository>();
        services.AddScoped<ICotizacionCompraRepository, CotizacionCompraRepository>();
        services.AddScoped<INotaPedidoRepository, NotaPedidoRepository>();
        services.AddScoped<ILiquidacionGranosRepository, LiquidacionGranosRepository>();
        services.AddScoped(typeof(IRepository<Caea>), typeof(BaseRepository<Caea>));
        services.AddScoped(typeof(IRepository<ComisionVendedor>), typeof(BaseRepository<ComisionVendedor>));
        services.AddScoped(typeof(IRepository<TasaInteres>), typeof(BaseRepository<TasaInteres>));
        services.AddScoped(typeof(IRepository<PlanTrabajo>), typeof(BaseRepository<PlanTrabajo>));
        services.AddScoped(typeof(IRepository<EvaluacionFranquicia>), typeof(BaseRepository<EvaluacionFranquicia>));
        services.AddScoped(typeof(IRepository<AuditoriaComprobante>), typeof(BaseRepository<AuditoriaComprobante>));
        services.AddScoped(typeof(IRepository<AuditoriaCaea>), typeof(BaseRepository<AuditoriaCaea>));
        services.AddScoped(typeof(IRepository<ObjetivoVenta>), typeof(BaseRepository<ObjetivoVenta>));
        services.AddScoped(typeof(IRepository<LiquidacionGranos>), typeof(BaseRepository<LiquidacionGranos>));

        return services;
    }
}