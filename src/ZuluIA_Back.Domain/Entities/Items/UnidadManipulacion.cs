using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Items;

/// <summary>
/// Unidad de manipulación/embalaje de un ítem (caja, pallet, etc.).
/// Migrado desde VB6: frmItems / ume_unidades_manipulacion.
/// Campos: descripcion, id_item, cantidad, id_unidad, id_tipo_unidad_manipulacion.
/// </summary>
public class UnidadManipulacion : BaseEntity
{
    public long    ItemId                     { get; private set; }
    public string  Descripcion               { get; private set; } = string.Empty;
    public decimal Cantidad                  { get; private set; }
    public long    UnidadMedidaId            { get; private set; }
    public long?   TipoUnidadManipulacionId  { get; private set; }

    private UnidadManipulacion() { }

    public static UnidadManipulacion Crear(
        long    itemId,
        string  descripcion,
        decimal cantidad,
        long    unidadMedidaId,
        long?   tipoUnidadManipulacionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");
        return new UnidadManipulacion
        {
            ItemId                    = itemId,
            Descripcion               = descripcion.Trim(),
            Cantidad                  = cantidad,
            UnidadMedidaId            = unidadMedidaId,
            TipoUnidadManipulacionId  = tipoUnidadManipulacionId
        };
    }

    public void Actualizar(string descripcion, decimal cantidad, long unidadMedidaId, long? tipoUnidadManipulacionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");
        Descripcion              = descripcion.Trim();
        Cantidad                 = cantidad;
        UnidadMedidaId           = unidadMedidaId;
        TipoUnidadManipulacionId = tipoUnidadManipulacionId;
    }
}
