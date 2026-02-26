using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class CobroMedio : BaseEntity
{
    public long CobroId { get; private set; }
    public long CajaId { get; private set; }
    public long FormaPagoId { get; private set; }
    public long? ChequeId { get; private set; }
    public decimal Importe { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1;

    private CobroMedio() { }

    public static CobroMedio Crear(
        long cobroId,
        long cajaId,
        long formaPagoId,
        decimal importe,
        long monedaId,
        decimal cotizacion,
        long? chequeId = null)
    {
        if (importe <= 0)
            throw new InvalidOperationException("El importe del medio de cobro debe ser mayor a 0.");

        return new CobroMedio
        {
            CobroId     = cobroId,
            CajaId      = cajaId,
            FormaPagoId = formaPagoId,
            Importe     = importe,
            MonedaId    = monedaId,
            Cotizacion  = cotizacion,
            ChequeId    = chequeId
        };
    }
}