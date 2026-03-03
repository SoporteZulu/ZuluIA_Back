using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

public class FormaPago : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activa { get; private set; } = true;

    private FormaPago() { }
}