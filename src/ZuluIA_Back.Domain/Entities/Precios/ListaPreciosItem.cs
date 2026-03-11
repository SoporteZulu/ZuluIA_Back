using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Precios;

public class ListaPreciosItem : BaseEntity
{
    public long ListaId { get; private set; }
    public long ItemId { get; private set; }
    public decimal Precio { get; private set; }
    public decimal DescuentoPct { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ListaPrecios? Lista { get; private set; }
    private ListaPreciosItem() { }

    internal static ListaPreciosItem Crear(
        long listaId,
        long itemId,
        decimal precio,
        decimal descuentoPct)
    {
        return new ListaPreciosItem
        {
            ListaId      = listaId,
            ItemId       = itemId,
            Precio       = precio,
            DescuentoPct = descuentoPct,
            UpdatedAt    = DateTimeOffset.UtcNow
        };
    }

    internal void ActualizarPrecio(decimal precio, decimal descuentoPct)
    {
        Precio       = precio;
        DescuentoPct = descuentoPct;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Calcula el precio final aplicando el descuento.
    /// </summary>
    public decimal PrecioFinal =>
        Math.Round(Precio * (1 - DescuentoPct / 100), 4);
}