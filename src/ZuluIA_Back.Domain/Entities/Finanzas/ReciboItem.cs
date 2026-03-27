using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class ReciboItem : BaseEntity
{
    public long ReciboId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Importe { get; private set; }

    private ReciboItem() { }

    public static ReciboItem Crear(long reciboId, string descripcion, decimal importe)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(importe);
        return new ReciboItem
        {
            ReciboId    = reciboId,
            Descripcion = descripcion.Trim(),
            Importe     = importe
        };
    }
}
