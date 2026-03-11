namespace ZuluIA_Back.Domain.Enums;

/// <summary>
/// Tipos de movimiento de stock, cubriendo todos los escenarios de ingreso, egreso, ajustes, transferencias y producción.
/// </summary>
public enum TipoMovimientoStock
{
    // ── Ingresos ──────────────────────────────────
    Ingreso,                // Genérico
    CompraRecepcion,        // Recepción por compra
    DevolucionVenta,        // Devolución de cliente
    AjustePositivo,         // Ajuste por ingreso
    TransferenciaEntrada,   // Entrada por transferencia
    ProduccionIngreso,      // Ingreso por producción

    // ── Egresos ───────────────────────────────────
    Egreso,                 // Genérico
    VentaDespacho,          // Despacho por venta
    DevolucionCompra,       // Devolución a proveedor
    AjusteNegativo,         // Ajuste por egreso
    TransferenciaSalida,    // Salida por transferencia
    ProduccionConsumo,      // Consumo en producción

    // ── Ajustes ───────────────────────────────────
    Ajuste,                 // Ajuste genérico

    // ── Transferencias ────────────────────────────
    Transferencia,          // Transferencia genérica

    // ── Devoluciones ──────────────────────────────
    DevolucionCliente,      // Devolución de cliente (alias)
    DevolucionProveedor,    // Devolución a proveedor (alias)

    // ── Inicialización ────────────────────────────
    StockInicial            // Carga inicial de stock
}
