using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Stock;

public sealed class StockAjustadoEvent : DomainEvent
{
    public long ItemId { get; }
    public long DepositoId { get; }
    public decimal Anterior { get; }
    public decimal Nuevo { get; }
    public string Motivo { get; }

    public StockAjustadoEvent(long itemId, long depositoId, decimal anterior, decimal nuevo, string motivo)
    {
        ItemId     = itemId;
        DepositoId = depositoId;
        Anterior   = anterior;
        Nuevo      = nuevo;
        Motivo     = motivo;
    }
}