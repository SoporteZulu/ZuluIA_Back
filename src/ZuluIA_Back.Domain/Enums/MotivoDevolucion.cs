namespace ZuluIA_Back.Domain.Enums;

/// <summary>
/// Motivo de devolución de ventas o compras
/// </summary>
public enum MotivoDevolucion
{
    /// <summary>
    /// Producto defectuoso o con fallas
    /// </summary>
    ProductoDefectuoso = 1,
    
    /// <summary>
    /// Error en la entrega (producto equivocado)
    /// </summary>
    ErrorEntrega = 2,
    
    /// <summary>
    /// Cliente desiste de la compra
    /// </summary>
    DesistimientoCliente = 3,
    
    /// <summary>
    /// Vencimiento de producto
    /// </summary>
    ProductoVencido = 4,
    
    /// <summary>
    /// Diferencia de precio o facturación incorrecta
    /// </summary>
    DiferenciaPrecio = 5,
    
    /// <summary>
    /// Producto dañado en tránsito
    /// </summary>
    DanioTransito = 6,
    
    /// <summary>
    /// Garantía o reclamo
    /// </summary>
    Garantia = 7,
    
    /// <summary>
    /// Sobrante o excedente de pedido
    /// </summary>
    Sobrante = 8,
    
    /// <summary>
    /// Cambio por otro producto
    /// </summary>
    Cambio = 9,
    
    /// <summary>
    /// Ajuste de inventario
    /// </summary>
    AjusteInventario = 10,
    
    /// <summary>
    /// Otros motivos no especificados
    /// </summary>
    Otro = 99
}
