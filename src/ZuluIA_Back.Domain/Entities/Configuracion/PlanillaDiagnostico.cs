using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Planilla de diagnóstico (evaluación aplicada a un cliente).
/// Migrado desde VB6: clsPlanillaDiagnostico / FRA_PLANILLAS.
/// Contiene la cabecera de una evaluación; los ítems evaluados están en PlanillaDiagnosticoDetalle.
/// </summary>
public class PlanillaDiagnostico : BaseEntity
{
    public long?   ClienteId         { get; private set; }  // cli_id
    public long?   PlantillaId       { get; private set; }  // plant_id (plantilla origen)
    public long?   TipoPlanillaId    { get; private set; }  // tplan_id
    public long?   PlanillaPadreId   { get; private set; }  // plan_idPlanillaPadre
    public long?   EstadoId          { get; private set; }  // estp_id
    public DateTime? FechaEvaluacion { get; private set; }  // plan_fechaEvaluacion
    public DateTime? FechaDesde      { get; private set; }  // plan_fechaDesde
    public DateTime? FechaHasta      { get; private set; }  // plan_fechahasta
    public DateTime? FechaRegistro   { get; private set; }  // plan_fechaRegistro
    public string? Observaciones     { get; private set; }  // plan_observaciones

    private PlanillaDiagnostico() { }

    public static PlanillaDiagnostico Crear(
        long? clienteId,
        long? plantillaId      = null,
        long? tipoPlanillaId   = null,
        long? planillaPadreId  = null,
        long? estadoId         = null,
        DateTime? fechaEvaluacion = null,
        DateTime? fechaDesde   = null,
        DateTime? fechaHasta   = null,
        string? observaciones  = null)
    {
        return new PlanillaDiagnostico
        {
            ClienteId       = clienteId,
            PlantillaId     = plantillaId,
            TipoPlanillaId  = tipoPlanillaId,
            PlanillaPadreId = planillaPadreId,
            EstadoId        = estadoId,
            FechaEvaluacion = fechaEvaluacion,
            FechaDesde      = fechaDesde,
            FechaHasta      = fechaHasta,
            FechaRegistro   = DateTime.UtcNow,
            Observaciones   = observaciones
        };
    }

    public void Actualizar(
        long? clienteId,
        long? plantillaId,
        long? tipoPlanillaId,
        long? planillaPadreId,
        long? estadoId,
        DateTime? fechaEvaluacion,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string? observaciones)
    {
        ClienteId       = clienteId;
        PlantillaId     = plantillaId;
        TipoPlanillaId  = tipoPlanillaId;
        PlanillaPadreId = planillaPadreId;
        EstadoId        = estadoId;
        FechaEvaluacion = fechaEvaluacion;
        FechaDesde      = fechaDesde;
        FechaHasta      = fechaHasta;
        Observaciones   = observaciones;
    }
}
