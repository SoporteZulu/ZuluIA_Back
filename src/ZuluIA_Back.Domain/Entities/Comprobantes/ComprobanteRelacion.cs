namespace ZuluIA_Back.Domain.Entities.Comprobantes;

using ZuluIA_Back.Domain.Common;

/// <summary>
/// Relación entre dos comprobantes (ej. factura ← nota de crédito).
/// Mapea a la tabla comprobantes_relaciones.
/// </summary>
public class ComprobanteRelacion : BaseEntity
{
    public long ComprobanteOrigenId  { get; private set; }
    public long ComprobanteDestinoId { get; private set; }
    public string? Observacion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private ComprobanteRelacion() { }

    public static ComprobanteRelacion Crear(
        long origenId,
        long destinoId,
        string? observacion = null)
    {
        return new ComprobanteRelacion
        {
            ComprobanteOrigenId  = origenId,
            ComprobanteDestinoId = destinoId,
            Observacion          = observacion?.Trim(),
            CreatedAt            = DateTimeOffset.UtcNow,
        };
    }
}
