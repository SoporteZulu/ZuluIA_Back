using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
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
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Entities.Agro;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Franquicias;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Entities.CRM;

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
    public DbSet<Deposito> Depositos => Set<Deposito>();    public DbSet<HistorialPrecio> HistorialPrecios => Set<HistorialPrecio>();    // ── M8 — Stock ────────────────────────────────────────────────
    public DbSet<StockItem> Stock => Set<StockItem>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    // ── M9 — Comprobantes ─────────────────────────────────────────
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<ComprobanteItem> ComprobantesItems => Set<ComprobanteItem>();
    public DbSet<ComprobanteImpuesto> ComprobantesImpuestos => Set<ComprobanteImpuesto>();
    public DbSet<ComprobanteTributo> ComprobantesTributos => Set<ComprobanteTributo>();
    public DbSet<ComprobanteRelacion> ComprobantesRelaciones => Set<ComprobanteRelacion>();
    public DbSet<Imputacion> Imputaciones => Set<Imputacion>();
    public DbSet<OrdenCompraMeta> OrdenesCompraMeta => Set<OrdenCompraMeta>();
    // ── M10 — Cobros, Pagos y Cuenta Corriente ─────────────────────
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<CobroMedio> CobrosMedios => Set<CobroMedio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<PagoMedio> PagosMedios => Set<PagoMedio>();
    public DbSet<Retencion> Retenciones => Set<Retencion>();
    public DbSet<TipoRetencion> TiposRetencion => Set<TipoRetencion>();
    public DbSet<EscalaRetencion> EscalasRetencion => Set<EscalaRetencion>();
    public DbSet<TransferenciaCaja> TransferenciasCaja => Set<TransferenciaCaja>();
    public DbSet<CuentaCorriente> CuentaCorriente => Set<CuentaCorriente>();
    public DbSet<MovimientoCtaCte> MovimientosCtaCte => Set<MovimientoCtaCte>();
    public DbSet<Cedulon> Cedulones => Set<Cedulon>();
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
    public DbSet<OrdenTrabajo> OrdenesTrabajo => Set<OrdenTrabajo>();

    // ── M12 — RRHH ────────────────────────────────────────────────
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<LiquidacionSueldo> LiquidacionesSueldo => Set<LiquidacionSueldo>();

    // ── M12 — Extras ──────────────────────────────────────────────
    public DbSet<Transportista> Transportistas => Set<Transportista>();
    public DbSet<Busqueda> Busquedas => Set<Busqueda>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Integradora> Integradoras => Set<Integradora>();
    public DbSet<SorteoLista> SorteosLista => Set<SorteoLista>();
    public DbSet<SorteoListaTipo> SorteosListaTipos => Set<SorteoListaTipo>();
    public DbSet<SorteoListaXCliente> SorteosListaXCliente => Set<SorteoListaXCliente>();
    public DbSet<MesaEntrada> MesasEntrada => Set<MesaEntrada>();
    public DbSet<MesaEntradaTipo> MesasEntradaTipos => Set<MesaEntradaTipo>();
    public DbSet<MesaEntradaEstado> MesasEntradaEstados => Set<MesaEntradaEstado>();

    // ── M13 — Logística / Picking ─────────────────────────────────
    public DbSet<OrdenPreparacion> OrdenesPreparacion => Set<OrdenPreparacion>();
    public DbSet<OrdenPreparacionDetalle> OrdenesPreparacionDetalles => Set<OrdenPreparacionDetalle>();

    // ── M14 — Descuentos Comerciales ──────────────────────────────
    public DbSet<DescuentoComercial> DescuentosComerciales => Set<DescuentoComercial>();

    // ── M15 — Retenciones por Persona ──────────────────────────
    public DbSet<RetencionXPersona> RetencionesPorPersona => Set<RetencionXPersona>();
    // ── M16 — Presupuestos ─────────────────────────────────────────────────
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<PresupuestoItem> PresupuestosItems => Set<PresupuestoItem>();    // ── M17 — Contratos de Servicio ──────────────────────────────────
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ContratoDetalle> ContratosDetalles => Set<ContratoDetalle>();
    // ── M18 — Órdenes de Empaque ──────────────────────────────────
    public DbSet<OrdenEmpaque> OrdenesEmpaque => Set<OrdenEmpaque>();
    public DbSet<OrdenEmpaqueDetalle> OrdenesEmpaqueDetalles => Set<OrdenEmpaqueDetalle>();
    // ── M19 — Catálogos extendidos ──────────────────────────────────
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Zona> Zonas => Set<Zona>();
    public DbSet<Jurisdiccion> Jurisdicciones => Set<Jurisdiccion>();
    // ── M20 — Período Contable ────────────────────────────────────
    public DbSet<PeriodoContable> PeriodosContables => Set<PeriodoContable>();
    // ── M21 — Atributos de Ítems ──────────────────────────────────
    public DbSet<Atributo> Atributos => Set<Atributo>();
    public DbSet<AtributoItem> AtributosItems => Set<AtributoItem>();

    // ── M22 — Timbrado (Paraguay) ────────────────────────────────
    public DbSet<Timbrado> Timbrados => Set<Timbrado>();

    // ── M22 — Impuestos / Percepciones ───────────────────────────
    public DbSet<Impuesto> Impuestos => Set<Impuesto>();
    public DbSet<ImpuestoPorPersona> ImpuestosPorPersona => Set<ImpuestoPorPersona>();
    public DbSet<ImpuestoPorItem> ImpuestosPorItem => Set<ImpuestoPorItem>();

    // ── M23 — Impuesto×Sucursal, BOM, Proyectos, Entrega, Trazabilidad ──
    public DbSet<ImpuestoPorSucursal> ImpuestosPorSucursal => Set<ImpuestoPorSucursal>();
    public DbSet<ItemComponente> ItemsComponentes => Set<ItemComponente>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<ComprobanteProyecto> ComprobantesProyectos => Set<ComprobanteProyecto>();
    public DbSet<TareaEstimada> TareasEstimadas => Set<TareaEstimada>();
    public DbSet<TareaReal> TareasReales => Set<TareaReal>();
    public DbSet<MovimientoStockAtributo> MovimientosStockAtributos => Set<MovimientoStockAtributo>();
    public DbSet<TipoEntrega> TiposEntrega => Set<TipoEntrega>();
    public DbSet<ComprobanteEntrega> ComprobantesEntregas => Set<ComprobanteEntrega>();
    public DbSet<ComprobanteDetalleCosto> ComprobantesDetallesCostos => Set<ComprobanteDetalleCosto>();

    // ── M24 — Bancos, Regiones, Variables, Cierres, Config Comp., Retenciones ──
    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<Chequera> Chequeras => Set<Chequera>();
    public DbSet<TarjetaTipo> TarjetasTipos => Set<TarjetaTipo>();
    public DbSet<PlanTarjeta> PlanesTarjeta => Set<PlanTarjeta>();
    public DbSet<SeguimientoOrdenPago> SeguimientosOrdenPago => Set<SeguimientoOrdenPago>();
    public DbSet<Region> Regiones => Set<Region>();
    public DbSet<Aspecto> Aspectos => Set<Aspecto>();
    public DbSet<Variable> Variables => Set<Variable>();
    public DbSet<ComprobanteFormaPago> ComprobantesFormasPago => Set<ComprobanteFormaPago>();
    public DbSet<CierreCaja> CierresCaja => Set<CierreCaja>();
    public DbSet<TipoComprobanteSucursal> TiposComprobantesSucursal => Set<TipoComprobanteSucursal>();
    public DbSet<RetencionRegimen> RetencionesRegimenes => Set<RetencionRegimen>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<InventarioConteo> InventariosConteo => Set<InventarioConteo>();
    public DbSet<AutorizacionComprobante> AutorizacionesComprobantes => Set<AutorizacionComprobante>();
    public DbSet<HabilitacionComprobante> HabilitacionesComprobantes => Set<HabilitacionComprobante>();
    // ── M25 — OpcionVariable, Perfil, EmpleadoXArea, EmpleadoXPerfil, CierreCajaDetalle ──
    public DbSet<OpcionVariable> OpcionesVariable => Set<OpcionVariable>();
    public DbSet<Perfil> Perfiles => Set<Perfil>();
    public DbSet<EmpleadoXArea> EmpleadosXArea => Set<EmpleadoXArea>();
    public DbSet<EmpleadoXPerfil> EmpleadosXPerfil => Set<EmpleadoXPerfil>();
    public DbSet<CierreCajaDetalle> CierresCajaDetalle => Set<CierreCajaDetalle>();

    // ── M26 — Tercero sub-entidades ──────────────────────────────────────────
    public DbSet<CategoriaCliente> CategoriasClientes => Set<CategoriaCliente>();
    public DbSet<CategoriaProveedor> CategoriasProveedores => Set<CategoriaProveedor>();
    public DbSet<EstadoCliente> EstadosClientes => Set<EstadoCliente>();
    public DbSet<EstadoProveedor> EstadosProveedores => Set<EstadoProveedor>();
    public DbSet<PersonaDomicilio> Domicilios => Set<PersonaDomicilio>();
    public DbSet<MedioContacto> MediosContacto => Set<MedioContacto>();
    public DbSet<PersonaXTipoPersona> PersonasXTipoPersona => Set<PersonaXTipoPersona>();
    public DbSet<VinculacionPersona> VinculacionesPersona => Set<VinculacionPersona>();
    public DbSet<Contacto> Contactos => Set<Contacto>();
    public DbSet<CrmCampana> CrmCampanas => Set<CrmCampana>();
    public DbSet<CrmComunicado> CrmComunicados => Set<CrmComunicado>();
    public DbSet<CrmInteres> CrmIntereses => Set<CrmInteres>();
    public DbSet<CrmMotivo> CrmMotivos => Set<CrmMotivo>();
    public DbSet<CrmSeguimiento> CrmSeguimientos => Set<CrmSeguimiento>();
    public DbSet<CrmTipoComunicado> CrmTiposComunicado => Set<CrmTipoComunicado>();

    // ── M26 — Sucursal sub-entidades ─────────────────────────────────────────
    public DbSet<SucursalDomicilio> SucursalesDomicilio => Set<SucursalDomicilio>();
    public DbSet<SucursalMedioContacto> SucursalesMedioContacto => Set<SucursalMedioContacto>();

    // ── M26 — Configuracion: VariableDetalle, Planillas, Plantillas ──────────
    public DbSet<VariableDetalle> VariablesDetalle => Set<VariableDetalle>();
    public DbSet<PlanillaDiagnostico> PlanillasDiagnostico => Set<PlanillaDiagnostico>();
    public DbSet<PlanillaDiagnosticoDetalle> PlanillasDiagnosticoDetalle => Set<PlanillaDiagnosticoDetalle>();
    public DbSet<PlantillaDiagnostico> PlantillasDiagnostico => Set<PlantillaDiagnostico>();
    public DbSet<PlantillaDiagnosticoDetalle> PlantillasDiagnosticoDetalle => Set<PlantillaDiagnosticoDetalle>();

    // ── M26 — Seguridad: UsuarioXUsuario ─────────────────────────────────────
    public DbSet<UsuarioXUsuario> UsuariosXUsuario => Set<UsuarioXUsuario>();

    // ── M27 — BI: Cubos de análisis ─────────────────────────────────────────────
    public DbSet<Cubo>       Cubos       => Set<Cubo>();
    public DbSet<CuboCampo>  CubosCampos => Set<CuboCampo>();
    public DbSet<CuboFiltro> CubosFiltros => Set<CuboFiltro>();

    // ── M28 — Listas de precios × personas ───────────────────────────────────
    public DbSet<ListaPrecioPersona> ListasPreciosPersonas => Set<ListaPrecioPersona>();

    // ── M28 — Unidades de manipulación de ítems ──────────────────────────────
    public DbSet<UnidadManipulacion> UnidadesManipulacion => Set<UnidadManipulacion>();

    // ── M28 — Configuración de impresoras fiscales ────────────────────────────
    public DbSet<ConfiguracionFiscal> ConfiguracionesFiscales => Set<ConfiguracionFiscal>();

    // ── M29 — Impuesto × tipo de comprobante ─────────────────────────────────
    public DbSet<ImpuestoPorTipoComprobante> ImpuestosPorTipoComprobante => Set<ImpuestoPorTipoComprobante>();

    // ── M29 — Tipos de comprobante × punto de facturación ────────────────────
    public DbSet<TipoComprobantePuntoFacturacion> TiposComprobantesPuntoFacturacion => Set<TipoComprobantePuntoFacturacion>();

    // ── Nuevas Features (12) ───────────────────────────────────────────────────
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboItem> RecibosItems => Set<ReciboItem>();
    public DbSet<RequisicionCompra> RequisicionesCompra => Set<RequisicionCompra>();
    public DbSet<RequisicionCompraItem> RequisicionesCompraItems => Set<RequisicionCompraItem>();
    public DbSet<CotizacionCompra> CotizacionesCompra => Set<CotizacionCompra>();
    public DbSet<CotizacionCompraItem> CotizacionesCompraItems => Set<CotizacionCompraItem>();
    public DbSet<NotaPedido> NotasPedido => Set<NotaPedido>();
    public DbSet<NotaPedidoItem> NotasPedidoItems => Set<NotaPedidoItem>();
    public DbSet<Caea> Caeas => Set<Caea>();
    public DbSet<ComisionVendedor> ComisionesVendedor => Set<ComisionVendedor>();
    public DbSet<TasaInteres> TasasInteres => Set<TasaInteres>();
    public DbSet<PlanTrabajo> PlanesTrabajo => Set<PlanTrabajo>();
    public DbSet<PlanTrabajoKpi> PlanesTrabajosKpis => Set<PlanTrabajoKpi>();
    public DbSet<GrupoEconomico> GrupoEconomicos => Set<GrupoEconomico>();
    public DbSet<FranquiciaXRegion> FranquiciasXRegiones => Set<FranquiciaXRegion>();
    public DbSet<FranquiciaVariableXUsuario> FranquiciasVariablesXUsuarios => Set<FranquiciaVariableXUsuario>();
    public DbSet<EvaluacionFranquicia> EvaluacionesFranquicias => Set<EvaluacionFranquicia>();
    public DbSet<EvaluacionFranquiciaDetalle> EvaluacionesFranquiciasDetalles => Set<EvaluacionFranquiciaDetalle>();
    public DbSet<AuditoriaComprobante> AuditoriaComprobantes => Set<AuditoriaComprobante>();
    public DbSet<AuditoriaCaea> AuditoriaCaeas => Set<AuditoriaCaea>();
    public DbSet<HistorialSifenComprobante> HistorialSifenComprobantes => Set<HistorialSifenComprobante>();
    public DbSet<LiquidacionGranos> LiquidacionesGranos => Set<LiquidacionGranos>();
    public DbSet<LiquidacionGranosConcepto> LiquidacionesGranosConceptos => Set<LiquidacionGranosConcepto>();
    public DbSet<CertificacionGranos> CertificacionesGranos => Set<CertificacionGranos>();
    public DbSet<ObjetivoVenta> ObjetivosVenta => Set<ObjetivoVenta>();

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
