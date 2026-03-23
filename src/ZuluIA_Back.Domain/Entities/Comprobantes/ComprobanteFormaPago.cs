using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

/// <summary>
/// Forma de pago aplicada a un comprobante (cuotas, medios de pago en el momento de la venta/compra).
/// Permite desglosar el pago de un comprobante en varios medios (efectivo, transferencia, etc.).
/// Migrado desde VB6: clsComprobantesFormaPago / ComprobantesFormaPagos.
/// </summary>
public class ComprobanteFormaPago : BaseEntity
{
    public long      ComprobanteId { get; private set; }
    public long      FormaPagoId   { get; private set; }
    public DateOnly  Fecha         { get; private set; }
    public decimal   Importe       { get; private set; }
    public string?   Descripcion   { get; private set; }
    public string?   Observacion   { get; private set; }
    public bool      Valido        { get; private set; } = true;
    public long?     MonedaId      { get; private set; }
    public decimal   Cotizacion    { get; private set; } = 1m;

    private ComprobanteFormaPago() { }

    public static ComprobanteFormaPago Crear(long comprobanteId, long formaPagoId,
        DateOnly fecha, decimal importe, string? descripcion = null,
        string? observacion = null, long? monedaId = null, decimal cotizacion = 1m)
    {
        if (comprobanteId <= 0) throw new ArgumentException("El comprobante es requerido.");
        if (formaPagoId   <= 0) throw new ArgumentException("La forma de pago es requerida.");
        if (importe       <= 0) throw new ArgumentException("El importe debe ser mayor a cero.");
        if (cotizacion    <= 0) throw new ArgumentException("La cotización debe ser mayor a cero.");

        return new ComprobanteFormaPago
        {
            ComprobanteId = comprobanteId,
            FormaPagoId   = formaPagoId,
            Fecha         = fecha,
            Importe       = importe,
            Descripcion   = descripcion?.Trim(),
            Observacion   = observacion?.Trim(),
            Valido        = true,
            MonedaId      = monedaId,
            Cotizacion    = cotizacion
        };
    }

    public void Anular() => Valido = false;

    public void ActualizarImporte(decimal importe, decimal cotizacion = 1m)
    {
        if (importe    <= 0) throw new ArgumentException("El importe debe ser mayor a cero.");
        if (cotizacion <= 0) throw new ArgumentException("La cotización debe ser mayor a cero.");
        Importe    = importe;
        Cotizacion = cotizacion;
    }
}
