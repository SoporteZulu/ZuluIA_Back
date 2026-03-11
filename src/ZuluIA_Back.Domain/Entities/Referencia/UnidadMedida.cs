using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

public class UnidadMedida : BaseEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;

    private UnidadMedida() { }
}