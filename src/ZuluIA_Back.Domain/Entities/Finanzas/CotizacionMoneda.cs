using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class CotizacionMoneda : BaseEntity
{
    public long MonedaId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal Cotizacion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private CotizacionMoneda() { }

    public static CotizacionMoneda Crear(long monedaId, DateOnly fecha, decimal cotizacion)
    {
        if (cotizacion <= 0)
            throw new InvalidOperationException("La cotización debe ser mayor a 0.");

        return new CotizacionMoneda
        {
            MonedaId   = monedaId,
            Fecha      = fecha,
            Cotizacion = cotizacion,
            CreatedAt  = DateTimeOffset.UtcNow
        };
    }

    public void ActualizarCotizacion(decimal nuevaCotizacion)
    {
        if (nuevaCotizacion <= 0)
            throw new InvalidOperationException("La cotización debe ser mayor a 0.");

        Cotizacion = nuevaCotizacion;
    }
}