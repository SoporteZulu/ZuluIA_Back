using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Items;

public sealed record PrecioItemActualizadoEvent(
    long ItemId,
    string Codigo,
    decimal PrecioAnterior,
    decimal PrecioNuevo
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
