using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class TipoPuntoFacturacion : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool PorDefecto { get; private set; }

    private TipoPuntoFacturacion() { }
}