using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Agro;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Entities.CRM;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Colegio;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Contratos;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Fiscal;
using ZuluIA_Back.Domain.Entities.Franquicias;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.Proyectos;
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
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<TipoComprobanteSucursal> TiposComprobantesSucursal => Set<TipoComprobanteSucursal>();
    public DbSet<SucursalDomicilio> SucursalesDomicilio => Set<SucursalDomicilio>();
    public DbSet<SucursalMedioContacto> SucursalesMedioContacto => Set<SucursalMedioContacto>();
    public DbSet<ConfiguracionSistema> Config => Set<ConfiguracionSistema>();
    public DbSet<Variable> Variables => Set<Variable>();
    public DbSet<Aspecto> Aspectos => Set<Aspecto>();
    public DbSet<OpcionVariable> OpcionesVariable => Set<OpcionVariable>();
    public DbSet<VariableDetalle> VariablesDetalle => Set<VariableDetalle>();
    public DbSet<PlantillaDiagnostico> PlantillasDiagnostico => Set<PlantillaDiagnostico>();
    public DbSet<PlantillaDiagnosticoDetalle> PlantillasDiagnosticoDetalle => Set<PlantillaDiagnosticoDetalle>();
    public DbSet<PlanillaDiagnostico> PlanillasDiagnostico => Set<PlanillaDiagnostico>();
    public DbSet<PlanillaDiagnosticoDetalle> PlanillasDiagnosticoDetalle => Set<PlanillaDiagnosticoDetalle>();

    // ─── Terceros ─────────────────────────────────────────────────────────────
    public DbSet<Tercero> Terceros => Set<Tercero>();
    public DbSet<PersonaDomicilio> PersonasDomicilios => Set<PersonaDomicilio>();
    public DbSet<MedioContacto> MediosContacto => Set<MedioContacto>();
    public DbSet<TipoDomicilioCatalogo> TiposDomicilio => Set<TipoDomicilioCatalogo>();
    public DbSet<TerceroPerfilComercial> TercerosPerfilesComerciales => Set<TerceroPerfilComercial>();
    public DbSet<TerceroContacto> TercerosContactos => Set<TerceroContacto>();
    public DbSet<TerceroSucursalEntrega> TercerosSucursalesEntrega => Set<TerceroSucursalEntrega>();
    public DbSet<TerceroTransporte> TercerosTransportes => Set<TerceroTransporte>();
    public DbSet<TerceroVentanaCobranza> TercerosVentanasCobranza => Set<TerceroVentanaCobranza>();
    public DbSet<CategoriaCliente> CategoriasClientes => Set<CategoriaCliente>();
    public DbSet<CategoriaProveedor> CategoriasProveedores => Set<CategoriaProveedor>();
    public DbSet<EstadoCliente> EstadosClientes => Set<EstadoCliente>();
    public DbSet<EstadoProveedor> EstadosProveedores => Set<EstadoProveedor>();
    public DbSet<EstadoPersonaCatalogo> EstadosPersonas => Set<EstadoPersonaCatalogo>();
    public DbSet<EstadoCivilCatalogo> EstadosCiviles => Set<EstadoCivilCatalogo>();

    // ─── Catálogos de referencia ──────────────────────────────────────────────
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<CondicionIva> CondicionesIva => Set<CondicionIva>();
    public DbSet<CategoriaTercero> CategoriasTerceros => Set<CategoriaTercero>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<TipoComprobante> TiposComprobante => Set<TipoComprobante>();
    public DbSet<MotivoDebito> MotivosDebito => Set<MotivoDebito>();
    public DbSet<AlicuotaIva> AlicuotasIva => Set<AlicuotaIva>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<FormaPago> FormasPago => Set<FormaPago>();
    public DbSet<MarcaComercial> MarcasComerciales => Set<MarcaComercial>();
    public DbSet<ZonaComercial> ZonasComerciales => Set<ZonaComercial>();
    public DbSet<JurisdiccionComercial> JurisdiccionesComerciales => Set<JurisdiccionComercial>();
    public DbSet<MaestroAuxiliarComercial> MaestrosAuxiliaresComerciales => Set<MaestroAuxiliarComercial>();
    public DbSet<AtributoComercial> AtributosComerciales => Set<AtributoComercial>();
    public DbSet<ComprobanteItemAtributoComercial> ComprobantesItemsAtributos => Set<ComprobanteItemAtributoComercial>();
    public DbSet<ItemAtributoComercial> ItemsAtributosComerciales => Set<ItemAtributoComercial>();

    // ── Módulo 3 — Precios y Ventas ──────────────────────────────
    public DbSet<ListaPrecios> ListasPrecios => Set<ListaPrecios>();
    public DbSet<ListaPreciosItem> ListaPreciosItems => Set<ListaPreciosItem>();
    public DbSet<ListaPrecioPersona> ListasPreciosPersonas => Set<ListaPrecioPersona>();
    public DbSet<ListaPreciosPromocion> ListasPreciosPromociones => Set<ListaPreciosPromocion>();
    public DbSet<PrecioEspecialCliente> PreciosEspecialesClientes => Set<PrecioEspecialCliente>();
    public DbSet<PrecioEspecialCanal> PreciosEspecialesCanales => Set<PrecioEspecialCanal>();
    public DbSet<PrecioEspecialVendedor> PreciosEspecialesVendedores => Set<PrecioEspecialVendedor>();
    public DbSet<PlanPago> PlanesPago => Set<PlanPago>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<ComprobanteProyecto> ComprobantesProyectos => Set<ComprobanteProyecto>();
    public DbSet<TareaEstimada> TareasEstimadas => Set<TareaEstimada>();
    public DbSet<TareaReal> TareasReales => Set<TareaReal>();
    public DbSet<PlanTrabajo> PlanesTrabajo => Set<PlanTrabajo>();
    public DbSet<PlanTrabajoKpi> PlanesTrabajoKpis => Set<PlanTrabajoKpi>();
    public DbSet<EvaluacionFranquicia> EvaluacionesFranquicias => Set<EvaluacionFranquicia>();
    public DbSet<EvaluacionFranquiciaDetalle> EvaluacionesFranquiciasDetalles => Set<EvaluacionFranquiciaDetalle>();
    public DbSet<FranquiciaVariableXUsuario> FranquiciasVariablesXUsuarios => Set<FranquiciaVariableXUsuario>();
    public DbSet<FranquiciaXRegion> FranquiciasXRegiones => Set<FranquiciaXRegion>();
    public DbSet<GrupoEconomico> GrupoEconomicos => Set<GrupoEconomico>();

    // ── Módulo 4 — Financiero ────────────────────────────────────
    public DbSet<CajaCuentaBancaria> CajasCuentasBancarias => Set<CajaCuentaBancaria>();
    public DbSet<TipoCajaCuenta> TiposCajaCuenta => Set<TipoCajaCuenta>();
    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<Chequera> Chequeras => Set<Chequera>();
    public DbSet<Cheque> Cheques => Set<Cheque>();
    public DbSet<CierreCaja> CierresCaja => Set<CierreCaja>();
    public DbSet<CierreCajaDetalle> CierresCajaDetalle => Set<CierreCajaDetalle>();
    public DbSet<CotizacionMoneda> CotizacionesMoneda => Set<CotizacionMoneda>();
    public DbSet<FormaPagoCaja> FormasPagoCaja => Set<FormaPagoCaja>();
    public DbSet<PlanTarjeta> PlanesTarjeta => Set<PlanTarjeta>();
    public DbSet<TarjetaTipo> TarjetasTipos => Set<TarjetaTipo>();
    public DbSet<SeguimientoOrdenPago> SeguimientosOrdenPago => Set<SeguimientoOrdenPago>();

    // ── Módulo 5 — Facturación ────────────────────────────────────
    public DbSet<TipoPuntoFacturacion> TiposPuntoFacturacion => Set<TipoPuntoFacturacion>();
    public DbSet<PuntoFacturacion> PuntosFacturacion => Set<PuntoFacturacion>();
    public DbSet<ConfiguracionFiscal> ConfiguracionesFiscales => Set<ConfiguracionFiscal>();
    public DbSet<TipoComprobantePuntoFacturacion> TiposComprobantesPuntoFacturacion => Set<TipoComprobantePuntoFacturacion>();
    public DbSet<Caea> Caeas => Set<Caea>();
    public DbSet<Timbrado> Timbrados => Set<Timbrado>();
    public DbSet<AfipWsfeConfiguracion> AfipWsfeConfiguraciones => Set<AfipWsfeConfiguracion>();
    public DbSet<AfipWsfeAudit> AfipWsfeAudits => Set<AfipWsfeAudit>();
    public DbSet<TipoEntrega> TiposEntrega => Set<TipoEntrega>();
    public DbSet<ComprobanteEntrega> ComprobantesEntregas => Set<ComprobanteEntrega>();
    public DbSet<CartaPorte> CartasPorte => Set<CartaPorte>();
    public DbSet<CartaPorteEvento> CartasPorteEventos => Set<CartaPorteEvento>();
    public DbSet<PeriodoIva> PeriodosIva => Set<PeriodoIva>();
    public DbSet<AuditoriaCaea> AuditoriaCaeas => Set<AuditoriaCaea>();
    public DbSet<HistorialSifenComprobante> HistorialSifenComprobantes => Set<HistorialSifenComprobante>();

    // ── Módulo 6 — Usuarios, Menú y Seguridad ────────────────────
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioSucursal> UsuariosSucursal => Set<UsuarioSucursal>();
    public DbSet<UsuarioXUsuario> UsuariosXUsuario => Set<UsuarioXUsuario>();
    public DbSet<ParametroUsuario> ParametrosUsuario => Set<ParametroUsuario>();
    public DbSet<MenuItem> Menu => Set<MenuItem>();
    public DbSet<MenuUsuario> MenuUsuario => Set<MenuUsuario>();
    public DbSet<Seguridad> Seguridad => Set<Seguridad>();
    public DbSet<SeguridadUsuario> SeguridadUsuario => Set<SeguridadUsuario>();

    // ── Módulo 7 — Items, Categorías y Depósitos ──────────────────
    public DbSet<CategoriaItem> CategoriasItems => Set<CategoriaItem>();
    public DbSet<Atributo> Atributos => Set<Atributo>();
    public DbSet<AtributoItem> AtributosItems => Set<AtributoItem>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemComponente> ItemsComponentes => Set<ItemComponente>();
    public DbSet<UnidadManipulacion> UnidadesManipulacion => Set<UnidadManipulacion>();
    public DbSet<Deposito> Depositos => Set<Deposito>();
    // ── M8 — Stock ────────────────────────────────────────────────
    public DbSet<StockItem> Stock => Set<StockItem>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<MovimientoStockAtributo> MovimientosStockAtributos => Set<MovimientoStockAtributo>();
    public DbSet<InventarioConteo> InventariosConteo => Set<InventarioConteo>();
    // ── M9 — Comprobantes ─────────────────────────────────────────
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<AutorizacionComprobante> AutorizacionesComprobantes => Set<AutorizacionComprobante>();
    public DbSet<HabilitacionComprobante> HabilitacionesComprobantes => Set<HabilitacionComprobante>();
    public DbSet<ComprobanteItem> ComprobantesItems => Set<ComprobanteItem>();
    public DbSet<ComprobanteCot> ComprobantesCot => Set<ComprobanteCot>();
    public DbSet<ComprobanteAtributo> ComprobantesAtributos => Set<ComprobanteAtributo>();
    public DbSet<ComprobanteImpuesto> ComprobantesImpuestos => Set<ComprobanteImpuesto>();
    public DbSet<ComprobanteTributo> ComprobantesTributos => Set<ComprobanteTributo>();
    public DbSet<ComprobanteDetalleCosto> ComprobantesDetallesCostos => Set<ComprobanteDetalleCosto>();
    public DbSet<ComprobanteFormaPago> ComprobantesFormasPago => Set<ComprobanteFormaPago>();
    public DbSet<Imputacion> Imputaciones => Set<Imputacion>();
    public DbSet<OrdenCompraMeta> OrdenesCompraMeta => Set<OrdenCompraMeta>();
    public DbSet<AuditoriaComprobante> AuditoriaComprobantes => Set<AuditoriaComprobante>();
    // ── M10 — Cobros, Pagos y Cuenta Corriente ─────────────────────
    public DbSet<ChequeHistorial> ChequesHistorial => Set<ChequeHistorial>();
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<CobroMedio> CobrosMedios => Set<CobroMedio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<PagoMedio> PagosMedios => Set<PagoMedio>();
    public DbSet<Retencion> Retenciones => Set<Retencion>();
    public DbSet<RetencionRegimen> RetencionesRegimenes => Set<RetencionRegimen>();
    public DbSet<TasaInteres> TasasInteres => Set<TasaInteres>();
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
    public DbSet<Domain.Entities.Contratos.Contrato> Contratos => Set<Domain.Entities.Contratos.Contrato>();
    public DbSet<ContratoHistorial> ContratosHistorial => Set<ContratoHistorial>();
    public DbSet<ContratoImpacto> ContratosImpactos => Set<ContratoImpacto>();
    // ── M11 — Contabilidad ─────────────────────────────────────────
    public DbSet<Ejercicio> Ejercicios => Set<Ejercicio>();
    public DbSet<EjercicioSucursal> EjercicioSucursales => Set<EjercicioSucursal>();
    public DbSet<PlanCuenta> PlanCuentas => Set<PlanCuenta>();
    public DbSet<PlanCuentaParametro> PlanCuentasParametros => Set<PlanCuentaParametro>();
    public DbSet<PeriodoContable> PeriodosContables => Set<PeriodoContable>();
    public DbSet<CentroCosto> CentrosCosto => Set<CentroCosto>();
    public DbSet<Asiento> Asientos => Set<Asiento>();
    public DbSet<AsientoLinea> AsientosLineas => Set<AsientoLinea>();
    // ── M12 — Producción ──────────────────────────────────────────
    public DbSet<FormulaProduccion> FormulasProduccion => Set<FormulaProduccion>();
    public DbSet<FormulaIngrediente> FormulaIngredientes => Set<FormulaIngrediente>();
    public DbSet<FormulaProduccionHistorial> FormulasProduccionHistorial => Set<FormulaProduccionHistorial>();
    public DbSet<OrdenTrabajo> OrdenesTrabajo => Set<OrdenTrabajo>();
    public DbSet<OrdenTrabajoConsumo> OrdenesTrabajoConsumos => Set<OrdenTrabajoConsumo>();
    public DbSet<Domain.Entities.Produccion.OrdenEmpaque> OrdenesEmpaque => Set<Domain.Entities.Produccion.OrdenEmpaque>();

    // ── M12 — RRHH ────────────────────────────────────────────────
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<EmpleadoXArea> EmpleadosXArea => Set<EmpleadoXArea>();
    public DbSet<EmpleadoXPerfil> EmpleadosXPerfil => Set<EmpleadoXPerfil>();
    public DbSet<LiquidacionSueldo> LiquidacionesSueldo => Set<LiquidacionSueldo>();
    public DbSet<ComprobanteEmpleado> ComprobantesEmpleados => Set<ComprobanteEmpleado>();
    public DbSet<ImputacionEmpleado> ImputacionesEmpleados => Set<ImputacionEmpleado>();

    // ── M12 — Extras ──────────────────────────────────────────────
    public DbSet<Transportista> Transportistas => Set<Transportista>();
    public DbSet<Busqueda> Busquedas => Set<Busqueda>();
    public DbSet<Integradora> Integradoras => Set<Integradora>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<SorteoLista> SorteosLista => Set<SorteoLista>();
    public DbSet<SorteoListaTipo> SorteosListaTipos => Set<SorteoListaTipo>();
    public DbSet<SorteoListaXCliente> SorteosListaXCliente => Set<SorteoListaXCliente>();
    public DbSet<MesaEntrada> MesasEntrada => Set<MesaEntrada>();
    public DbSet<MesaEntradaEstado> MesasEntradaEstados => Set<MesaEntradaEstado>();
    public DbSet<MesaEntradaTipo> MesasEntradaTipos => Set<MesaEntradaTipo>();

    // ── M13 — Logística / Picking ─────────────────────────────────
    public DbSet<OrdenCarga> OrdenesCarga => Set<OrdenCarga>();
    public DbSet<Domain.Entities.Logistica.OrdenEmpaque> OrdenesEmpaquesLogistica => Set<Domain.Entities.Logistica.OrdenEmpaque>();
    public DbSet<OrdenPreparacion> OrdenesPreparacion => Set<OrdenPreparacion>();
    public DbSet<OrdenPreparacionDetalle> OrdenesPreparacionDetalles => Set<OrdenPreparacionDetalle>();
    public DbSet<TransferenciaDeposito> TransferenciasDeposito => Set<TransferenciaDeposito>();
    public DbSet<TransferenciaDepositoDetalle> TransferenciasDepositoDetalles => Set<TransferenciaDepositoDetalle>();
    public DbSet<LogisticaInternaEvento> LogisticaInternaEventos => Set<LogisticaInternaEvento>();

    // ── M14 — Descuentos Comerciales ──────────────────────────────
    public DbSet<DescuentoComercial> DescuentosComerciales => Set<DescuentoComercial>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Zona> Zonas => Set<Zona>();
    public DbSet<Jurisdiccion> Jurisdicciones => Set<Jurisdiccion>();

    // ── M15 — Retenciones por Persona ──────────────────────────
    public DbSet<RetencionXPersona> RetencionesPorPersona => Set<RetencionXPersona>();

    // ── M16 — Diagnóstico / Plan de Trabajo ────────────────────
    public DbSet<RegionDiagnostica> RegionesDiagnosticas => Set<RegionDiagnostica>();
    public DbSet<Region> Regiones => Set<Region>();
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
    public DbSet<Cubo> Cubos => Set<Cubo>();
    public DbSet<CuboCampo> CubosCampos => Set<CuboCampo>();
    public DbSet<CuboFiltro> CubosFiltros => Set<CuboFiltro>();
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
    public DbSet<LiquidacionGranos> LiquidacionesGranos => Set<LiquidacionGranos>();
    public DbSet<CertificacionGranos> CertificacionesGranos => Set<CertificacionGranos>();
    public DbSet<Impuesto> Impuestos => Set<Impuesto>();
    public DbSet<ImpuestoPorItem> ImpuestosPorItem => Set<ImpuestoPorItem>();
    public DbSet<ImpuestoPorPersona> ImpuestosPorPersona => Set<ImpuestoPorPersona>();
    public DbSet<ImpuestoPorSucursal> ImpuestosPorSucursal => Set<ImpuestoPorSucursal>();
    public DbSet<ImpuestoPorTipoComprobante> ImpuestosPorTipoComprobante => Set<ImpuestoPorTipoComprobante>();
    public DbSet<Contacto> Contactos => Set<Contacto>();
    public DbSet<CrmCampana> CrmCampanas => Set<CrmCampana>();
    public DbSet<CrmComunicado> CrmComunicados => Set<CrmComunicado>();
    public DbSet<CrmInteres> CrmIntereses => Set<CrmInteres>();
    public DbSet<CrmMotivo> CrmMotivos => Set<CrmMotivo>();
    public DbSet<CrmSeguimiento> CrmSeguimientos => Set<CrmSeguimiento>();
    public DbSet<CrmTipoComunicado> CrmTiposComunicado => Set<CrmTipoComunicado>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<PresupuestoItem> PresupuestosItems => Set<PresupuestoItem>();
    public DbSet<ObjetivoVenta> ObjetivosVenta => Set<ObjetivoVenta>();
    public DbSet<ComisionVendedor> ComisionesVendedor => Set<ComisionVendedor>();

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
