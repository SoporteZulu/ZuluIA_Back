using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Finanzas;

public sealed class PagoRegistradoEvent : DomainEvent
{
    public long PagoId { get; }
    public long SucursalId { get; }
    public long TerceroId { get; }
    public decimal Total { get; }
    public long MonedaId { get; }

    public PagoRegistradoEvent(long pagoId, long sucursalId, long terceroId, decimal total, long monedaId)
    {
        PagoId     = pagoId;
        SucursalId = sucursalId;
        TerceroId  = terceroId;
        Total      = total;
        MonedaId   = monedaId;
    }
}