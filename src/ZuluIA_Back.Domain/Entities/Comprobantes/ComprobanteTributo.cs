using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class ComprobanteTributo : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public long? ImpuestoId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public decimal BaseImponible { get; private set; }
    public decimal Alicuota { get; private set; }
    public decimal Importe { get; private set; }
    public int Orden { get; private set; }

    private ComprobanteTributo() { }

    public static ComprobanteTributo Crear(
        long comprobanteId,
        long? impuestoId,
        string codigo,
        string descripcion,
        decimal baseImponible,
        decimal alicuota,
        decimal importe,
        int orden = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new ComprobanteTributo
        {
            ComprobanteId = comprobanteId,
            ImpuestoId = impuestoId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            BaseImponible = baseImponible,
            Alicuota = alicuota,
            Importe = importe,
            Orden = orden
        };
    }
}