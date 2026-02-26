using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Terceros;

public sealed class TerceroDesactivadoEvent : DomainEvent
{
    public long TerceroId { get; }
    public string Legajo { get; }

    public TerceroDesactivadoEvent(long terceroId, string legajo)
    {
        TerceroId = terceroId;
        Legajo    = legajo;
    }
}