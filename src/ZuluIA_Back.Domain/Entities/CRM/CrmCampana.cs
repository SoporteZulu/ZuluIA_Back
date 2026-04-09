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
    public string   TipoCampana  { get; private set; } = "email";
    public string   Objetivo     { get; private set; } = "generacion_leads";
    public long?    SegmentoObjetivoId { get; private set; }
    public DateOnly FechaInicio  { get; private set; }
    public DateOnly FechaFin     { get; private set; }
    public decimal? Presupuesto  { get; private set; }
    public decimal? PresupuestoGastado { get; private set; }
    public long?    ResponsableId { get; private set; }
    public string?  Notas { get; private set; }
    public int      LeadsGenerados { get; private set; }
    public int      OportunidadesGeneradas { get; private set; }
    public int      NegociosGanados { get; private set; }
    public bool     Activa       { get; private set; } = true;

    private CrmCampana() { }

    public static CrmCampana Crear(
        long sucursalId, string nombre, string? descripcion,
        DateOnly fechaInicio, DateOnly fechaFin, decimal? presupuesto, long? userId,
        string tipoCampana = "email", string objetivo = "generacion_leads", long? segmentoObjetivoId = null,
        decimal? presupuestoGastado = 0m, long? responsableId = null, string? notas = null,
        int leadsGenerados = 0, int oportunidadesGeneradas = 0, int negociosGanados = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (fechaFin < fechaInicio) throw new InvalidOperationException("La fecha de fin no puede ser anterior a la de inicio.");
        var c = new CrmCampana
        {
            SucursalId  = sucursalId,
            Nombre      = nombre.Trim(),
            Descripcion = descripcion?.Trim(),
            TipoCampana = tipoCampana.Trim(),
            Objetivo = objetivo.Trim(),
            SegmentoObjetivoId = segmentoObjetivoId,
            FechaInicio = fechaInicio,
            FechaFin    = fechaFin,
            Presupuesto = presupuesto,
            PresupuestoGastado = presupuestoGastado,
            ResponsableId = responsableId,
            Notas = notas?.Trim(),
            LeadsGenerados = leadsGenerados,
            OportunidadesGeneradas = oportunidadesGeneradas,
            NegociosGanados = negociosGanados,
            Activa      = true
        };
        c.SetCreated(userId);
        return c;
    }

    public void Actualizar(
        string nombre,
        string? descripcion,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        decimal? presupuesto,
        long? userId,
        string tipoCampana = "email",
        string objetivo = "generacion_leads",
        long? segmentoObjetivoId = null,
        decimal? presupuestoGastado = 0m,
        long? responsableId = null,
        string? notas = null,
        int leadsGenerados = 0,
        int oportunidadesGeneradas = 0,
        int negociosGanados = 0)
    {
        Nombre      = nombre.Trim();
        Descripcion = descripcion?.Trim();
        TipoCampana = tipoCampana.Trim();
        Objetivo = objetivo.Trim();
        SegmentoObjetivoId = segmentoObjetivoId;
        FechaInicio = fechaInicio;
        FechaFin    = fechaFin;
        Presupuesto = presupuesto;
        PresupuestoGastado = presupuestoGastado;
        ResponsableId = responsableId;
        Notas = notas?.Trim();
        LeadsGenerados = leadsGenerados;
        OportunidadesGeneradas = oportunidadesGeneradas;
        NegociosGanados = negociosGanados;
        SetUpdated(userId);
    }

    public void Cerrar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }
}
