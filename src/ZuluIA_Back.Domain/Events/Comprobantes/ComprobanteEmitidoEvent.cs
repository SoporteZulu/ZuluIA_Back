using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Comprobantes;

public sealed record ComprobanteEmitidoEvent(
    long ComprobanteId,
    long SucursalId,
    long TerceroId,
    decimal Total,
    long MonedaId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
