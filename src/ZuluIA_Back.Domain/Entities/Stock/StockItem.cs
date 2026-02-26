using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Events.Stock;

namespace ZuluIA_Back.Domain.Entities.Stock;

public class StockItem : BaseEntity
{
    public long ItemId { get; private set; }
    public long DepositoId { get; private set; }
    public decimal Cantidad { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private StockItem() { }

    public static StockItem Crear(long itemId, long depositoId, decimal cantidadInicial = 0)
    {
        return new StockItem
        {
            ItemId     = itemId,
            DepositoId = depositoId,
            Cantidad   = cantidadInicial,
            UpdatedAt  = DateTimeOffset.UtcNow
        };
    }

    public void AjustarStock(decimal nuevaCantidad, string motivo)
    {
        var anterior = Cantidad;
        Cantidad  = nuevaCantidad;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new StockAjustadoEvent(ItemId, DepositoId, anterior, nuevaCantidad, motivo));
    }

    public void Incrementar(decimal cantidad)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad a incrementar debe ser mayor a 0.");

        Cantidad  += cantidad;
        UpdatedAt  = DateTimeOffset.UtcNow;
    }

    public void Decrementar(decimal cantidad, bool permitirNegativo = false)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad a decrementar debe ser mayor a 0.");

        if (!permitirNegativo && Cantidad < cantidad)
            throw new InvalidOperationException($"Stock insuficiente. Disponible: {Cantidad}, Requerido: {cantidad}.");

        Cantidad  -= cantidad;
        UpdatedAt  = DateTimeOffset.UtcNow;
    }
}