using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Plantilla de diagnóstico (modelo reutilizable para generar planillas).
/// Migrado desde VB6: clsPlantillaDiagnostico / FRA_PLANTILLAS.
/// </summary>
public class PlantillaDiagnostico : BaseEntity
{
    public string  Descripcion    { get; private set; } = "";  // plant_descripcion
    public DateTime? FechaDesde   { get; private set; }  // plant_FechaDesde
    public DateTime? FechaHasta   { get; private set; }  // plant_fechahasta
    public DateTime? FechaRegistro { get; private set; } // plant_fechaRegistro
    public string? Observaciones  { get; private set; }  // plant_observaciones

    private PlantillaDiagnostico() { }

    public static PlantillaDiagnostico Crear(
        string descripcion,
        DateTime? fechaDesde  = null,
        DateTime? fechaHasta  = null,
        string? observaciones = null)
    {
        if (string.IsNullOrWhiteSpace(descripcion)) throw new ArgumentException("La descripción es requerida.");
        return new PlantillaDiagnostico
        {
            Descripcion    = descripcion.Trim(),
            FechaDesde     = fechaDesde,
            FechaHasta     = fechaHasta,
            FechaRegistro  = DateTime.UtcNow,
            Observaciones  = observaciones
        };
    }

    public void Actualizar(string descripcion, DateTime? fechaDesde, DateTime? fechaHasta, string? observaciones)
    {
        if (string.IsNullOrWhiteSpace(descripcion)) throw new ArgumentException("La descripción es requerida.");
        Descripcion   = descripcion.Trim();
        FechaDesde    = fechaDesde;
        FechaHasta    = fechaHasta;
        Observaciones = observaciones;
    }
}
