using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Escala (tramo) de cálculo de un tipo de retención.
/// Equivale a la tabla RETENCION_ESCALA del sistema VB6.
/// Define el porcentaje a aplicar según el rango de base imponible.
/// </summary>
public class EscalaRetencion : BaseEntity
{
    public long TipoRetencionId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Importe mínimo de la base imponible para aplicar este tramo.
    /// </summary>
    public decimal ImporteDesde { get; private set; }

    /// <summary>
    /// Importe máximo del tramo (0 = sin límite superior).
    /// </summary>
    public decimal ImporteHasta { get; private set; }

    /// <summary>
    /// Porcentaje a aplicar sobre la base imponible neta (0-100).
    /// </summary>
    public decimal Porcentaje { get; private set; }

    private EscalaRetencion() { }

    internal static EscalaRetencion Crear(
        long tipoRetencionId,
        string descripcion,
        decimal importeDesde,
        decimal importeHasta,
        decimal porcentaje)
    {
        return new EscalaRetencion
        {
            TipoRetencionId = tipoRetencionId,
            Descripcion     = descripcion.Trim(),
            ImporteDesde    = importeDesde,
            ImporteHasta    = importeHasta,
            Porcentaje      = porcentaje
        };
    }
}
