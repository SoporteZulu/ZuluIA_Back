using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comercial;

public class ComprobanteItemAtributoComercial : AuditableEntity
{
    public long ComprobanteItemId { get; private set; }
    public long AtributoComercialId { get; private set; }
    public string Valor { get; private set; } = string.Empty;

    private ComprobanteItemAtributoComercial() { }

    public static ComprobanteItemAtributoComercial Crear(long comprobanteItemId, long atributoComercialId, string valor, long? userId)
    {
        if (comprobanteItemId <= 0)
            throw new InvalidOperationException("El ítem de comprobante es obligatorio.");
        if (atributoComercialId <= 0)
            throw new InvalidOperationException("El atributo comercial es obligatorio.");
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var entity = new ComprobanteItemAtributoComercial
        {
            ComprobanteItemId = comprobanteItemId,
            AtributoComercialId = atributoComercialId,
            Valor = valor.Trim()
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void ActualizarValor(string valor, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);
        Valor = valor.Trim();
        SetUpdated(userId);
    }
}
