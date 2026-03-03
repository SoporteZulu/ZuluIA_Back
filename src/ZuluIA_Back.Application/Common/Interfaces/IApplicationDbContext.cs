using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Precios;

//using ZuluIA_Back.Domain.Entities.Referencia;   // <- descomentar cuando tengas esos modelos
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
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
    DbSet<TipoDocumento> TiposDocumento { get; }
    DbSet<CondicionIva> CondicionesIva { get; }
    DbSet<CategoriaTercero> CategoriasTerceros { get; }
    DbSet<Moneda> Monedas { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<TipoComprobante> TiposComprobante { get; }
    DbSet<AlicuotaIva> AlicuotasIva { get; }
    DbSet<UnidadMedida> UnidadesMedida { get; }
    DbSet<FormaPago> FormasPago { get; }

    // ─── Items ────────────────────────────────────────────────────────────────
    DbSet<Item> Items { get; }

    // ─── Stock ────────────────────────────────────────────────────────────────
    DbSet<StockItem> Stock { get; }
    DbSet<MovimientoStock> MovimientosStock { get; }

    // ─── Comprobantes ─────────────────────────────────────────────────────────
    DbSet<Comprobante> Comprobantes { get; }
    DbSet<ComprobanteItem> ComprobantesItems { get; }

    // ─── Finanzas ─────────────────────────────────────────────────────────────
    DbSet<Cobro> Cobros { get; }
    DbSet<CobroMedio> CobrosMedios { get; }
    DbSet<Pago> Pagos { get; }
    DbSet<PagoMedio> PagosMedios { get; }

    // ─── Contabilidad ─────────────────────────────────────────────────────────
    DbSet<Asiento> Asientos { get; }
    DbSet<AsientoLinea> AsientosLineas { get; }

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

    // ─── Persistencia ─────────────────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}