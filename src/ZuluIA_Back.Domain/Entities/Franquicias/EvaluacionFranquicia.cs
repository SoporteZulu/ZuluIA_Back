using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Franquicias;

public class EvaluacionFranquicia : AuditableEntity
{
    public long PlanTrabajoId { get; private set; }
    public long SucursalId { get; private set; }
    public int Periodo { get; private set; }
    public decimal PuntajeTotal { get; private set; }
    public DateOnly FechaEvaluacion { get; private set; }
    public string? Observacion { get; private set; }

    private readonly List<EvaluacionFranquiciaDetalle> _detalles = [];
    public IReadOnlyCollection<EvaluacionFranquiciaDetalle> Detalles => _detalles.AsReadOnly();

    private EvaluacionFranquicia() { }

    public static EvaluacionFranquicia Crear(
        long planTrabajoId,
        long sucursalId,
        int periodo,
        DateOnly fechaEvaluacion,
        string? observacion,
        long? userId)
    {
        var eval = new EvaluacionFranquicia
        {
            PlanTrabajoId   = planTrabajoId,
            SucursalId      = sucursalId,
            Periodo         = periodo,
            FechaEvaluacion = fechaEvaluacion,
            PuntajeTotal    = 0,
            Observacion     = observacion?.Trim()
        };

        eval.SetCreated(userId);
        return eval;
    }

    public void AgregarDetalle(EvaluacionFranquiciaDetalle detalle)
    {
        _detalles.Add(detalle);
        PuntajeTotal = _detalles.Sum(d => d.Puntaje);
    }
}
