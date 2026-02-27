using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Items;

public sealed record ItemCreadoEvent(
    string Codigo,
    string Descripcion
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
