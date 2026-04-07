using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

/// <summary>
/// Asignación de un centro de costo a una línea de comprobante.
/// Permite la imputación de costos por centro de costo al nivel de ítem de venta/compra.
/// Migrado desde VB6: clsComprobantesDetalleCostos / COMPROBANTESDETALLESCOSTOS.
/// </summary>
public class ComprobanteDetalleCosto : BaseEntity
{
    /// <summary>ID del detalle (línea de ítem) del comprobante.</summary>
    public long ComprobanteItemId { get; private set; }
    public long CentroCostoId     { get; private set; }
    public bool Procesado         { get; private set; }

    private ComprobanteDetalleCosto() { }

    public static ComprobanteDetalleCosto Crear(long comprobanteItemId, long centroCostoId)
    {
        if (comprobanteItemId <= 0) throw new ArgumentException("El ítem del comprobante es requerido.");
        if (centroCostoId     <= 0) throw new ArgumentException("El centro de costo es requerido.");

        return new ComprobanteDetalleCosto
        {
            ComprobanteItemId = comprobanteItemId,
            CentroCostoId     = centroCostoId,
            Procesado         = false
        };
    }

    public void MarcarProcesado()   => Procesado = true;
    public void DesmarcarProcesado()=> Procesado = false;
}
