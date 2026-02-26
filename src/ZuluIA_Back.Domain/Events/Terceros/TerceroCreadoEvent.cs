using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Terceros;

public sealed class TerceroCreadoEvent : DomainEvent
{
    public string Legajo { get; }
    public bool EsCliente { get; }
    public bool EsProveedor { get; }

    public TerceroCreadoEvent(string legajo, bool esCliente, bool esProveedor)
    {
        Legajo      = legajo;
        EsCliente   = esCliente;
        EsProveedor = esProveedor;
    }
}