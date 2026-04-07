using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comercial;

public class ItemAtributoComercial : AuditableEntity
{
    public long ItemId { get; private set; }
    public long AtributoComercialId { get; private set; }
    public string Valor { get; private set; } = string.Empty;

    private ItemAtributoComercial() { }

    public static ItemAtributoComercial Crear(long itemId, long atributoComercialId, string valor, long? userId)
    {
        if (itemId <= 0)
            throw new InvalidOperationException("El ítem es obligatorio.");
        if (atributoComercialId <= 0)
            throw new InvalidOperationException("El atributo comercial es obligatorio.");
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var entity = new ItemAtributoComercial
        {
            ItemId = itemId,
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
