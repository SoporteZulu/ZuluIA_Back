using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
    DbSet<TerceroPerfilComercial> TercerosPerfilesComerciales { get; }
    DbSet<TerceroContacto> TercerosContactos { get; }
    DbSet<TerceroSucursalEntrega> TercerosSucursalesEntrega { get; }
    DbSet<TerceroTransporte> TercerosTransportes { get; }
    DbSet<TerceroVentanaCobranza> TercerosVentanasCobranza { get; }

    // ─── Catálogos de referencia ──────────────────────────────────────────────
    DbSet<Domain.Entities.Referencia.TipoDocumento> TiposDocumento { get; }
    DbSet<Domain.Entities.Referencia.CondicionIva> CondicionesIva { get; }
    DbSet<CategoriaTercero> CategoriasTerceros { get; }
    DbSet<Moneda> Monedas { get; }
    DbSet<TipoComprobante> TiposComprobante { get; }
    DbSet<AlicuotaIva> AlicuotasIva { get; }
    DbSet<UnidadMedida> UnidadesMedida { get; }
    DbSet<FormaPago> FormasPago { get; }
    DbSet<MarcaComercial> MarcasComerciales { get; }
    DbSet<ZonaComercial> ZonasComerciales { get; }
    DbSet<JurisdiccionComercial> JurisdiccionesComerciales { get; }
    DbSet<MaestroAuxiliarComercial> MaestrosAuxiliaresComerciales { get; }
    DbSet<AtributoComercial> AtributosComerciales { get; }
    DbSet<ComprobanteItemAtributoComercial> ComprobantesItemsAtributos { get; }

    // Sucursales y Configuración
    DbSet<Sucursal> Sucursales { get; }
    DbSet<ConfiguracionSistema> Config { get; }

    // ✅ Módulo 3 — Precios y Ventas
    DbSet<ListaPrecios> ListasPrecios { get; }
    DbSet<ListaPreciosItem> ListaPreciosItems { get; }
    DbSet<PlanPago> PlanesPago { get; }

    // ✅ Módulo 4 — Financiero (Cajas, Cheques, Cotizaciones)
    DbSet<CajaCuentaBancaria> CajasCuentasBancarias { get; }
    DbSet<TipoCajaCuenta> TiposCajaCuenta { get; }
    DbSet<Cheque> Cheques { get; }
    DbSet<CotizacionMoneda> CotizacionesMoneda { get; }
    DbSet<FormaPagoCaja> FormasPagoCaja { get; }
    // ✅ Módulo 5 — Facturación
    DbSet<TipoPuntoFacturacion> TiposPuntoFacturacion { get; }
    DbSet<PuntoFacturacion> PuntosFacturacion { get; }
    DbSet<AfipWsfeConfiguracion> AfipWsfeConfiguraciones { get; }
    DbSet<AfipWsfeAudit> AfipWsfeAudits { get; }
    DbSet<CartaPorte> CartasPorte { get; }
    DbSet<CartaPorteEvento> CartasPorteEventos { get; }
    DbSet<PeriodoIva> PeriodosIva { get; }
    // Módulo 6 — Usuarios, Menú y Seguridad
    DbSet<Usuario> Usuarios { get; }
    DbSet<UsuarioSucursal> UsuariosSucursal { get; }
    DbSet<MenuItem> Menu { get; }
    DbSet<MenuUsuario> MenuUsuario { get; }
    DbSet<Seguridad> Seguridad { get; }
    DbSet<SeguridadUsuario> SeguridadUsuario { get; }
    DbSet<ParametroUsuario> ParametrosUsuario { get; }
    // ── Módulo 7 — Items, Categorías y Depósitos ──────────────────
    DbSet<CategoriaItem> CategoriasItems { get; }
    DbSet<Item> Items { get; }
    DbSet<Deposito> Depositos { get; }
    // ── M8 — Stock ────────────────────────────────────────────────
    DbSet<StockItem> Stock { get; }
    DbSet<MovimientoStock> MovimientosStock { get; }
    // ── M9 — Comprobantes ─────────────────────────────────────────
    DbSet<Comprobante> Comprobantes { get; }
    DbSet<ComprobanteItem> ComprobantesItems { get; }
    DbSet<Imputacion> Imputaciones { get; }
    DbSet<OrdenCompraMeta> OrdenesCompraMeta { get; }
    // ── M10 — Cobros, Pagos y Cuenta Corriente ─────────────────────
    DbSet<ChequeHistorial> ChequesHistorial { get; }
    DbSet<Cobro> Cobros { get; }
    DbSet<CobroMedio> CobrosMedios { get; }
    DbSet<Pago> Pagos { get; }
    DbSet<PagoMedio> PagosMedios { get; }
    DbSet<Retencion> Retenciones { get; }
    DbSet<TipoRetencion> TiposRetencion { get; }
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
    DbSet<Contrato> Contratos { get; }
    DbSet<ContratoHistorial> ContratosHistorial { get; }
    DbSet<ContratoImpacto> ContratosImpactos { get; }
    // ── M11 — Contabilidad ─────────────────────────────────────────
    DbSet<Ejercicio> Ejercicios { get; }
    DbSet<EjercicioSucursal> EjercicioSucursales { get; }
    DbSet<PlanCuenta> PlanCuentas { get; }
    DbSet<PlanCuentaParametro> PlanCuentasParametros { get; }
    DbSet<CentroCosto> CentrosCosto { get; }
    DbSet<Asiento> Asientos { get; }
    DbSet<AsientoLinea> AsientosLineas { get; }
    // ── M12 — Producción ──────────────────────────────────────────
    DbSet<FormulaProduccion> FormulasProduccion { get; }
    DbSet<FormulaIngrediente> FormulaIngredientes { get; }
    DbSet<FormulaProduccionHistorial> FormulasProduccionHistorial { get; }
    DbSet<OrdenTrabajo> OrdenesTrabajo { get; }
    DbSet<OrdenTrabajoConsumo> OrdenesTrabajoConsumos { get; }
    DbSet<OrdenEmpaque> OrdenesEmpaque { get; }

    // ── M12 — RRHH ────────────────────────────────────────────────
    DbSet<Empleado> Empleados { get; }
    DbSet<LiquidacionSueldo> LiquidacionesSueldo { get; }
    DbSet<ComprobanteEmpleado> ComprobantesEmpleados { get; }
    DbSet<ImputacionEmpleado> ImputacionesEmpleados { get; }

    // ── M12 — Extras ──────────────────────────────────────────────
    DbSet<Transportista> Transportistas { get; }
    DbSet<Busqueda> Busquedas { get; }

    // ── M13 — Logística / Picking ─────────────────────────────────
    DbSet<OrdenCarga> OrdenesCarga { get; }
    DbSet<OrdenPreparacion> OrdenesPreparacion { get; }
    DbSet<OrdenPreparacionDetalle> OrdenesPreparacionDetalles { get; }
    DbSet<TransferenciaDeposito> TransferenciasDeposito { get; }
    DbSet<TransferenciaDepositoDetalle> TransferenciasDepositoDetalles { get; }
    DbSet<LogisticaInternaEvento> LogisticaInternaEventos { get; }

    // ── M14 — Descuentos Comerciales ──────────────────────────────
    DbSet<DescuentoComercial> DescuentosComerciales { get; }

    // ── M15 — Retenciones por Persona ──────────────────────────
    DbSet<RetencionXPersona> RetencionesPorPersona { get; }

    // ── M16 — Diagnóstico / Plan de Trabajo ────────────────────
    DbSet<RegionDiagnostica> RegionesDiagnosticas { get; }
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

    // ─── Persistencia ─────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}