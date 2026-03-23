using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Ítem de una plantilla de diagnóstico (variable incluida en la plantilla).
/// Migrado desde VB6: clsPlantillaDiagnosticoDetalle / FRA_PLANTILLASDETALLE.
/// </summary>
public class PlantillaDiagnosticoDetalle : BaseEntity
{
    public long   PlantillaId            { get; private set; }  // plant_id
    public long?  VariableDetalleId      { get; private set; }  // vard_id
    public decimal PorcentajeIncidencia  { get; private set; }  // plantd_porcentajeIncidencia
    public decimal? ValorObjetivo        { get; private set; }  // plantd_valorObjetivo

    private PlantillaDiagnosticoDetalle() { }

    public static PlantillaDiagnosticoDetalle Crear(
        long plantillaId,
        long? variableDetalleId,
        decimal porcentajeIncidencia = 0,
        decimal? valorObjetivo       = null)
    {
        if (plantillaId <= 0) throw new ArgumentException("PlantillaId es requerido.");
        return new PlantillaDiagnosticoDetalle
        {
            PlantillaId          = plantillaId,
            VariableDetalleId    = variableDetalleId,
            PorcentajeIncidencia = porcentajeIncidencia,
            ValorObjetivo        = valorObjetivo
        };
    }

    public void Actualizar(long? variableDetalleId, decimal porcentajeIncidencia, decimal? valorObjetivo)
    {
        VariableDetalleId    = variableDetalleId;
        PorcentajeIncidencia = porcentajeIncidencia;
        ValorObjetivo        = valorObjetivo;
    }
}
