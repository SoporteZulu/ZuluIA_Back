using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Infrastructure.Services;

public class DomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> events,
        CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
            await mediator.Publish(domainEvent, ct);
    }
}