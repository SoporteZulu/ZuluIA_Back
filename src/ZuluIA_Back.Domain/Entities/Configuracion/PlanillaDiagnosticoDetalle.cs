using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Ítem de una planilla de diagnóstico (resultado por variable evaluada).
/// Migrado desde VB6: clsPlanillaDiagnosticoDetalle / FRA_PLANILLASDETALLE.
/// </summary>
public class PlanillaDiagnosticoDetalle : BaseEntity
{
    public long     PlanillaId               { get; private set; }  // plan_id
    public long?    VariableDetalleId        { get; private set; }  // vard_id
    public long?    OpcionVariableId         { get; private set; }  // opc_id
    public decimal  PuntajeVariable          { get; private set; }  // pland_puntajeVariable
    public decimal  Valor                    { get; private set; }  // pland_valor
    public decimal  PorcentajeIncidencia     { get; private set; }  // pland_porcentajeIncidencia
    public decimal? ValorObjetivo            { get; private set; }  // pland_valorObjetivo

    private PlanillaDiagnosticoDetalle() { }

    public static PlanillaDiagnosticoDetalle Crear(
        long planillaId,
        long? variableDetalleId,
        long? opcionVariableId,
        decimal puntajeVariable      = 0,
        decimal valor                = 0,
        decimal porcentajeIncidencia = 0,
        decimal? valorObjetivo       = null)
    {
        if (planillaId <= 0) throw new ArgumentException("PlanillaId es requerido.");
        return new PlanillaDiagnosticoDetalle
        {
            PlanillaId           = planillaId,
            VariableDetalleId    = variableDetalleId,
            OpcionVariableId     = opcionVariableId,
            PuntajeVariable      = puntajeVariable,
            Valor                = valor,
            PorcentajeIncidencia = porcentajeIncidencia,
            ValorObjetivo        = valorObjetivo
        };
    }

    public void Actualizar(
        long? opcionVariableId,
        decimal puntajeVariable,
        decimal valor,
        decimal porcentajeIncidencia,
        decimal? valorObjetivo)
    {
        OpcionVariableId     = opcionVariableId;
        PuntajeVariable      = puntajeVariable;
        Valor                = valor;
        PorcentajeIncidencia = porcentajeIncidencia;
        ValorObjetivo        = valorObjetivo;
    }
}
