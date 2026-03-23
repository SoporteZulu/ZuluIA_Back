using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Items;

/// <summary>
/// Componente de un ítem compuesto (BOM – Bill of Materials).
/// Vincula un ítem padre con un ítem componente y su cantidad.
/// Migrado desde VB6: clsItemComponente / ITEMS_COMPONENTES.
/// </summary>
public class ItemComponente : BaseEntity
{
    /// <summary>ID del ítem padre (kit/conjunto).</summary>
    public long ItemPadreId     { get; private set; }
    /// <summary>ID del ítem componente (parte).</summary>
    public long ComponenteId    { get; private set; }
    public decimal Cantidad     { get; private set; }
    public long? UnidadMedidaId { get; private set; }

    private ItemComponente() { }

    public static ItemComponente Crear(long itemPadreId, long componenteId,
        decimal cantidad, long? unidadMedidaId = null)
    {
        if (itemPadreId  <= 0) throw new ArgumentException("El ítem padre es requerido.");
        if (componenteId <= 0) throw new ArgumentException("El componente es requerido.");
        if (itemPadreId  == componenteId) throw new ArgumentException("Un ítem no puede ser su propio componente.");
        if (cantidad     <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");

        return new ItemComponente
        {
            ItemPadreId    = itemPadreId,
            ComponenteId   = componenteId,
            Cantidad       = cantidad,
            UnidadMedidaId = unidadMedidaId
        };
    }

    public void ActualizarCantidad(decimal cantidad, long? unidadMedidaId = null)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");
        Cantidad       = cantidad;
        UnidadMedidaId = unidadMedidaId;
    }
}
