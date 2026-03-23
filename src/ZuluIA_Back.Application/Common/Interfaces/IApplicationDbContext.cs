using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Proyectos;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Entities.Agro;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Franquicias;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Entities.CRM;

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

    // ─── Catálogos de referencia ──────────────────────────────────────────────
    DbSet<Domain.Entities.Referencia.TipoDocumento> TiposDocumento { get; }
    DbSet<Domain.Entities.Referencia.CondicionIva> CondicionesIva { get; }
    DbSet<CategoriaTercero> CategoriasTerceros { get; }
    DbSet<Moneda> Monedas { get; }
    DbSet<TipoComprobante> TiposComprobante { get; }
    DbSet<AlicuotaIva> AlicuotasIva { get; }
    DbSet<UnidadMedida> UnidadesMedida { get; }
    DbSet<FormaPago> FormasPago { get; }

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
    DbSet<CartaPorte> CartasPorte { get; }
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
    DbSet<HistorialPrecio> HistorialPrecios { get; }
    // ── M8 — Stock ────────────────────────────────────────────────
    DbSet<StockItem> Stock { get; }
    DbSet<MovimientoStock> MovimientosStock { get; }
    // ── M9 — Comprobantes ─────────────────────────────────────────
    DbSet<Comprobante> Comprobantes { get; }
    DbSet<ComprobanteItem> ComprobantesItems { get; }
    DbSet<ComprobanteImpuesto> ComprobantesImpuestos { get; }
    DbSet<ComprobanteTributo> ComprobantesTributos { get; }
    DbSet<ComprobanteRelacion> ComprobantesRelaciones { get; }
    DbSet<Imputacion> Imputaciones { get; }
    DbSet<OrdenCompraMeta> OrdenesCompraMeta { get; }
    // ── M10 — Cobros, Pagos y Cuenta Corriente ─────────────────────
    DbSet<Cobro> Cobros { get; }
    DbSet<CobroMedio> CobrosMedios { get; }
    DbSet<Pago> Pagos { get; }
    DbSet<PagoMedio> PagosMedios { get; }
    DbSet<Retencion> Retenciones { get; }
    DbSet<TipoRetencion> TiposRetencion { get; }
    DbSet<EscalaRetencion> EscalasRetencion { get; }
    DbSet<TransferenciaCaja> TransferenciasCaja { get; }
    DbSet<CuentaCorriente> CuentaCorriente { get; }
    DbSet<MovimientoCtaCte> MovimientosCtaCte { get; }
    DbSet<Cedulon> Cedulones { get; }
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
    DbSet<OrdenTrabajo> OrdenesTrabajo { get; }

    // ── M12 — RRHH ────────────────────────────────────────────────
    DbSet<Empleado> Empleados { get; }
    DbSet<LiquidacionSueldo> LiquidacionesSueldo { get; }

    // ── M12 — Extras ──────────────────────────────────────────────
    DbSet<Transportista> Transportistas { get; }
    DbSet<Busqueda> Busquedas { get; }
    DbSet<Matricula> Matriculas { get; }
    DbSet<Integradora> Integradoras { get; }
    DbSet<SorteoLista> SorteosLista { get; }
    DbSet<SorteoListaTipo> SorteosListaTipos { get; }
    DbSet<SorteoListaXCliente> SorteosListaXCliente { get; }
    DbSet<MesaEntrada> MesasEntrada { get; }
    DbSet<MesaEntradaTipo> MesasEntradaTipos { get; }
    DbSet<MesaEntradaEstado> MesasEntradaEstados { get; }

    // ── M13 — Logística / Picking ─────────────────────────────────
    DbSet<OrdenPreparacion> OrdenesPreparacion { get; }
    DbSet<OrdenPreparacionDetalle> OrdenesPreparacionDetalles { get; }

    // ── M14 — Descuentos Comerciales ──────────────────────────────
    DbSet<DescuentoComercial> DescuentosComerciales { get; }

    // ── M16 — Presupuestos ────────────────────────────────────────
    DbSet<Presupuesto> Presupuestos { get; }
    DbSet<PresupuestoItem> PresupuestosItems { get; }

    // ── M15 — Retenciones por Persona ──────────────────────────
    DbSet<RetencionXPersona> RetencionesPorPersona { get; }

    // ── M17 — Contratos de Servicio ──────────────────────────────
    DbSet<Contrato> Contratos { get; }
    DbSet<ContratoDetalle> ContratosDetalles { get; }

    // ── M18 — Órdenes de Empaque ──────────────────────────────────
    DbSet<OrdenEmpaque> OrdenesEmpaque { get; }
    DbSet<OrdenEmpaqueDetalle> OrdenesEmpaqueDetalles { get; }

    // ── M19 — Catálogos extendidos (Marcas, Zonas, Jurisdicciones) ──
    DbSet<Marca> Marcas { get; }
    DbSet<Zona> Zonas { get; }
    DbSet<Jurisdiccion> Jurisdicciones { get; }
    // ── M20 — Período Contable ───────────────────────────────────────────────
    DbSet<PeriodoContable> PeriodosContables { get; }

    // ── M21 — Atributos de Ítems ───────────────────────────────────────────
    DbSet<Atributo> Atributos { get; }
    DbSet<AtributoItem> AtributosItems { get; }

    // ── M22 — Timbrado (Paraguay) ────────────────────────────────────────────
    DbSet<Timbrado> Timbrados { get; }

    // ── M22 — Impuestos / Percepciones ───────────────────────────────────────
    DbSet<Impuesto> Impuestos { get; }
    DbSet<ImpuestoPorPersona> ImpuestosPorPersona { get; }
    DbSet<ImpuestoPorItem> ImpuestosPorItem { get; }

    // ── M23 — Impuesto×Sucursal, BOM, Proyectos, Entrega, Trazabilidad ──
    DbSet<ImpuestoPorSucursal> ImpuestosPorSucursal { get; }
    DbSet<ItemComponente> ItemsComponentes { get; }
    DbSet<Proyecto> Proyectos { get; }
    DbSet<ComprobanteProyecto> ComprobantesProyectos { get; }
    DbSet<TareaEstimada> TareasEstimadas { get; }
    DbSet<TareaReal> TareasReales { get; }
    DbSet<MovimientoStockAtributo> MovimientosStockAtributos { get; }
    DbSet<TipoEntrega> TiposEntrega { get; }
    DbSet<ComprobanteEntrega> ComprobantesEntregas { get; }
    DbSet<ComprobanteDetalleCosto> ComprobantesDetallesCostos { get; }

    // ── M24 — Bancos, Regiones, Variables, Cierres, Config Comp., Retenciones ──
    DbSet<Banco> Bancos { get; }
    DbSet<Chequera> Chequeras { get; }
    DbSet<TarjetaTipo> TarjetasTipos { get; }
    DbSet<PlanTarjeta> PlanesTarjeta { get; }
    DbSet<SeguimientoOrdenPago> SeguimientosOrdenPago { get; }
    DbSet<Region> Regiones { get; }
    DbSet<Aspecto> Aspectos { get; }
    DbSet<Variable> Variables { get; }
    DbSet<ComprobanteFormaPago> ComprobantesFormasPago { get; }
    DbSet<CierreCaja> CierresCaja { get; }
    DbSet<TipoComprobanteSucursal> TiposComprobantesSucursal { get; }
    DbSet<RetencionRegimen> RetencionesRegimenes { get; }
    DbSet<Area> Areas { get; }
    DbSet<InventarioConteo> InventariosConteo { get; }
    DbSet<AutorizacionComprobante> AutorizacionesComprobantes { get; }
    DbSet<HabilitacionComprobante> HabilitacionesComprobantes { get; }
    // ── M25 — OpcionVariable, Perfil, EmpleadoXArea, EmpleadoXPerfil, CierreCajaDetalle ──
    DbSet<OpcionVariable> OpcionesVariable { get; }
    DbSet<Perfil> Perfiles { get; }
    DbSet<EmpleadoXArea> EmpleadosXArea { get; }
    DbSet<EmpleadoXPerfil> EmpleadosXPerfil { get; }
    DbSet<CierreCajaDetalle> CierresCajaDetalle { get; }

    // ── M26 — Tercero sub-entidades (domicilios, medios de contacto, relaciones) ──
    DbSet<CategoriaCliente> CategoriasClientes { get; }
    DbSet<CategoriaProveedor> CategoriasProveedores { get; }
    DbSet<EstadoCliente> EstadosClientes { get; }
    DbSet<EstadoProveedor> EstadosProveedores { get; }
    DbSet<PersonaDomicilio> Domicilios { get; }
    DbSet<MedioContacto> MediosContacto { get; }
    DbSet<PersonaXTipoPersona> PersonasXTipoPersona { get; }
    DbSet<VinculacionPersona> VinculacionesPersona { get; }
    DbSet<Contacto> Contactos { get; }
    DbSet<CrmCampana> CrmCampanas { get; }
    DbSet<CrmComunicado> CrmComunicados { get; }
    DbSet<CrmInteres> CrmIntereses { get; }
    DbSet<CrmMotivo> CrmMotivos { get; }
    DbSet<CrmSeguimiento> CrmSeguimientos { get; }
    DbSet<CrmTipoComunicado> CrmTiposComunicado { get; }

    // ── M26 — Sucursal sub-entidades ──────────────────────────────────────────
    DbSet<SucursalDomicilio> SucursalesDomicilio { get; }
    DbSet<SucursalMedioContacto> SucursalesMedioContacto { get; }

    // ── M26 — Configuracion: VariableDetalle, Planillas, Plantillas ──────────
    DbSet<VariableDetalle> VariablesDetalle { get; }
    DbSet<PlanillaDiagnostico> PlanillasDiagnostico { get; }
    DbSet<PlanillaDiagnosticoDetalle> PlanillasDiagnosticoDetalle { get; }
    DbSet<PlantillaDiagnostico> PlantillasDiagnostico { get; }
    DbSet<PlantillaDiagnosticoDetalle> PlantillasDiagnosticoDetalle { get; }

    // ── M26 — Seguridad: UsuarioXUsuario ─────────────────────────────────────
    DbSet<UsuarioXUsuario> UsuariosXUsuario { get; }

    // ── M27 — BI: Cubos de análisis ──────────────────────────────────────────
    DbSet<Cubo> Cubos { get; }
    DbSet<CuboCampo> CubosCampos { get; }
    DbSet<CuboFiltro> CubosFiltros { get; }

    // ── M28 — Listas de precios × personas ───────────────────────────────────
    DbSet<ListaPrecioPersona> ListasPreciosPersonas { get; }

    // ── M28 — Unidades de manipulación de ítems ──────────────────────────────
    DbSet<UnidadManipulacion> UnidadesManipulacion { get; }

    // ── M28 — Configuración de impresoras fiscales ────────────────────────────
    DbSet<ConfiguracionFiscal> ConfiguracionesFiscales { get; }

    // ── M29 — Impuesto × tipo de comprobante ─────────────────────────────────
    DbSet<ImpuestoPorTipoComprobante> ImpuestosPorTipoComprobante { get; }

    // ── M29 — Tipos de comprobante × punto de facturación ────────────────────
    DbSet<TipoComprobantePuntoFacturacion> TiposComprobantesPuntoFacturacion { get; }

    // ── Nuevas Features ──────────────────────────────────────────────────────────
    // F1 — Recibo
    DbSet<Recibo> Recibos { get; }
    DbSet<ReciboItem> RecibosItems { get; }
    // F2 — Compras
    DbSet<RequisicionCompra> RequisicionesCompra { get; }
    DbSet<RequisicionCompraItem> RequisicionesCompraItems { get; }
    DbSet<CotizacionCompra> CotizacionesCompra { get; }
    DbSet<CotizacionCompraItem> CotizacionesCompraItems { get; }
    // F3 — Nota de Pedido
    DbSet<NotaPedido> NotasPedido { get; }
    DbSet<NotaPedidoItem> NotasPedidoItems { get; }
    // F4 — CAEA
    DbSet<Caea> Caeas { get; }
    // F6 — Comisiones
    DbSet<ComisionVendedor> ComisionesVendedor { get; }
    // F7 — Tasas de Interés
    DbSet<TasaInteres> TasasInteres { get; }
    // F8 — Plan de Trabajo
    DbSet<PlanTrabajo> PlanesTrabajo { get; }
    DbSet<PlanTrabajoKpi> PlanesTrabajosKpis { get; }
    DbSet<GrupoEconomico> GrupoEconomicos { get; }
    DbSet<FranquiciaXRegion> FranquiciasXRegiones { get; }
    DbSet<FranquiciaVariableXUsuario> FranquiciasVariablesXUsuarios { get; }
    DbSet<EvaluacionFranquicia> EvaluacionesFranquicias { get; }
    DbSet<EvaluacionFranquiciaDetalle> EvaluacionesFranquiciasDetalles { get; }
    // F10 — Auditoría
    DbSet<AuditoriaComprobante> AuditoriaComprobantes { get; }
    DbSet<AuditoriaCaea> AuditoriaCaeas { get; }
    DbSet<HistorialSifenComprobante> HistorialSifenComprobantes { get; }
    // F11 — Granos
    DbSet<LiquidacionGranos> LiquidacionesGranos { get; }
    DbSet<LiquidacionGranosConcepto> LiquidacionesGranosConceptos { get; }
    DbSet<CertificacionGranos> CertificacionesGranos { get; }
    // F12 — Objetivos de Venta
    DbSet<ObjetivoVenta> ObjetivosVenta { get; }

    // ─── Persistencia ─────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}