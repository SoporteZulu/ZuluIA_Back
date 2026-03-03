using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

public class CondicionIva : BaseEntity
{
    public short Codigo { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;

    private CondicionIva() { }
}