namespace ZuluIA_Back.Domain.Common;

public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId    = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
}
