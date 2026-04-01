using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class ReciboItem : BaseEntity
{
    public long ReciboId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Importe { get; private set; }

    // Vinculación a comprobante imputado
    public long? ComprobanteImputadoId { get; private set; }

    private ReciboItem() { }

    public static ReciboItem Crear(
        long reciboId,
        string descripcion,
        decimal importe,
        long? comprobanteImputadoId = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(importe);
        return new ReciboItem
        {
            ReciboId                = reciboId,
            Descripcion             = descripcion.Trim(),
            Importe                 = importe,
            ComprobanteImputadoId   = comprobanteImputadoId
        };
    }
}
