using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

public class Moneda : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Simbolo { get; private set; } = "$";
    public bool SinDecimales { get; private set; }
    public bool Activa { get; private set; } = true;

    private Moneda() { }
}