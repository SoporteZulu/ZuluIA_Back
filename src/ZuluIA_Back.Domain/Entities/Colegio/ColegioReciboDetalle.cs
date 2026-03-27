using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Colegio;

public class ColegioReciboDetalle : BaseEntity
{
    public long CobroId { get; private set; }
    public long CedulonId { get; private set; }
    public decimal Importe { get; private set; }

    private ColegioReciboDetalle() { }

    public static ColegioReciboDetalle Crear(long cobroId, long cedulonId, decimal importe)
    {
        if (importe <= 0)
            throw new InvalidOperationException("El importe del recibo debe ser mayor a 0.");

        return new ColegioReciboDetalle
        {
            CobroId = cobroId,
            CedulonId = cedulonId,
            Importe = importe
        };
    }
}
