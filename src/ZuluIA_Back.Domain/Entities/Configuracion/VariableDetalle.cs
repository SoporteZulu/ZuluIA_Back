using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Detalle / ítem de una Variable dinámica.
/// Migrado desde VB6: clsVariablesDetalle / FRA_VARIABLESDETALLE.
/// Cada variable tiene una colección de detalles que representan sus ítems configurables.
/// </summary>
public class VariableDetalle : BaseEntity
{
    public long  VariableId                   { get; private set; }  // var_id
    public long? OpcionVariableId             { get; private set; }  // opc_id
    public bool  AplicaPuntajePenalizacion    { get; private set; }  // vard_aplicaPuntajePenalizacion
    public bool  VisualizarOpcion             { get; private set; }  // vard_visualizarOpcion
    public decimal? PorcentajeIncidencia      { get; private set; }  // vard_porcentajeIncidencia
    public decimal? ValorObjetivo             { get; private set; }  // vard_valorObjetivo

    private VariableDetalle() { }

    public static VariableDetalle Crear(
        long variableId,
        long? opcionVariableId,
        bool aplicaPuntajePenalizacion = false,
        bool visualizarOpcion          = true,
        decimal? porcentajeIncidencia  = null,
        decimal? valorObjetivo         = null)
    {
        if (variableId <= 0) throw new ArgumentException("VariableId es requerido.");
        return new VariableDetalle
        {
            VariableId                = variableId,
            OpcionVariableId          = opcionVariableId,
            AplicaPuntajePenalizacion = aplicaPuntajePenalizacion,
            VisualizarOpcion          = visualizarOpcion,
            PorcentajeIncidencia      = porcentajeIncidencia,
            ValorObjetivo             = valorObjetivo
        };
    }

    public void Actualizar(
        long? opcionVariableId,
        bool aplicaPuntajePenalizacion,
        bool visualizarOpcion,
        decimal? porcentajeIncidencia,
        decimal? valorObjetivo)
    {
        OpcionVariableId          = opcionVariableId;
        AplicaPuntajePenalizacion = aplicaPuntajePenalizacion;
        VisualizarOpcion          = visualizarOpcion;
        PorcentajeIncidencia      = porcentajeIncidencia;
        ValorObjetivo             = valorObjetivo;
    }
}
