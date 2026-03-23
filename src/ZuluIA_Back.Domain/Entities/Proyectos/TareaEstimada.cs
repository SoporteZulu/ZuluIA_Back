using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Proyectos;

/// <summary>
/// Tarea estimada/planificada dentro de un proyecto XerP.
/// Migrado desde VB6: XERPTAREASESTIMADAS.
/// </summary>
public class TareaEstimada : AuditableEntity
{
    public long    ProyectoId       { get; private set; }
    public long    SucursalId       { get; private set; }
    public long?   AsignadoA        { get; private set; }
    public string  Descripcion      { get; private set; } = string.Empty;
    public DateOnly FechaDesde      { get; private set; }
    public DateOnly FechaHasta      { get; private set; }
    public decimal HorasEstimadas   { get; private set; }
    public string? Observacion      { get; private set; }
    public bool    Activa           { get; private set; } = true;

    private TareaEstimada() { }

    public static TareaEstimada Crear(
        long proyectoId, long sucursalId, long? asignadoA,
        string descripcion, DateOnly fechaDesde, DateOnly fechaHasta,
        decimal horasEstimadas, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (horasEstimadas <= 0) throw new InvalidOperationException("Las horas estimadas deben ser mayores a 0.");
        var t = new TareaEstimada
        {
            ProyectoId     = proyectoId,
            SucursalId     = sucursalId,
            AsignadoA      = asignadoA,
            Descripcion    = descripcion.Trim(),
            FechaDesde     = fechaDesde,
            FechaHasta     = fechaHasta,
            HorasEstimadas = horasEstimadas,
            Observacion    = observacion?.Trim(),
            Activa         = true
        };
        t.SetCreated(userId);
        return t;
    }

    public void Actualizar(string descripcion, DateOnly fechaDesde, DateOnly fechaHasta, decimal horasEstimadas, long? asignadoA, string? obs, long? userId)
    {
        Descripcion    = descripcion.Trim();
        FechaDesde     = fechaDesde;
        FechaHasta     = fechaHasta;
        HorasEstimadas = horasEstimadas;
        AsignadoA      = asignadoA;
        Observacion    = obs?.Trim();
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }

    public void Activar(long? userId) { Activa = true; SetUpdated(userId); }
}
