using MediatR;

namespace ZuluIA_Back.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}
