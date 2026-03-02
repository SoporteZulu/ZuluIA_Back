using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Events.Terceros;

/// <summary>
/// Se dispara cuando cambian los roles de un tercero
/// (ej: un proveedor pasa a ser también cliente).
/// Permite que el módulo de Empleados reaccione si EsEmpleado cambia.
/// </summary>
public sealed record TerceroRolesActualizadosEvent(
    long TerceroId,
    string Legajo,
    bool EsCliente,
    bool EsProveedor,
    bool EsEmpleado
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}