using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Franquicias;

public class PlanTrabajo : AuditableEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public long SucursalId { get; private set; }
    public int Periodo { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public EstadoPlanTrabajo Estado { get; private set; }
    public string? Descripcion { get; private set; }

    private readonly List<PlanTrabajoKpi> _kpis = [];
    public IReadOnlyCollection<PlanTrabajoKpi> Kpis => _kpis.AsReadOnly();

    private PlanTrabajo() { }

    public static PlanTrabajo Crear(
        string nombre,
        long sucursalId,
        int periodo,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        string? descripcion,
        long? userId)
    {
        var plan = new PlanTrabajo
        {
            Nombre      = nombre.Trim(),
            SucursalId  = sucursalId,
            Periodo     = periodo,
            FechaInicio = fechaInicio,
            FechaFin    = fechaFin,
            Estado      = EstadoPlanTrabajo.Activo,
            Descripcion = descripcion?.Trim()
        };

        plan.SetCreated(userId);
        return plan;
    }

    public void AgregarKpi(PlanTrabajoKpi kpi)
    {
        if (Estado != EstadoPlanTrabajo.Activo)
            throw new InvalidOperationException("Solo se pueden agregar KPIs a planes activos.");
        _kpis.Add(kpi);
    }

    public void Cerrar(long? userId)
    {
        if (Estado != EstadoPlanTrabajo.Activo)
            throw new InvalidOperationException("Solo se pueden cerrar planes activos.");
        Estado = EstadoPlanTrabajo.Cerrado;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoPlanTrabajo.Anulado)
            throw new InvalidOperationException("El plan ya está anulado.");
        Estado = EstadoPlanTrabajo.Anulado;
        SetDeleted();
        SetUpdated(userId);
    }
}
