namespace ZuluIA_Back.Domain.Enums;

/// <summary>
/// Tipo de operación de devolución
/// </summary>
public enum TipoDevolucion
{
    /// <summary>
    /// Devolución sin reintegro de stock
    /// </summary>
    SinReintegroStock = 0,
    
    /// <summary>
    /// Devolución con reintegro de stock
    /// </summary>
    ConReintegroStock = 1,
    
    /// <summary>
    /// Devolución con reintegro de stock y acreditación cuenta corriente
    /// </summary>
    ConReintegroStockYAcreditacion = 2,
    
    /// <summary>
    /// Devolución solo con acreditación cuenta corriente (sin stock)
    /// </summary>
    SoloAcreditacion = 3
}
