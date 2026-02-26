using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}