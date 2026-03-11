using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Terceros;

/// <summary>
/// Se dispara cuando se crea un nuevo tercero (cliente, proveedor o ambos).
/// Equivalente al momento en que el VB6 ejecutaba Guardar() por primera vez.
/// </summary>
public sealed record TerceroCreadoEvent(
    long TerceroId,
    string Legajo,
    string RazonSocial
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}