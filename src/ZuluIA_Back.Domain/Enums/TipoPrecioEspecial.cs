namespace ZuluIA_Back.Domain.Enums;

/// <summary>
/// Tipo de precio especial aplicable.
/// Define la prioridad en la resolución de precios.
/// </summary>
public enum TipoPrecioEspecial
{
    /// <summary>
    /// Precio especial por cliente específico (máxima prioridad).
    /// </summary>
    Cliente = 1,

    /// <summary>
    /// Precio especial por canal de venta.
    /// </summary>
    Canal = 2,

    /// <summary>
    /// Precio especial por vendedor.
    /// </summary>
    Vendedor = 3,

    /// <summary>
    /// Precio especial por categoría de cliente.
    /// </summary>
    CategoriaCliente = 4,

    /// <summary>
    /// Precio especial por zona comercial.
    /// </summary>
    ZonaComercial = 5
}
