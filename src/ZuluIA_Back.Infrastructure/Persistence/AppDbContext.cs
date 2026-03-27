using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Colegio;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Contratos;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Fiscal;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.RRHH;
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
    public DbSet<TerceroPerfilComercial> TercerosPerfilesComerciales => Set<TerceroPerfilComercial>();
    public DbSet<TerceroContacto> TercerosContactos => Set<TerceroContacto>();
    public DbSet<TerceroSucursalEntrega> TercerosSucursalesEntrega => Set<TerceroSucursalEntrega>();
    public DbSet<TerceroTransporte> TercerosTransportes => Set<TerceroTransporte>();
    public DbSet<TerceroVentanaCobranza> TercerosVentanasCobranza => Set<TerceroVentanaCobranza>();

    // ─── Catálogos de referencia ──────────────────────────────────────────────
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<CondicionIva> CondicionesIva => Set<CondicionIva>();
    public DbSet<CategoriaTercero> CategoriasTerceros => Set<CategoriaTercero>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<TipoComprobante> TiposComprobante => Set<TipoComprobante>();
    public DbSet<AlicuotaIva> AlicuotasIva => Set<AlicuotaIva>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<FormaPago> FormasPago => Set<FormaPago>();
    public DbSet<MarcaComercial> MarcasComerciales => Set<MarcaComercial>();
    public DbSet<ZonaComercial> ZonasComerciales => Set<ZonaComercial>();
    public DbSet<JurisdiccionComercial> JurisdiccionesComerciales => Set<JurisdiccionComercial>();
    public DbSet<MaestroAuxiliarComercial> MaestrosAuxiliaresComerciales => Set<MaestroAuxiliarComercial>();
    public DbSet<AtributoComercial> AtributosComerciales => Set<AtributoComercial>();
    public DbSet<ComprobanteItemAtributoComercial> ComprobantesItemsAtributos => Set<ComprobanteItemAtributoComercial>();

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
    public DbSet<AfipWsfeConfiguracion> AfipWsfeConfiguraciones => Set<AfipWsfeConfiguracion>();
    public DbSet<AfipWsfeAudit> AfipWsfeAudits => Set<AfipWsfeAudit>();
    public DbSet<CartaPorte> CartasPorte => Set<CartaPorte>();
    public DbSet<CartaPorteEvento> CartasPorteEventos => Set<CartaPorteEvento>();
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
    // ── M10 — Cobros, Pagos y Cuenta Corriente ─────────────────────
    public DbSet<ChequeHistorial> ChequesHistorial => Set<ChequeHistorial>();
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<CobroMedio> CobrosMedios => Set<CobroMedio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<PagoMedio> PagosMedios => Set<PagoMedio>();
    public DbSet<Retencion> Retenciones => Set<Retencion>();
    public DbSet<TipoRetencion> TiposRetencion => Set<TipoRetencion>();
    public DbSet<EscalaRetencion> EscalasRetencion => Set<EscalaRetencion>();
    public DbSet<TransferenciaCaja> TransferenciasCaja => Set<TransferenciaCaja>();
    public DbSet<TesoreriaMovimiento> TesoreriaMovimientos => Set<TesoreriaMovimiento>();
    public DbSet<TesoreriaCierre> TesoreriaCierres => Set<TesoreriaCierre>();
    public DbSet<CuentaCorriente> CuentaCorriente => Set<CuentaCorriente>();
    public DbSet<MovimientoCtaCte> MovimientosCtaCte => Set<MovimientoCtaCte>();
    public DbSet<Cedulon> Cedulones => Set<Cedulon>();
    public DbSet<PlanGeneralColegio> ColegioPlanesGenerales => Set<PlanGeneralColegio>();
    public DbSet<LoteColegio> ColegioLotes => Set<LoteColegio>();
    public DbSet<CobinproColegioOperacion> ColegioCobinproOperaciones => Set<CobinproColegioOperacion>();
    public DbSet<ColegioReciboDetalle> ColegioRecibosDetalles => Set<ColegioReciboDetalle>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ContratoHistorial> ContratosHistorial => Set<ContratoHistorial>();
    public DbSet<ContratoImpacto> ContratosImpactos => Set<ContratoImpacto>();
    // ── M11 — Contabilidad ─────────────────────────────────────────
    public DbSet<Ejercicio> Ejercicios => Set<Ejercicio>();
    public DbSet<EjercicioSucursal> EjercicioSucursales => Set<EjercicioSucursal>();
    public DbSet<PlanCuenta> PlanCuentas => Set<PlanCuenta>();
    public DbSet<PlanCuentaParametro> PlanCuentasParametros => Set<PlanCuentaParametro>();
    public DbSet<CentroCosto> CentrosCosto => Set<CentroCosto>();
    public DbSet<Asiento> Asientos => Set<Asiento>();
    public DbSet<AsientoLinea> AsientosLineas => Set<AsientoLinea>();
    // ── M12 — Producción ──────────────────────────────────────────
    public DbSet<FormulaProduccion> FormulasProduccion => Set<FormulaProduccion>();
    public DbSet<FormulaIngrediente> FormulaIngredientes => Set<FormulaIngrediente>();
    public DbSet<FormulaProduccionHistorial> FormulasProduccionHistorial => Set<FormulaProduccionHistorial>();
    public DbSet<OrdenTrabajo> OrdenesTrabajo => Set<OrdenTrabajo>();
    public DbSet<OrdenTrabajoConsumo> OrdenesTrabajoConsumos => Set<OrdenTrabajoConsumo>();
    public DbSet<OrdenEmpaque> OrdenesEmpaque => Set<OrdenEmpaque>();

    // ── M12 — RRHH ────────────────────────────────────────────────
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<LiquidacionSueldo> LiquidacionesSueldo => Set<LiquidacionSueldo>();
    public DbSet<ComprobanteEmpleado> ComprobantesEmpleados => Set<ComprobanteEmpleado>();
    public DbSet<ImputacionEmpleado> ImputacionesEmpleados => Set<ImputacionEmpleado>();

    // ── M12 — Extras ──────────────────────────────────────────────
    public DbSet<Transportista> Transportistas => Set<Transportista>();
    public DbSet<Busqueda> Busquedas => Set<Busqueda>();

    // ── M13 — Logística / Picking ─────────────────────────────────
    public DbSet<OrdenCarga> OrdenesCarga => Set<OrdenCarga>();
    public DbSet<OrdenPreparacion> OrdenesPreparacion => Set<OrdenPreparacion>();
    public DbSet<OrdenPreparacionDetalle> OrdenesPreparacionDetalles => Set<OrdenPreparacionDetalle>();
    public DbSet<TransferenciaDeposito> TransferenciasDeposito => Set<TransferenciaDeposito>();
    public DbSet<TransferenciaDepositoDetalle> TransferenciasDepositoDetalles => Set<TransferenciaDepositoDetalle>();
    public DbSet<LogisticaInternaEvento> LogisticaInternaEventos => Set<LogisticaInternaEvento>();

    // ── M14 — Descuentos Comerciales ──────────────────────────────
    public DbSet<DescuentoComercial> DescuentosComerciales => Set<DescuentoComercial>();

    // ── M15 — Retenciones por Persona ──────────────────────────
    public DbSet<RetencionXPersona> RetencionesPorPersona => Set<RetencionXPersona>();

    // ── M16 — Diagnóstico / Plan de Trabajo ────────────────────
    public DbSet<RegionDiagnostica> RegionesDiagnosticas => Set<RegionDiagnostica>();
    public DbSet<AspectoDiagnostico> AspectosDiagnostico => Set<AspectoDiagnostico>();
    public DbSet<VariableDiagnostica> VariablesDiagnosticas => Set<VariableDiagnostica>();
    public DbSet<VariableDiagnosticaOpcion> VariablesDiagnosticasOpciones => Set<VariableDiagnosticaOpcion>();
    public DbSet<PlantillaDiagnostica> PlantillasDiagnosticas => Set<PlantillaDiagnostica>();
    public DbSet<PlantillaDiagnosticaVariable> PlantillasDiagnosticasVariables => Set<PlantillaDiagnosticaVariable>();
    public DbSet<PlanillaDiagnostica> PlanillasDiagnosticas => Set<PlanillaDiagnostica>();
    public DbSet<PlanillaDiagnosticaRespuesta> PlanillasDiagnosticasRespuestas => Set<PlanillaDiagnosticaRespuesta>();

    // ── M17 — Integraciones / Importaciones / Sync ─────────────
    public DbSet<ProcesoIntegracionJob> ProcesosIntegracionJobs => Set<ProcesoIntegracionJob>();
    public DbSet<ProcesoIntegracionLog> ProcesosIntegracionLogs => Set<ProcesoIntegracionLog>();
    public DbSet<MonitorExportacion> MonitoresExportacion => Set<MonitorExportacion>();
    public DbSet<IntegracionExternaAudit> IntegracionesExternasAudit => Set<IntegracionExternaAudit>();
    public DbSet<BatchProgramacion> BatchProgramaciones => Set<BatchProgramacion>();
    public DbSet<ImpresionSpoolTrabajo> ImpresionSpoolTrabajos => Set<ImpresionSpoolTrabajo>();

    // ── M19 — POS / Punto de Venta / Fiscal alternativo ──────────
    public DbSet<TimbradoFiscal> TimbradosFiscales => Set<TimbradoFiscal>();
    public DbSet<OperacionPuntoVenta> OperacionesPuntoVenta => Set<OperacionPuntoVenta>();
    public DbSet<SifenOperacion> SifenOperaciones => Set<SifenOperacion>();
    public DbSet<DeuceOperacion> DeuceOperaciones => Set<DeuceOperacion>();

    // ── M18 — Fiscal y Contabilidad Local ──────────────────────
    public DbSet<CierrePeriodoContable> CierresPeriodoContable => Set<CierrePeriodoContable>();
    public DbSet<ReorganizacionAsientos> ReorganizacionesAsientos => Set<ReorganizacionAsientos>();
    public DbSet<LibroViajanteRegistro> LibrosViajantesRegistros => Set<LibroViajanteRegistro>();
    public DbSet<RentasBsAsRegistro> RentasBsAsRegistros => Set<RentasBsAsRegistro>();
    public DbSet<HechaukaRegistro> HechaukaRegistros => Set<HechaukaRegistro>();
    public DbSet<LiquidacionPrimariaGrano> LiquidacionesPrimariasGranos => Set<LiquidacionPrimariaGrano>();
    public DbSet<SalidaRegulatoria> SalidasRegulatorias => Set<SalidaRegulatoria>();

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
