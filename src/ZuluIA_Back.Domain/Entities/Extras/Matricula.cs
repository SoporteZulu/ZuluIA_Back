using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

/// <summary>
/// Matrícula/inscripción de un tercero en el sistema (colegio, título, habilitación profesional, etc.).
/// Migrado desde VB6: MATRICULAS, MATRICULASCONSULTA.
/// </summary>
public class Matricula : AuditableEntity
{
    public long     TerceroId        { get; private set; }
    public long     SucursalId       { get; private set; }
    public string   NroMatricula     { get; private set; } = string.Empty;
    public string?  Descripcion      { get; private set; }
    public DateOnly FechaAlta        { get; private set; }
    public DateOnly? FechaVencimiento { get; private set; }
    public bool     Activa           { get; private set; } = true;

    private Matricula() { }

    public static Matricula Crear(
        long terceroId, long sucursalId, string nroMatricula,
        string? descripcion, DateOnly fechaAlta, DateOnly? fechaVencimiento, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroMatricula);
        var m = new Matricula
        {
            TerceroId        = terceroId,
            SucursalId       = sucursalId,
            NroMatricula     = nroMatricula.Trim(),
            Descripcion      = descripcion?.Trim(),
            FechaAlta        = fechaAlta,
            FechaVencimiento = fechaVencimiento,
            Activa           = true
        };
        m.SetCreated(userId);
        return m;
    }

    public void Actualizar(string? descripcion, DateOnly? fechaVencimiento, long? userId)
    {
        Descripcion      = descripcion?.Trim();
        FechaVencimiento = fechaVencimiento;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activa = true; SetUpdated(userId); }
}
