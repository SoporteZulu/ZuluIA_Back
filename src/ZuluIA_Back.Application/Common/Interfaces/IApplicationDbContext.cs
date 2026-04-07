using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Agro;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Entities.CRM;
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
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.Proyectos;
using ZuluIA_Back.Domain.Entities.Referencia;

using ZuluIA_Back.Domain.Entities.RRHH;


//using ZuluIA_Back.Domain.Entities.Referencia;   // <- descomentar cuando tengas esos modelos
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Enums;

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
    DbSet<PersonaDomicilio> PersonasDomicilios { get; }
    DbSet<MedioContacto> MediosContacto { get; }
    DbSet<TipoDomicilioCatalogo> TiposDomicilio { get; }
    DbSet<TerceroPerfilComercial> TercerosPerfilesComerciales { get; }
    DbSet<TerceroContacto> TercerosContactos { get; }
    DbSet<TerceroSucursalEntrega> TercerosSucursalesEntrega { get; }
    DbSet<TerceroTransporte> TercerosTransportes { get; }
    DbSet<TerceroVentanaCobranza> TercerosVentanasCobranza { get; }
    DbSet<CategoriaCliente> CategoriasClientes { get; }
    DbSet<CategoriaProveedor> CategoriasProveedores { get; }
    DbSet<EstadoCliente> EstadosClientes { get; }
    DbSet<EstadoProveedor> EstadosProveedores { get; }
    DbSet<EstadoPersonaCatalogo> EstadosPersonas { get; }
    DbSet<EstadoCivilCatalogo> EstadosCiviles { get; }

    // ─── Catálogos de referencia ──────────────────────────────────────────────
    DbSet<Domain.Entities.Referencia.TipoDocumento> TiposDocumento { get; }
    DbSet<Domain.Entities.Referencia.CondicionIva> CondicionesIva { get; }
    DbSet<CategoriaTercero> CategoriasTerceros { get; }
    DbSet<Moneda> Monedas { get; }
    DbSet<TipoComprobante> TiposComprobante { get; }
    DbSet<MotivoDebito> MotivosDebito { get; }
    DbSet<AlicuotaIva> AlicuotasIva { get; }
    DbSet<UnidadMedida> UnidadesMedida { get; }
    DbSet<FormaPago> FormasPago { get; }
    DbSet<MarcaComercial> MarcasComerciales { get; }
    DbSet<ZonaComercial> ZonasComerciales { get; }
    DbSet<JurisdiccionComercial> JurisdiccionesComerciales { get; }
    DbSet<MaestroAuxiliarComercial> MaestrosAuxiliaresComerciales { get; }
    DbSet<AtributoComercial> AtributosComerciales { get; }
    DbSet<ComprobanteItemAtributoComercial> ComprobantesItemsAtributos { get; }
    DbSet<ItemAtributoComercial> ItemsAtributosComerciales { get; }

    // Sucursales y Configuración
    DbSet<Sucursal> Sucursales { get; }
    DbSet<Perfil> Perfiles { get; }
    DbSet<Area> Areas { get; }
    DbSet<TipoComprobanteSucursal> TiposComprobantesSucursal { get; }
    DbSet<SucursalDomicilio> SucursalesDomicilio { get; }
    DbSet<SucursalMedioContacto> SucursalesMedioContacto { get; }
    DbSet<ConfiguracionSistema> Config { get; }
    DbSet<Variable> Variables { get; }
    DbSet<Aspecto> Aspectos { get; }
    DbSet<OpcionVariable> OpcionesVariable { get; }
    DbSet<VariableDetalle> VariablesDetalle { get; }
    DbSet<PlantillaDiagnostico> PlantillasDiagnostico { get; }
    DbSet<PlantillaDiagnosticoDetalle> PlantillasDiagnosticoDetalle { get; }
    DbSet<PlanillaDiagnostico> PlanillasDiagnostico { get; }
    DbSet<PlanillaDiagnosticoDetalle> PlanillasDiagnosticoDetalle { get; }

    // ✅ Módulo 3 — Precios y Ventas
    DbSet<ListaPrecios> ListasPrecios { get; }
    DbSet<ListaPreciosItem> ListaPreciosItems { get; }
    DbSet<ListaPrecioPersona> ListasPreciosPersonas { get; }
    DbSet<ListaPreciosPromocion> ListasPreciosPromociones { get; }
    DbSet<PrecioEspecialCliente> PreciosEspecialesClientes { get; }
    DbSet<PrecioEspecialCanal> PreciosEspecialesCanales { get; }
    DbSet<PrecioEspecialVendedor> PreciosEspecialesVendedores { get; }
    DbSet<PlanPago> PlanesPago { get; }
    DbSet<Proyecto> Proyectos { get; }
    DbSet<ComprobanteProyecto> ComprobantesProyectos { get; }
    DbSet<TareaEstimada> TareasEstimadas { get; }
    DbSet<TareaReal> TareasReales { get; }
    DbSet<PlanTrabajo> PlanesTrabajo { get; }
    DbSet<PlanTrabajoKpi> PlanesTrabajoKpis { get; }
    DbSet<EvaluacionFranquicia> EvaluacionesFranquicias { get; }
    DbSet<EvaluacionFranquiciaDetalle> EvaluacionesFranquiciasDetalles { get; }
    DbSet<FranquiciaVariableXUsuario> FranquiciasVariablesXUsuarios { get; }
    DbSet<FranquiciaXRegion> FranquiciasXRegiones { get; }
    DbSet<GrupoEconomico> GrupoEconomicos { get; }

    // ✅ Módulo 4 — Financiero (Cajas, Cheques, Cotizaciones)
    DbSet<CajaCuentaBancaria> CajasCuentasBancarias { get; }
    DbSet<TipoCajaCuenta> TiposCajaCuenta { get; }
    DbSet<Banco> Bancos { get; }
    DbSet<Chequera> Chequeras { get; }
    DbSet<Cheque> Cheques { get; }
    DbSet<CierreCaja> CierresCaja { get; }
    DbSet<CierreCajaDetalle> CierresCajaDetalle { get; }
    DbSet<CotizacionMoneda> CotizacionesMoneda { get; }
    DbSet<FormaPagoCaja> FormasPagoCaja { get; }
    DbSet<PlanTarjeta> PlanesTarjeta { get; }
    DbSet<TarjetaTipo> TarjetasTipos { get; }
    DbSet<SeguimientoOrdenPago> SeguimientosOrdenPago { get; }
    // ✅ Módulo 5 — Facturación
    DbSet<TipoPuntoFacturacion> TiposPuntoFacturacion { get; }
    DbSet<PuntoFacturacion> PuntosFacturacion { get; }
    DbSet<ConfiguracionFiscal> ConfiguracionesFiscales { get; }
    DbSet<TipoComprobantePuntoFacturacion> TiposComprobantesPuntoFacturacion { get; }
    DbSet<Caea> Caeas { get; }
    DbSet<Timbrado> Timbrados { get; }
    DbSet<AfipWsfeConfiguracion> AfipWsfeConfiguraciones { get; }
    DbSet<AfipWsfeAudit> AfipWsfeAudits { get; }
    DbSet<TipoEntrega> TiposEntrega { get; }
    DbSet<ComprobanteEntrega> ComprobantesEntregas { get; }
    DbSet<CartaPorte> CartasPorte { get; }
    DbSet<CartaPorteEvento> CartasPorteEventos { get; }
    DbSet<PeriodoIva> PeriodosIva { get; }
    DbSet<AuditoriaCaea> AuditoriaCaeas { get; }
    DbSet<HistorialSifenComprobante> HistorialSifenComprobantes { get; }
    // Módulo 6 — Usuarios, Menú y Seguridad
    DbSet<Usuario> Usuarios { get; }
    DbSet<UsuarioSucursal> UsuariosSucursal { get; }
    DbSet<UsuarioXUsuario> UsuariosXUsuario { get; }
    DbSet<ParametroUsuario> ParametrosUsuario { get; }
    DbSet<MenuItem> Menu { get; }
    DbSet<MenuUsuario> MenuUsuario { get; }
    DbSet<Seguridad> Seguridad { get; }
    DbSet<SeguridadUsuario> SeguridadUsuario { get; }
    // ── Módulo 7 — Items, Categorías y Depósitos ──────────────────
    DbSet<CategoriaItem> CategoriasItems { get; }
    DbSet<Atributo> Atributos { get; }
    DbSet<AtributoItem> AtributosItems { get; }
    DbSet<Item> Items { get; }
    DbSet<ItemComponente> ItemsComponentes { get; }
    DbSet<UnidadManipulacion> UnidadesManipulacion { get; }
    DbSet<Deposito> Depositos { get; }
    // ── M8 — Stock ────────────────────────────────────────────────
    DbSet<StockItem> Stock { get; }
    DbSet<MovimientoStock> MovimientosStock { get; }
    DbSet<MovimientoStockAtributo> MovimientosStockAtributos { get; }
    DbSet<InventarioConteo> InventariosConteo { get; }
    // ── M9 — Comprobantes ─────────────────────────────────────────
    DbSet<Comprobante> Comprobantes { get; }
    DbSet<AutorizacionComprobante> AutorizacionesComprobantes { get; }
    DbSet<HabilitacionComprobante> HabilitacionesComprobantes { get; }
    DbSet<ComprobanteItem> ComprobantesItems { get; }
    DbSet<ComprobanteCot> ComprobantesCot { get; }
    DbSet<ComprobanteAtributo> ComprobantesAtributos { get; }
    DbSet<ComprobanteImpuesto> ComprobantesImpuestos { get; }
    DbSet<ComprobanteTributo> ComprobantesTributos { get; }
    DbSet<ComprobanteDetalleCosto> ComprobantesDetallesCostos { get; }
    DbSet<ComprobanteFormaPago> ComprobantesFormasPago { get; }
    DbSet<Imputacion> Imputaciones { get; }
    DbSet<OrdenCompraMeta> OrdenesCompraMeta { get; }
    DbSet<AuditoriaComprobante> AuditoriaComprobantes { get; }
    // ── M10 — Cobros, Pagos y Cuenta Corriente ─────────────────────
    DbSet<ChequeHistorial> ChequesHistorial { get; }
    DbSet<Cobro> Cobros { get; }
    DbSet<CobroMedio> CobrosMedios { get; }
    DbSet<Pago> Pagos { get; }
    DbSet<PagoMedio> PagosMedios { get; }
    DbSet<Retencion> Retenciones { get; }
    DbSet<RetencionRegimen> RetencionesRegimenes { get; }
    DbSet<TipoRetencion> TiposRetencion { get; }
    DbSet<TasaInteres> TasasInteres { get; }
    DbSet<EscalaRetencion> EscalasRetencion { get; }
    DbSet<TransferenciaCaja> TransferenciasCaja { get; }
    DbSet<TesoreriaMovimiento> TesoreriaMovimientos { get; }
    DbSet<TesoreriaCierre> TesoreriaCierres { get; }
    DbSet<CuentaCorriente> CuentaCorriente { get; }
    DbSet<MovimientoCtaCte> MovimientosCtaCte { get; }
    DbSet<Cedulon> Cedulones { get; }
    DbSet<PlanGeneralColegio> ColegioPlanesGenerales { get; }
    DbSet<LoteColegio> ColegioLotes { get; }
    DbSet<CobinproColegioOperacion> ColegioCobinproOperaciones { get; }
    DbSet<ColegioReciboDetalle> ColegioRecibosDetalles { get; }
    DbSet<Domain.Entities.Contratos.Contrato> Contratos { get; }
    DbSet<ContratoHistorial> ContratosHistorial { get; }
    DbSet<ContratoImpacto> ContratosImpactos { get; }
    // ── M11 — Contabilidad ─────────────────────────────────────────
    DbSet<Ejercicio> Ejercicios { get; }
    DbSet<EjercicioSucursal> EjercicioSucursales { get; }
    DbSet<PlanCuenta> PlanCuentas { get; }
    DbSet<PlanCuentaParametro> PlanCuentasParametros { get; }
    DbSet<PeriodoContable> PeriodosContables { get; }
    DbSet<CentroCosto> CentrosCosto { get; }
    DbSet<Asiento> Asientos { get; }
    DbSet<AsientoLinea> AsientosLineas { get; }
    // ── M12 — Producción ──────────────────────────────────────────
    DbSet<FormulaProduccion> FormulasProduccion { get; }
    DbSet<FormulaIngrediente> FormulaIngredientes { get; }
    DbSet<FormulaProduccionHistorial> FormulasProduccionHistorial { get; }
    DbSet<OrdenTrabajo> OrdenesTrabajo { get; }
    DbSet<OrdenTrabajoConsumo> OrdenesTrabajoConsumos { get; }
    DbSet<Domain.Entities.Produccion.OrdenEmpaque> OrdenesEmpaque { get; }

    // ── M12 — RRHH ────────────────────────────────────────────────
    DbSet<Empleado> Empleados { get; }
    DbSet<EmpleadoXArea> EmpleadosXArea { get; }
    DbSet<EmpleadoXPerfil> EmpleadosXPerfil { get; }
    DbSet<LiquidacionSueldo> LiquidacionesSueldo { get; }
    DbSet<ComprobanteEmpleado> ComprobantesEmpleados { get; }
    DbSet<ImputacionEmpleado> ImputacionesEmpleados { get; }

    // ── M12 — Extras ──────────────────────────────────────────────
    DbSet<Transportista> Transportistas { get; }
    DbSet<Busqueda> Busquedas { get; }
    DbSet<Integradora> Integradoras { get; }
    DbSet<Matricula> Matriculas { get; }
    DbSet<SorteoLista> SorteosLista { get; }
    DbSet<SorteoListaTipo> SorteosListaTipos { get; }
    DbSet<SorteoListaXCliente> SorteosListaXCliente { get; }
    DbSet<MesaEntrada> MesasEntrada { get; }
    DbSet<MesaEntradaEstado> MesasEntradaEstados { get; }
    DbSet<MesaEntradaTipo> MesasEntradaTipos { get; }

    // ── M13 — Logística / Picking ─────────────────────────────────
    DbSet<OrdenCarga> OrdenesCarga { get; }
    DbSet<Domain.Entities.Logistica.OrdenEmpaque> OrdenesEmpaquesLogistica { get; }
    DbSet<OrdenPreparacion> OrdenesPreparacion { get; }
    DbSet<OrdenPreparacionDetalle> OrdenesPreparacionDetalles { get; }
    DbSet<TransferenciaDeposito> TransferenciasDeposito { get; }
    DbSet<TransferenciaDepositoDetalle> TransferenciasDepositoDetalles { get; }
    DbSet<LogisticaInternaEvento> LogisticaInternaEventos { get; }

    // ── M14 — Descuentos Comerciales ──────────────────────────────
    DbSet<DescuentoComercial> DescuentosComerciales { get; }
    DbSet<Marca> Marcas { get; }
    DbSet<Zona> Zonas { get; }
    DbSet<Jurisdiccion> Jurisdicciones { get; }

    // ── M15 — Retenciones por Persona ──────────────────────────
    DbSet<RetencionXPersona> RetencionesPorPersona { get; }

    // ── M16 — Diagnóstico / Plan de Trabajo ────────────────────
    DbSet<RegionDiagnostica> RegionesDiagnosticas { get; }
    DbSet<Region> Regiones { get; }
    DbSet<AspectoDiagnostico> AspectosDiagnostico { get; }
    DbSet<VariableDiagnostica> VariablesDiagnosticas { get; }
    DbSet<VariableDiagnosticaOpcion> VariablesDiagnosticasOpciones { get; }
    DbSet<PlantillaDiagnostica> PlantillasDiagnosticas { get; }
    DbSet<PlantillaDiagnosticaVariable> PlantillasDiagnosticasVariables { get; }
    DbSet<PlanillaDiagnostica> PlanillasDiagnosticas { get; }
    DbSet<PlanillaDiagnosticaRespuesta> PlanillasDiagnosticasRespuestas { get; }

    // ── M17 — Integraciones / Importaciones / Sync ─────────────
    DbSet<ProcesoIntegracionJob> ProcesosIntegracionJobs { get; }
    DbSet<ProcesoIntegracionLog> ProcesosIntegracionLogs { get; }
    DbSet<MonitorExportacion> MonitoresExportacion { get; }
    DbSet<IntegracionExternaAudit> IntegracionesExternasAudit { get; }
    DbSet<Cubo> Cubos { get; }
    DbSet<CuboCampo> CubosCampos { get; }
    DbSet<CuboFiltro> CubosFiltros { get; }
    DbSet<BatchProgramacion> BatchProgramaciones { get; }
    DbSet<ImpresionSpoolTrabajo> ImpresionSpoolTrabajos { get; }

    // ── M19 — POS / Punto de Venta / Fiscal alternativo ──────────
    DbSet<TimbradoFiscal> TimbradosFiscales { get; }
    DbSet<OperacionPuntoVenta> OperacionesPuntoVenta { get; }
    DbSet<SifenOperacion> SifenOperaciones { get; }
    DbSet<DeuceOperacion> DeuceOperaciones { get; }

    // ── M18 — Fiscal y Contabilidad Local ──────────────────────
    DbSet<CierrePeriodoContable> CierresPeriodoContable { get; }
    DbSet<ReorganizacionAsientos> ReorganizacionesAsientos { get; }
    DbSet<LibroViajanteRegistro> LibrosViajantesRegistros { get; }
    DbSet<RentasBsAsRegistro> RentasBsAsRegistros { get; }
    DbSet<HechaukaRegistro> HechaukaRegistros { get; }
    DbSet<LiquidacionPrimariaGrano> LiquidacionesPrimariasGranos { get; }
    DbSet<SalidaRegulatoria> SalidasRegulatorias { get; }
    DbSet<LiquidacionGranos> LiquidacionesGranos { get; }
    DbSet<CertificacionGranos> CertificacionesGranos { get; }
    DbSet<Impuesto> Impuestos { get; }
    DbSet<ImpuestoPorItem> ImpuestosPorItem { get; }
    DbSet<ImpuestoPorPersona> ImpuestosPorPersona { get; }
    DbSet<ImpuestoPorSucursal> ImpuestosPorSucursal { get; }
    DbSet<ImpuestoPorTipoComprobante> ImpuestosPorTipoComprobante { get; }
    DbSet<Contacto> Contactos { get; }
    DbSet<CrmCampana> CrmCampanas { get; }
    DbSet<CrmComunicado> CrmComunicados { get; }
    DbSet<CrmInteres> CrmIntereses { get; }
    DbSet<CrmMotivo> CrmMotivos { get; }
    DbSet<CrmSeguimiento> CrmSeguimientos { get; }
    DbSet<CrmTipoComunicado> CrmTiposComunicado { get; }
    DbSet<Presupuesto> Presupuestos { get; }
    DbSet<PresupuestoItem> PresupuestosItems { get; }
    DbSet<ObjetivoVenta> ObjetivosVenta { get; }
    DbSet<ComisionVendedor> ComisionesVendedor { get; }

    // ─── Persistencia ─────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}