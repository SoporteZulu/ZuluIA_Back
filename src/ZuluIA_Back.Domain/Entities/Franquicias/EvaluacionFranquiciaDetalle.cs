using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Franquicias;

public class EvaluacionFranquiciaDetalle : BaseEntity
{
    public long EvaluacionId { get; private set; }
    public long KpiId { get; private set; }
    public decimal ValorReal { get; private set; }
    public decimal Puntaje { get; private set; }
    public string? Observacion { get; private set; }

    private EvaluacionFranquiciaDetalle() { }

    public static EvaluacionFranquiciaDetalle Crear(
        long evaluacionId,
        long kpiId,
        decimal valorReal,
        decimal puntaje,
        string? observacion)
    {
        return new EvaluacionFranquiciaDetalle
        {
            EvaluacionId = evaluacionId,
            KpiId        = kpiId,
            ValorReal    = valorReal,
            Puntaje      = puntaje,
            Observacion  = observacion?.Trim()
        };
    }
}
