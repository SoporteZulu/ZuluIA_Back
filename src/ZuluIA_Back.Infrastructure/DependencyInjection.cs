using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Logistica.Services;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Entities.Colegio;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Entities.Contratos;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Fiscal;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
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

        services.AddMemoryCache();
        services.AddHttpClient<AfipWsaaAuthService>();
        services.AddHttpClient<AfipWsfeCaeService>();
        services.AddHttpClient<AfipWsfeCaeaService>();
        services.AddHttpClient<ParaguaySifenService>();
        services.AddScoped<IAfipWsaaAuthService, CachedAfipWsaaAuthService>();
        services.AddScoped<IAfipWsfeCaeService, AfipWsfeCaeService>();
        services.AddScoped<IAfipWsfeCaeaService, AfipWsfeCaeaService>();
        services.AddScoped<IParaguaySifenService, ParaguaySifenService>();

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── M1 — Terceros ────────────────────────────────────────
        services.AddScoped<ITerceroRepository, TerceroRepository>();

        // ── M2 — Sucursales y Configuración ──────────────────────
        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();

        // ── M3 — Precios y Planes de Pago ────────────────────────
        services.AddScoped<IListaPreciosRepository, ListaPreciosRepository>();
        services.AddScoped<IPlanPagoRepository, PlanPagoRepository>();

        // ── M4 — Financiero ──────────────────────────────────────
        services.AddScoped<ICajaRepository, CajaRepository>();
        services.AddScoped<IChequeRepository, ChequeRepository>();
        services.AddScoped<ICotizacionMonedaRepository, CotizacionMonedaRepository>();
        services.AddScoped<ICotizacionCompraRepository, CotizacionCompraRepository>();
        services.AddScoped<IRequisicionCompraRepository, RequisicionCompraRepository>();
        services.AddScoped<INotaPedidoRepository, NotaPedidoRepository>();
        services.AddScoped<IReciboRepository, ReciboRepository>();
        services.AddScoped<ILiquidacionGranosRepository, LiquidacionGranosRepository>();
        services.AddScoped(typeof(IRepository<FormaPagoCaja>),
                           typeof(BaseRepository<FormaPagoCaja>));
        services.AddScoped(typeof(IRepository<TipoCajaCuenta>),
                           typeof(BaseRepository<TipoCajaCuenta>));

        // ── M5 — Facturación ─────────────────────────────────────
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
        services.AddScoped(typeof(IRepository<ComprobanteItem>),
                           typeof(BaseRepository<ComprobanteItem>));
        services.AddScoped(typeof(IRepository<OrdenCompraMeta>),
                           typeof(BaseRepository<OrdenCompraMeta>));
        services.AddScoped(typeof(IRepository<Seguridad>), typeof(BaseRepository<Seguridad>));
        services.AddScoped(typeof(IRepository<CategoriaCliente>), typeof(BaseRepository<CategoriaCliente>));
        services.AddScoped(typeof(IRepository<CategoriaProveedor>), typeof(BaseRepository<CategoriaProveedor>));
        services.AddScoped(typeof(IRepository<EstadoCliente>), typeof(BaseRepository<EstadoCliente>));
        services.AddScoped(typeof(IRepository<EstadoProveedor>), typeof(BaseRepository<EstadoProveedor>));
        services.AddScoped(typeof(IRepository<EstadoCivilCatalogo>), typeof(BaseRepository<EstadoCivilCatalogo>));
        services.AddScoped(typeof(IRepository<EstadoPersonaCatalogo>), typeof(BaseRepository<EstadoPersonaCatalogo>));
        services.AddScoped(typeof(IRepository<TipoDomicilioCatalogo>), typeof(BaseRepository<TipoDomicilioCatalogo>));
        services.AddScoped(typeof(IRepository<Jurisdiccion>), typeof(BaseRepository<Jurisdiccion>));
        services.AddScoped(typeof(IRepository<Marca>), typeof(BaseRepository<Marca>));
        services.AddScoped(typeof(IRepository<UnidadMedida>), typeof(BaseRepository<UnidadMedida>));
        services.AddScoped(typeof(IRepository<Zona>), typeof(BaseRepository<Zona>));
        services.AddScoped(typeof(IRepository<Atributo>), typeof(BaseRepository<Atributo>));
        services.AddScoped(typeof(IRepository<AtributoItem>), typeof(BaseRepository<AtributoItem>));
        services.AddScoped(typeof(IRepository<Impuesto>), typeof(BaseRepository<Impuesto>));
        services.AddScoped(typeof(IRepository<ImpuestoPorSucursal>), typeof(BaseRepository<ImpuestoPorSucursal>));
        services.AddScoped(typeof(IRepository<ImpuestoPorTipoComprobante>), typeof(BaseRepository<ImpuestoPorTipoComprobante>));
        services.AddScoped(typeof(IRepository<ImpuestoPorItem>), typeof(BaseRepository<ImpuestoPorItem>));
        services.AddScoped(typeof(IRepository<ImpuestoPorPersona>), typeof(BaseRepository<ImpuestoPorPersona>));
        services.AddScoped(typeof(IRepository<Banco>), typeof(BaseRepository<Banco>));
        services.AddScoped(typeof(IRepository<Chequera>), typeof(BaseRepository<Chequera>));
        services.AddScoped(typeof(IRepository<PlanTarjeta>), typeof(BaseRepository<PlanTarjeta>));
        services.AddScoped(typeof(IRepository<TarjetaTipo>), typeof(BaseRepository<TarjetaTipo>));
        services.AddScoped(typeof(IRepository<SeguimientoOrdenPago>), typeof(BaseRepository<SeguimientoOrdenPago>));
        services.AddScoped(typeof(IRepository<Timbrado>), typeof(BaseRepository<Timbrado>));
        services.AddScoped(typeof(IRepository<Integradora>), typeof(BaseRepository<Integradora>));
        services.AddScoped(typeof(IRepository<Matricula>), typeof(BaseRepository<Matricula>));
        services.AddScoped(typeof(IRepository<Cubo>), typeof(BaseRepository<Cubo>));
        services.AddScoped(typeof(IRepository<CuboCampo>), typeof(BaseRepository<CuboCampo>));
        services.AddScoped(typeof(IRepository<CuboFiltro>), typeof(BaseRepository<CuboFiltro>));
        services.AddScoped(typeof(IRepository<Region>), typeof(BaseRepository<Region>));
        services.AddScoped(typeof(IRepository<Caea>), typeof(BaseRepository<Caea>));
        services.AddScoped(typeof(IRepository<AuditoriaCaea>), typeof(BaseRepository<AuditoriaCaea>));
        // ── M10 ───────────────────────────────────────────────────
        services.AddScoped<ICobroRepository, CobroRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        services.AddScoped<ICuentaCorrienteRepository, CuentaCorrienteRepository>();
        services.AddScoped<IMovimientoCtaCteRepository, MovimientoCtaCteRepository>();
        services.AddScoped<ICedulonRepository, CedulonRepository>();
        services.AddScoped(typeof(IRepository<Cobro>), typeof(BaseRepository<Cobro>));
        services.AddScoped(typeof(IRepository<CobroMedio>), typeof(BaseRepository<CobroMedio>));
        services.AddScoped(typeof(IRepository<Pago>), typeof(BaseRepository<Pago>));
        services.AddScoped(typeof(IRepository<PagoMedio>), typeof(BaseRepository<PagoMedio>));
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
        services.AddScoped(typeof(IRepository<PlanGeneralColegio>), typeof(BaseRepository<PlanGeneralColegio>));
        services.AddScoped(typeof(IRepository<LoteColegio>), typeof(BaseRepository<LoteColegio>));
        services.AddScoped(typeof(IRepository<CobinproColegioOperacion>), typeof(BaseRepository<CobinproColegioOperacion>));
        services.AddScoped(typeof(IRepository<ColegioReciboDetalle>), typeof(BaseRepository<ColegioReciboDetalle>));
        services.AddScoped(typeof(IRepository<ZuluIA_Back.Domain.Entities.Contratos.Contrato>),
                           typeof(BaseRepository<ZuluIA_Back.Domain.Entities.Contratos.Contrato>));
        services.AddScoped(typeof(IRepository<ContratoHistorial>), typeof(BaseRepository<ContratoHistorial>));
        services.AddScoped(typeof(IRepository<ContratoImpacto>), typeof(BaseRepository<ContratoImpacto>));
        services.AddScoped(typeof(IRepository<MarcaComercial>), typeof(BaseRepository<MarcaComercial>));
        services.AddScoped(typeof(IRepository<ZonaComercial>), typeof(BaseRepository<ZonaComercial>));
        services.AddScoped(typeof(IRepository<JurisdiccionComercial>), typeof(BaseRepository<JurisdiccionComercial>));
        services.AddScoped(typeof(IRepository<MaestroAuxiliarComercial>), typeof(BaseRepository<MaestroAuxiliarComercial>));
        services.AddScoped(typeof(IRepository<AtributoComercial>), typeof(BaseRepository<AtributoComercial>));
        services.AddScoped(typeof(IRepository<ComprobanteItemAtributoComercial>), typeof(BaseRepository<ComprobanteItemAtributoComercial>));

        // ── M11 ───────────────────────────────────────────────────
        services.AddScoped<IEjercicioRepository, EjercicioRepository>();
        services.AddScoped<IPlanCuentasRepository, PlanCuentasRepository>();
        services.AddScoped<IAsientoRepository, AsientoRepository>();
        services.AddScoped(typeof(IRepository<EjercicioSucursal>),
                           typeof(BaseRepository<EjercicioSucursal>));
        services.AddScoped(typeof(IRepository<PlanCuentaParametro>),
                           typeof(BaseRepository<PlanCuentaParametro>));
        services.AddScoped(typeof(IRepository<AsientoLinea>),
                           typeof(BaseRepository<AsientoLinea>));
        services.AddScoped(typeof(IRepository<CentroCosto>),
                           typeof(BaseRepository<CentroCosto>));
        // ── M12 ── Producción ─────────────────────────────────────
        services.AddScoped<IFormulaProduccionRepository, FormulaProduccionRepository>();
        services.AddScoped<IOrdenTrabajoRepository, OrdenTrabajoRepository>();
        services.AddScoped(typeof(IRepository<FormulaIngrediente>),
                           typeof(BaseRepository<FormulaIngrediente>));
        services.AddScoped(typeof(IRepository<FormulaProduccionHistorial>),
                           typeof(BaseRepository<FormulaProduccionHistorial>));
        services.AddScoped(typeof(IRepository<OrdenTrabajoConsumo>),
                           typeof(BaseRepository<OrdenTrabajoConsumo>));
        services.AddScoped(typeof(IRepository<ZuluIA_Back.Domain.Entities.Produccion.OrdenEmpaque>),
                           typeof(BaseRepository<ZuluIA_Back.Domain.Entities.Produccion.OrdenEmpaque>));
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

        // ── M18 ── Fiscal y Contabilidad Local ──────────────────
        services.AddScoped(typeof(IRepository<CierrePeriodoContable>), typeof(BaseRepository<CierrePeriodoContable>));
        services.AddScoped(typeof(IRepository<ReorganizacionAsientos>), typeof(BaseRepository<ReorganizacionAsientos>));
        services.AddScoped(typeof(IRepository<LibroViajanteRegistro>), typeof(BaseRepository<LibroViajanteRegistro>));
        services.AddScoped(typeof(IRepository<RentasBsAsRegistro>), typeof(BaseRepository<RentasBsAsRegistro>));
        services.AddScoped(typeof(IRepository<HechaukaRegistro>), typeof(BaseRepository<HechaukaRegistro>));
        services.AddScoped(typeof(IRepository<LiquidacionPrimariaGrano>), typeof(BaseRepository<LiquidacionPrimariaGrano>));
        services.AddScoped(typeof(IRepository<SalidaRegulatoria>), typeof(BaseRepository<SalidaRegulatoria>));

        // ── M19 ── POS / Punto de Venta / Fiscal alternativo ─────
        services.AddScoped(typeof(IRepository<TimbradoFiscal>), typeof(BaseRepository<TimbradoFiscal>));
        services.AddScoped(typeof(IRepository<OperacionPuntoVenta>), typeof(BaseRepository<OperacionPuntoVenta>));
        services.AddScoped(typeof(IRepository<SifenOperacion>), typeof(BaseRepository<SifenOperacion>));
        services.AddScoped(typeof(IRepository<DeuceOperacion>), typeof(BaseRepository<DeuceOperacion>));

        services.AddScoped<NumeracionComprobanteService>();
        services.AddScoped<PermisoService>();
        services.AddScoped<IAfipCaeComprobanteService, ZuluIA_Back.Application.Features.Comprobantes.Services.AfipCaeComprobanteService>();
        services.AddScoped<IParaguaySifenComprobanteService, ZuluIA_Back.Application.Features.Comprobantes.Services.ParaguaySifenComprobanteService>();
        services.AddScoped<CuentaCorrienteService>();
        services.AddScoped<ContabilidadService>();
        services.AddScoped<StockService>();
        services.AddScoped<ComprobanteService>();
        services.AddScoped<LogisticaInternaService>();
        services.AddScoped<ProduccionService>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
        services.AddScoped<IPasswordHasherService, Pbkdf2PasswordHasherService>();

        return services;
    }
}