using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Colegio;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Contratos;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Fiscal;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Ventas;
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
        services.AddScoped(typeof(IRepository<AfipWsfeConfiguracion>), typeof(BaseRepository<AfipWsfeConfiguracion>));
        services.AddScoped(typeof(IRepository<AfipWsfeAudit>), typeof(BaseRepository<AfipWsfeAudit>));
        services.AddScoped(typeof(IRepository<CartaPorteEvento>), typeof(BaseRepository<CartaPorteEvento>));
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
        services.AddScoped<IChequeRepository, ChequeRepository>();
        services.AddScoped<ICobroRepository, CobroRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        services.AddScoped<ICuentaCorrienteRepository, CuentaCorrienteRepository>();
        services.AddScoped<IMovimientoCtaCteRepository, MovimientoCtaCteRepository>();
        services.AddScoped<ICedulonRepository, CedulonRepository>();
        services.AddScoped(typeof(IRepository<PlanGeneralColegio>), typeof(BaseRepository<PlanGeneralColegio>));
        services.AddScoped(typeof(IRepository<LoteColegio>), typeof(BaseRepository<LoteColegio>));
        services.AddScoped(typeof(IRepository<CobinproColegioOperacion>), typeof(BaseRepository<CobinproColegioOperacion>));
        services.AddScoped(typeof(IRepository<ColegioReciboDetalle>), typeof(BaseRepository<ColegioReciboDetalle>));
        services.AddScoped(typeof(IRepository<Contrato>), typeof(BaseRepository<Contrato>));
        services.AddScoped(typeof(IRepository<ContratoHistorial>), typeof(BaseRepository<ContratoHistorial>));
        services.AddScoped(typeof(IRepository<ContratoImpacto>), typeof(BaseRepository<ContratoImpacto>));
        services.AddScoped(typeof(IRepository<MarcaComercial>), typeof(BaseRepository<MarcaComercial>));
        services.AddScoped(typeof(IRepository<ZonaComercial>), typeof(BaseRepository<ZonaComercial>));
        services.AddScoped(typeof(IRepository<JurisdiccionComercial>), typeof(BaseRepository<JurisdiccionComercial>));
        services.AddScoped(typeof(IRepository<MaestroAuxiliarComercial>), typeof(BaseRepository<MaestroAuxiliarComercial>));
        services.AddScoped(typeof(IRepository<AtributoComercial>), typeof(BaseRepository<AtributoComercial>));
        services.AddScoped(typeof(IRepository<ComprobanteItemAtributoComercial>), typeof(BaseRepository<ComprobanteItemAtributoComercial>));
        services.AddScoped(typeof(IRepository<ChequeHistorial>),
                           typeof(BaseRepository<ChequeHistorial>));
        services.AddScoped(typeof(IRepository<Retencion>),
                           typeof(BaseRepository<Retencion>));
        services.AddScoped(typeof(IRepository<RetencionXPersona>),
                           typeof(BaseRepository<RetencionXPersona>));
        services.AddScoped(typeof(IRepository<DescuentoComercial>),
                           typeof(BaseRepository<DescuentoComercial>));
        services.AddScoped(typeof(IRepository<TesoreriaMovimiento>),
                           typeof(BaseRepository<TesoreriaMovimiento>));
        services.AddScoped(typeof(IRepository<TesoreriaCierre>),
                           typeof(BaseRepository<TesoreriaCierre>));
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
        services.AddScoped(typeof(IRepository<FormulaProduccionHistorial>),
                           typeof(BaseRepository<FormulaProduccionHistorial>));
        services.AddScoped(typeof(IRepository<OrdenTrabajoConsumo>),
                           typeof(BaseRepository<OrdenTrabajoConsumo>));
        services.AddScoped(typeof(IRepository<OrdenEmpaque>),
                           typeof(BaseRepository<OrdenEmpaque>));
        // ── M12 ── RRHH ───────────────────────────────────────────
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped(typeof(IRepository<LiquidacionSueldo>),
                           typeof(BaseRepository<LiquidacionSueldo>));
        services.AddScoped(typeof(IRepository<ComprobanteEmpleado>),
                           typeof(BaseRepository<ComprobanteEmpleado>));
        services.AddScoped(typeof(IRepository<ImputacionEmpleado>),
                           typeof(BaseRepository<ImputacionEmpleado>));
        // ── M12 ── Extras ─────────────────────────────────────────
        services.AddScoped(typeof(IRepository<Transportista>),
                           typeof(BaseRepository<Transportista>));
        services.AddScoped(typeof(IRepository<Busqueda>),
                           typeof(BaseRepository<Busqueda>));
        services.AddScoped(typeof(IRepository<OrdenCarga>),
                           typeof(BaseRepository<OrdenCarga>));
        services.AddScoped(typeof(IRepository<OrdenPreparacion>),
                           typeof(BaseRepository<OrdenPreparacion>));
        services.AddScoped(typeof(IRepository<OrdenPreparacionDetalle>),
                           typeof(BaseRepository<OrdenPreparacionDetalle>));
        services.AddScoped(typeof(IRepository<TransferenciaDeposito>),
                           typeof(BaseRepository<TransferenciaDeposito>));
        services.AddScoped(typeof(IRepository<TransferenciaDepositoDetalle>),
                           typeof(BaseRepository<TransferenciaDepositoDetalle>));
        services.AddScoped(typeof(IRepository<LogisticaInternaEvento>),
                           typeof(BaseRepository<LogisticaInternaEvento>));

        // ── M16 ── Diagnóstico / Plan de Trabajo ────────────────
        services.AddScoped(typeof(IRepository<RegionDiagnostica>), typeof(BaseRepository<RegionDiagnostica>));
        services.AddScoped(typeof(IRepository<AspectoDiagnostico>), typeof(BaseRepository<AspectoDiagnostico>));
        services.AddScoped(typeof(IRepository<VariableDiagnostica>), typeof(BaseRepository<VariableDiagnostica>));
        services.AddScoped(typeof(IRepository<VariableDiagnosticaOpcion>), typeof(BaseRepository<VariableDiagnosticaOpcion>));
        services.AddScoped(typeof(IRepository<PlantillaDiagnostica>), typeof(BaseRepository<PlantillaDiagnostica>));
        services.AddScoped(typeof(IRepository<PlantillaDiagnosticaVariable>), typeof(BaseRepository<PlantillaDiagnosticaVariable>));
        services.AddScoped(typeof(IRepository<PlanillaDiagnostica>), typeof(BaseRepository<PlanillaDiagnostica>));
        services.AddScoped(typeof(IRepository<PlanillaDiagnosticaRespuesta>), typeof(BaseRepository<PlanillaDiagnosticaRespuesta>));

        // ── M17 ── Integraciones / Importaciones / Sync ─────────
        services.AddScoped(typeof(IRepository<ProcesoIntegracionJob>), typeof(BaseRepository<ProcesoIntegracionJob>));
        services.AddScoped(typeof(IRepository<ProcesoIntegracionLog>), typeof(BaseRepository<ProcesoIntegracionLog>));
        services.AddScoped(typeof(IRepository<MonitorExportacion>), typeof(BaseRepository<MonitorExportacion>));
        services.AddScoped(typeof(IRepository<IntegracionExternaAudit>), typeof(BaseRepository<IntegracionExternaAudit>));
        services.AddScoped(typeof(IRepository<BatchProgramacion>), typeof(BaseRepository<BatchProgramacion>));
        services.AddScoped(typeof(IRepository<ImpresionSpoolTrabajo>), typeof(BaseRepository<ImpresionSpoolTrabajo>));

        // ── M19 ── POS / Punto de Venta / Fiscal alternativo ─────
        services.AddScoped(typeof(IRepository<TimbradoFiscal>), typeof(BaseRepository<TimbradoFiscal>));
        services.AddScoped(typeof(IRepository<OperacionPuntoVenta>), typeof(BaseRepository<OperacionPuntoVenta>));
        services.AddScoped(typeof(IRepository<SifenOperacion>), typeof(BaseRepository<SifenOperacion>));
        services.AddScoped(typeof(IRepository<DeuceOperacion>), typeof(BaseRepository<DeuceOperacion>));

        // ── M18 ── Fiscal y Contabilidad Local ──────────────────
        services.AddScoped(typeof(IRepository<CierrePeriodoContable>), typeof(BaseRepository<CierrePeriodoContable>));
        services.AddScoped(typeof(IRepository<ReorganizacionAsientos>), typeof(BaseRepository<ReorganizacionAsientos>));
        services.AddScoped(typeof(IRepository<LibroViajanteRegistro>), typeof(BaseRepository<LibroViajanteRegistro>));
        services.AddScoped(typeof(IRepository<RentasBsAsRegistro>), typeof(BaseRepository<RentasBsAsRegistro>));
        services.AddScoped(typeof(IRepository<HechaukaRegistro>), typeof(BaseRepository<HechaukaRegistro>));
        services.AddScoped(typeof(IRepository<LiquidacionPrimariaGrano>), typeof(BaseRepository<LiquidacionPrimariaGrano>));
        services.AddScoped(typeof(IRepository<SalidaRegulatoria>), typeof(BaseRepository<SalidaRegulatoria>));

        services.AddScoped<NumeracionComprobanteService>();
        services.AddScoped<PermisoService>();
        services.AddScoped<CuentaCorrienteService>();
        services.AddScoped<ContabilidadService>();
        services.AddScoped<StockService>();
        services.AddScoped<ComprobanteService>();
        services.AddScoped<CuentaCorrienteService>();
        services.AddScoped<ContabilidadService>();
        services.AddScoped<ProduccionService>();

        services.AddScoped(typeof(IRepository<Cobro>), typeof(BaseRepository<Cobro>));
        services.AddScoped(typeof(IRepository<CobroMedio>), typeof(BaseRepository<CobroMedio>));
        services.AddScoped(typeof(IRepository<Pago>), typeof(BaseRepository<Pago>));
        services.AddScoped(typeof(IRepository<PagoMedio>), typeof(BaseRepository<PagoMedio>));
        services.AddScoped(typeof(IRepository<AsientoLinea>), typeof(BaseRepository<AsientoLinea>));
        services.AddScoped(typeof(IRepository<ComprobanteItem>), typeof(BaseRepository<ComprobanteItem>));

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
        services.AddScoped<IPasswordHasherService, Pbkdf2PasswordHasherService>();

        return services;
    }
}