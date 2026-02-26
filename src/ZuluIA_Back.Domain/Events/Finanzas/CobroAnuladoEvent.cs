using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Finanzas;

public sealed class CobroAnuladoEvent : DomainEvent
{
    public long CobroId { get; }
    public long SucursalId { get; }
    public long TerceroId { get; }
    public decimal Total { get; }
    public long MonedaId { get; }

    public CobroAnuladoEvent(long cobroId, long sucursalId, long terceroId, decimal total, long monedaId)
    {
        CobroId    = cobroId;
        SucursalId = sucursalId;
        TerceroId  = terceroId;
        Total      = total;
        MonedaId   = monedaId;
    }
}