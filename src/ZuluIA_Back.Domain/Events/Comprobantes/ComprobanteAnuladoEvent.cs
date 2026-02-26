using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Comprobantes;

public sealed class ComprobanteAnuladoEvent : DomainEvent
{
    public long ComprobanteId { get; }
    public long SucursalId { get; }
    public long TerceroId { get; }
    public decimal Total { get; }
    public long MonedaId { get; }

    public ComprobanteAnuladoEvent(long comprobanteId, long sucursalId, long terceroId, decimal total, long monedaId)
    {
        ComprobanteId = comprobanteId;
        SucursalId    = sucursalId;
        TerceroId     = terceroId;
        Total         = total;
        MonedaId      = monedaId;
    }
}