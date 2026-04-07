using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Agro;

public class LiquidacionGranosConcepto : BaseEntity
{
    public long LiquidacionId { get; private set; }
    public string Concepto { get; private set; } = string.Empty;
    public decimal Importe { get; private set; }
    public bool EsDeduccion { get; private set; }

    private LiquidacionGranosConcepto() { }

    public static LiquidacionGranosConcepto Crear(
        long liquidacionId,
        string concepto,
        decimal importe,
        bool esDeduccion)
    {
        return new LiquidacionGranosConcepto
        {
            LiquidacionId = liquidacionId,
            Concepto      = concepto.Trim(),
            Importe       = importe,
            EsDeduccion   = esDeduccion
        };
    }
}
