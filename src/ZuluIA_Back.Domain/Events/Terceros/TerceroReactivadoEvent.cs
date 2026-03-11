using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Terceros;

/// <summary>
/// Se dispara cuando se reactiva un tercero que estaba dado de baja.
/// No existía en VB6 (la reactivación era manual en la BD),
/// pero es necesario para mantener consistencia con CuentaCorriente
/// y otros módulos que filtran por activo.
/// </summary>
public sealed record TerceroReactivadoEvent(
    long TerceroId,
    string Legajo,
    string RazonSocial
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}