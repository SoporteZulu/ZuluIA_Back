using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

/// <summary>
/// Campaña de marketing del módulo CRM.
/// Migrado desde VB6: CRMCAMPANA.
/// </summary>
public class CrmCampana : AuditableEntity
{
    public long     SucursalId   { get; private set; }
    public string   Nombre       { get; private set; } = string.Empty;
    public string?  Descripcion  { get; private set; }
    public DateOnly FechaInicio  { get; private set; }
    public DateOnly FechaFin     { get; private set; }
    public decimal? Presupuesto  { get; private set; }
    public bool     Activa       { get; private set; } = true;

    private CrmCampana() { }

    public static CrmCampana Crear(
        long sucursalId, string nombre, string? descripcion,
        DateOnly fechaInicio, DateOnly fechaFin, decimal? presupuesto, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (fechaFin < fechaInicio) throw new InvalidOperationException("La fecha de fin no puede ser anterior a la de inicio.");
        var c = new CrmCampana
        {
            SucursalId  = sucursalId,
            Nombre      = nombre.Trim(),
            Descripcion = descripcion?.Trim(),
            FechaInicio = fechaInicio,
            FechaFin    = fechaFin,
            Presupuesto = presupuesto,
            Activa      = true
        };
        c.SetCreated(userId);
        return c;
    }

    public void Actualizar(string nombre, string? descripcion, DateOnly fechaInicio, DateOnly fechaFin, decimal? presupuesto, long? userId)
    {
        Nombre      = nombre.Trim();
        Descripcion = descripcion?.Trim();
        FechaInicio = fechaInicio;
        FechaFin    = fechaFin;
        Presupuesto = presupuesto;
        SetUpdated(userId);
    }

    public void Cerrar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }
}
