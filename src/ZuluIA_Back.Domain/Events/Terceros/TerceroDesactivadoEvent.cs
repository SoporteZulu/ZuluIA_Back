using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Terceros;

/// <summary>
/// Se dispara cuando se da de baja lógica a un tercero (soft delete).
/// Equivalente al eliminar() del VB6, que ponía activo = false.
/// Permite que otros módulos (Comprobantes, CuentaCorriente) reaccionen
/// si necesitan marcar el tercero como inactivo en sus vistas.
/// </summary>
public sealed record TerceroDesactivadoEvent(
    long TerceroId,
    string Legajo,
    string RazonSocial
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}