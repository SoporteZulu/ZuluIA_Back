using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

/// <summary>
/// Representa la imputación (aplicación) de un comprobante contra otro.
/// Ejemplo: NC aplicada a una Factura, o un Pago contra una Factura.
/// </summary>
public class Imputacion : BaseEntity
{
    public long ComprobanteOrigenId { get; private set; }
    public long ComprobanteDestinoId { get; private set; }
    public decimal Importe { get; private set; }
    public DateOnly Fecha { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public long? CreatedBy { get; private set; }

    private Imputacion() { }

    public static Imputacion Crear(
        long comprobanteOrigenId,
        long comprobanteDestinoId,
        decimal importe,
        DateOnly fecha,
        long? userId)
    {
        if (comprobanteOrigenId == comprobanteDestinoId)
            throw new InvalidOperationException(
                "No se puede imputar un comprobante contra sí mismo.");

        if (importe <= 0)
            throw new InvalidOperationException(
                "El importe de la imputación debe ser mayor a 0.");

        return new Imputacion
        {
            ComprobanteOrigenId  = comprobanteOrigenId,
            ComprobanteDestinoId = comprobanteDestinoId,
            Importe              = importe,
            Fecha                = fecha,
            CreatedAt            = DateTimeOffset.UtcNow,
            CreatedBy            = userId
        };
    }
}