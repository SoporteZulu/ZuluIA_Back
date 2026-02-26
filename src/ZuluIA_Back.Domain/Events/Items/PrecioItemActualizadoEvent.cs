using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Items;

public sealed class PrecioItemActualizadoEvent : DomainEvent
{
    public long ItemId { get; }
    public string Codigo { get; }
    public decimal PrecioAnterior { get; }
    public decimal PrecioNuevo { get; }

    public PrecioItemActualizadoEvent(long itemId, string codigo, decimal precioAnterior, decimal precioNuevo)
    {
        ItemId         = itemId;
        Codigo         = codigo;
        PrecioAnterior = precioAnterior;
        PrecioNuevo    = precioNuevo;
    }
}