using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Proyectos;

/// <summary>
/// Registro de horas/trabajo real ejecutado en un proyecto XerP.
/// Migrado desde VB6: XERPTAREASREALES, XERPTAREASREALESADMIN.
/// </summary>
public class TareaReal : AuditableEntity
{
    public long    ProyectoId      { get; private set; }
    public long    SucursalId      { get; private set; }
    public long?   TareaEstimadaId { get; private set; }
    public long    UsuarioId       { get; private set; }
    public DateOnly Fecha          { get; private set; }
    public string   Descripcion    { get; private set; } = string.Empty;
    public decimal  HorasReales    { get; private set; }
    public bool     Aprobada       { get; private set; }
    public string?  Observacion    { get; private set; }

    private TareaReal() { }

    public static TareaReal Registrar(
        long proyectoId, long sucursalId, long? tareaEstimadaId,
        long usuarioId, DateOnly fecha, string descripcion,
        decimal horasReales, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (horasReales <= 0) throw new InvalidOperationException("Las horas reales deben ser mayores a 0.");
        var t = new TareaReal
        {
            ProyectoId      = proyectoId,
            SucursalId      = sucursalId,
            TareaEstimadaId = tareaEstimadaId,
            UsuarioId       = usuarioId,
            Fecha           = fecha,
            Descripcion     = descripcion.Trim(),
            HorasReales     = horasReales,
            Observacion     = observacion?.Trim(),
            Aprobada        = false
        };
        t.SetCreated(userId);
        return t;
    }

    public void Aprobar(long? userId) { Aprobada = true; SetUpdated(userId); }
    public void Eliminar(long? userId) { SetDeleted(); SetUpdated(userId); }

    public void Actualizar(string descripcion, decimal horasReales, string? obs, long? userId)
    {
        if (Aprobada) throw new InvalidOperationException("No se puede modificar una tarea ya aprobada.");
        Descripcion = descripcion.Trim();
        HorasReales = horasReales;
        Observacion = obs?.Trim();
        SetUpdated(userId);
    }
}
