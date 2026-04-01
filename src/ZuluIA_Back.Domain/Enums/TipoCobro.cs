namespace ZuluIA_Back.Domain.Enums;

/// <summary>
/// Tipo de cobro según el canal/contexto operativo
/// </summary>
public enum TipoCobro
{
    /// <summary>
    /// Cobro administrativo normal, no asociado a ventanilla
    /// </summary>
    Administrativo = 0,

    /// <summary>
    /// Cobro en ventanilla/mostrador contra entrega (venta directa)
    /// </summary>
    VentanillaContraEntrega = 1,

    /// <summary>
    /// Cobro en ventanilla contra pedido (cliente retira con factura previa)
    /// </summary>
    VentanillaContraPedido = 2,

    /// <summary>
    /// Cobro en ruta por cobrador
    /// </summary>
    CobranzaEnRuta = 3,

    /// <summary>
    /// Cobro por transferencia/depósito bancario
    /// </summary>
    Bancario = 4,

    /// <summary>
    /// Cobro electrónico (mercado pago, etc)
    /// </summary>
    Electronico = 5
}
