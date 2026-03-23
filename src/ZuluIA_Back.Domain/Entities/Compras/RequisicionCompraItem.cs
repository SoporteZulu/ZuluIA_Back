using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Compras;

public class RequisicionCompraItem : BaseEntity
{
    public long RequisicionId { get; private set; }
    public long? ItemId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public string UnidadMedida { get; private set; } = string.Empty;
    public string? Observacion { get; private set; }

    private RequisicionCompraItem() { }

    public static RequisicionCompraItem Crear(
        long requisicionId,
        long? itemId,
        string descripcion,
        decimal cantidad,
        string unidadMedida,
        string? observacion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cantidad);
        return new RequisicionCompraItem
        {
            RequisicionId = requisicionId,
            ItemId        = itemId,
            Descripcion   = descripcion.Trim(),
            Cantidad      = cantidad,
            UnidadMedida  = unidadMedida.Trim(),
            Observacion   = observacion?.Trim()
        };
    }
}
