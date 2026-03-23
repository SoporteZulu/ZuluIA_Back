using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Impuestos;

/// <summary>
/// Asignación de un impuesto/percepción a un tipo de comprobante.
/// Migrado desde VB6: frmRentasBSAS / IMP_IMPUESTOXTIPOCOMPROBANTE.
/// Define en qué tipos de comprobante se aplica la percepción al emitir.
/// </summary>
public class ImpuestoPorTipoComprobante : BaseEntity
{
    public long ImpuestoId        { get; private set; }
    public long TipoComprobanteId { get; private set; }
    public int  Orden             { get; private set; }

    private ImpuestoPorTipoComprobante() { }

    public static ImpuestoPorTipoComprobante Crear(
        long impuestoId,
        long tipoComprobanteId,
        int  orden = 0)
    {
        if (impuestoId        <= 0) throw new ArgumentException("El impuesto es requerido.");
        if (tipoComprobanteId <= 0) throw new ArgumentException("El tipo de comprobante es requerido.");

        return new ImpuestoPorTipoComprobante
        {
            ImpuestoId        = impuestoId,
            TipoComprobanteId = tipoComprobanteId,
            Orden             = orden
        };
    }

    public void ActualizarOrden(int orden)
    {
        Orden = orden;
    }
}
