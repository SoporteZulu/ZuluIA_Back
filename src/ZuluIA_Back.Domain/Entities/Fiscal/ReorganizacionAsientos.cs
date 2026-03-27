using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Fiscal;

public class ReorganizacionAsientos : AuditableEntity
{
    public long EjercicioId { get; private set; }
    public long? SucursalId { get; private set; }
    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public int CantidadAsientos { get; private set; }
    public string? Observacion { get; private set; }

    private ReorganizacionAsientos() { }

    public static ReorganizacionAsientos Registrar(long ejercicioId, long? sucursalId, DateOnly desde, DateOnly hasta, int cantidadAsientos, string? observacion, long? userId)
    {
        if (hasta < desde)
            throw new InvalidOperationException("La fecha hasta no puede ser anterior a la fecha desde.");
        if (cantidadAsientos < 0)
            throw new InvalidOperationException("La cantidad de asientos no puede ser negativa.");

        var registro = new ReorganizacionAsientos
        {
            EjercicioId = ejercicioId,
            SucursalId = sucursalId,
            Desde = desde,
            Hasta = hasta,
            CantidadAsientos = cantidadAsientos,
            Observacion = observacion?.Trim()
        };

        registro.SetCreated(userId);
        return registro;
    }
}
