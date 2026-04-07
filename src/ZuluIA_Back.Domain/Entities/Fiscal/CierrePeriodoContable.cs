using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Fiscal;

public class CierrePeriodoContable : AuditableEntity
{
    public long EjercicioId { get; private set; }
    public long? SucursalId { get; private set; }
    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public string? Observacion { get; private set; }

    private CierrePeriodoContable() { }

    public static CierrePeriodoContable Crear(long ejercicioId, long? sucursalId, DateOnly desde, DateOnly hasta, string? observacion, long? userId)
    {
        if (hasta < desde)
            throw new InvalidOperationException("La fecha hasta no puede ser anterior a la fecha desde.");

        var cierre = new CierrePeriodoContable
        {
            EjercicioId = ejercicioId,
            SucursalId = sucursalId,
            Desde = desde,
            Hasta = hasta,
            Observacion = observacion?.Trim()
        };

        cierre.SetCreated(userId);
        return cierre;
    }
}
