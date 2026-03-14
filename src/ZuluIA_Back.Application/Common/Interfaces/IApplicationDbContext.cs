using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Items;
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
    // ── M8 — Stock ────────────────────────────────────────────────
    DbSet<StockItem> Stock { get; }
    DbSet<MovimientoStock> MovimientosStock { get; }
    // ── M9 — Comprobantes ─────────────────────────────────────────
    DbSet<Comprobante> Comprobantes { get; }
    DbSet<ComprobanteItem> ComprobantesItems { get; }
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

    // ── M13 — Logística / Picking ─────────────────────────────────
    DbSet<OrdenPreparacion> OrdenesPreparacion { get; }
    DbSet<OrdenPreparacionDetalle> OrdenesPreparacionDetalles { get; }

    // ── M14 — Descuentos Comerciales ──────────────────────────────
    DbSet<DescuentoComercial> DescuentosComerciales { get; }

    // ── M15 — Retenciones por Persona ──────────────────────────
    DbSet<RetencionXPersona> RetencionesPorPersona { get; }

    // ─── Persistencia ─────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}