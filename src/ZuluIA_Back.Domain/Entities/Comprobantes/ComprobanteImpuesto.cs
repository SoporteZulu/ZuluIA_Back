namespace ZuluIA_Back.Domain.Entities.Comprobantes;

using ZuluIA_Back.Domain.Common;

/// <summary>
/// Desglose de IVA por alícuota de un comprobante.
/// Mapea a la tabla comprobantes_impuestos.
/// </summary>
public class ComprobanteImpuesto : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public long AlicuotaIvaId { get; private set; }
    public decimal PorcentajeIva { get; private set; }
    public decimal BaseImponible { get; private set; }
    public decimal ImporteIva { get; private set; }

    private ComprobanteImpuesto() { }

    public static ComprobanteImpuesto Crear(
        long comprobanteId,
        long alicuotaIvaId,
        decimal porcentajeIva,
        decimal baseImponible,
        decimal importeIva)
    {
        return new ComprobanteImpuesto
        {
            ComprobanteId = comprobanteId,
            AlicuotaIvaId = alicuotaIvaId,
            PorcentajeIva = porcentajeIva,
            BaseImponible = baseImponible,
            ImporteIva    = importeIva,
        };
    }
}
