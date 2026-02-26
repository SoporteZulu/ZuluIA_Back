using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Items;

public sealed class ItemCreadoEvent : DomainEvent
{
    public string Codigo { get; }
    public string Descripcion { get; }

    public ItemCreadoEvent(string codigo, string descripcion)
    {
        Codigo      = codigo;
        Descripcion = descripcion;
    }
}