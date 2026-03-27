using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Items;

/// <summary>
/// Valor de un atributo para un ítem específico.
/// Migrado desde VB6: clsAtributosItems / ATRIBUTOSITEMS.
/// </summary>
public class AtributoItem : BaseEntity
{
    public long ItemId { get; private set; }
    public long AtributoId { get; private set; }
    public string Valor { get; private set; } = string.Empty;

    private AtributoItem() { }

    public static AtributoItem Crear(long itemId, long atributoId, string valor)
    {
        if (itemId <= 0)   throw new ArgumentException("El ítem es requerido.");
        if (atributoId <= 0) throw new ArgumentException("El atributo es requerido.");

        return new AtributoItem
        {
            ItemId      = itemId,
            AtributoId  = atributoId,
            Valor       = valor?.Trim() ?? string.Empty
        };
    }

    public void ActualizarValor(string valor) => Valor = valor?.Trim() ?? string.Empty;
}
