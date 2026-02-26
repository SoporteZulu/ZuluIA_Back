using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Contabilidad;

public class AsientoLinea : BaseEntity
{
    public long AsientoId { get; private set; }
    public long CuentaId { get; private set; }
    public decimal Debe { get; private set; }
    public decimal Haber { get; private set; }
    public string? Descripcion { get; private set; }
    public short Orden { get; private set; }
    public long? CentroCostoId { get; private set; }

    private AsientoLinea() { }

    public static AsientoLinea Crear(
        long asientoId,
        long cuentaId,
        decimal debe,
        decimal haber,
        string? descripcion,
        short orden,
        long? centroCostoId = null)
    {
        if (debe < 0 || haber < 0)
            throw new InvalidOperationException("Los importes de debe y haber no pueden ser negativos.");

        if (debe == 0 && haber == 0)
            throw new InvalidOperationException("Una línea de asiento debe tener debe o haber mayor a 0.");

        if (debe > 0 && haber > 0)
            throw new InvalidOperationException("Una línea no puede tener debe y haber al mismo tiempo.");

        return new AsientoLinea
        {
            AsientoId     = asientoId,
            CuentaId      = cuentaId,
            Debe          = Math.Round(debe, 2),
            Haber         = Math.Round(haber, 2),
            Descripcion   = descripcion?.Trim(),
            Orden         = orden,
            CentroCostoId = centroCostoId
        };
    }
}