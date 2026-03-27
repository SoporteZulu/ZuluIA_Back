using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Stock;

/// <summary>
/// Valor de un atributo (lote, serie, vencimiento, etc.) para un movimiento de stock.
/// Permite trazabilidad por número de serie/lote.
/// Migrado desde VB6: clsMovimientosStockAtributos / MOVIMIENTOSTOCKATRIBUTOS.
/// </summary>
public class MovimientoStockAtributo : BaseEntity
{
    public long   MovimientoStockId { get; private set; }
    public long   AtributoId        { get; private set; }
    /// <summary>Valor del atributo (ej: "LOTE-2024-01", "S/N 123456", "2026-12-31").</summary>
    public string Valor             { get; private set; } = string.Empty;

    private MovimientoStockAtributo() { }

    public static MovimientoStockAtributo Crear(long movimientoStockId, long atributoId, string valor)
    {
        if (movimientoStockId <= 0) throw new ArgumentException("El movimiento de stock es requerido.");
        if (atributoId        <= 0) throw new ArgumentException("El atributo es requerido.");
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        return new MovimientoStockAtributo
        {
            MovimientoStockId = movimientoStockId,
            AtributoId        = atributoId,
            Valor             = valor.Trim()
        };
    }

    public void ActualizarValor(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);
        Valor = valor.Trim();
    }
}
