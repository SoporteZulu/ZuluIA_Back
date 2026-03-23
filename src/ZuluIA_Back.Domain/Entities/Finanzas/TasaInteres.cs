using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class TasaInteres : AuditableEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public decimal TasaMensual { get; private set; }
    public DateOnly FechaDesde { get; private set; }
    public DateOnly? FechaHasta { get; private set; }
    public bool Activo { get; private set; }

    private TasaInteres() { }

    public static TasaInteres Crear(
        string descripcion,
        decimal tasaMensual,
        DateOnly fechaDesde,
        DateOnly? fechaHasta,
        long? userId)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tasaMensual);
        var tasa = new TasaInteres
        {
            Descripcion  = descripcion.Trim(),
            TasaMensual  = tasaMensual,
            FechaDesde   = fechaDesde,
            FechaHasta   = fechaHasta,
            Activo       = true
        };

        tasa.SetCreated(userId);
        return tasa;
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activo = true;
        SetUpdated(userId);
    }
}
