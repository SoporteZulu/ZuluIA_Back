using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Stock;

public sealed record StockAjustadoEvent(
    long ItemId,
    long DepositoId,
    decimal Anterior,
    decimal Nuevo,
    string Motivo
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
