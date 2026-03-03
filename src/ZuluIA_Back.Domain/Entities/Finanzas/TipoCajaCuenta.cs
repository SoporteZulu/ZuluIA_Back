using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class TipoCajaCuenta : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool EsCaja { get; private set; } = true;

    private TipoCajaCuenta() { }
}