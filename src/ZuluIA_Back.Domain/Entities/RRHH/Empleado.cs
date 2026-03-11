using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.RRHH;

public class Empleado : BaseEntity
{
    public long TerceroId { get; private set; }
    public long SucursalId { get; private set; }
    public string Legajo { get; private set; } = string.Empty;
    public string Cargo { get; private set; } = string.Empty;
    public string? Area { get; private set; }
    public DateOnly FechaIngreso { get; private set; }
    public DateOnly? FechaEgreso { get; private set; }
    public decimal SueldoBasico { get; private set; }
    public long MonedaId { get; private set; }
    public EstadoEmpleado Estado { get; private set; } = EstadoEmpleado.Activo;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Empleado() { }

    public static Empleado Crear(
        long terceroId,
        long sucursalId,
        string legajo,
        string cargo,
        string? area,
        DateOnly fechaIngreso,
        decimal sueldoBasico,
        long monedaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(legajo);
        ArgumentException.ThrowIfNullOrWhiteSpace(cargo);

        if (sueldoBasico < 0)
            throw new InvalidOperationException(
                "El sueldo básico no puede ser negativo.");

        return new Empleado
        {
            TerceroId    = terceroId,
            SucursalId   = sucursalId,
            Legajo       = legajo.Trim().ToUpperInvariant(),
            Cargo        = cargo.Trim(),
            Area         = area?.Trim(),
            FechaIngreso = fechaIngreso,
            SueldoBasico = sueldoBasico,
            MonedaId     = monedaId,
            Estado       = EstadoEmpleado.Activo,
            CreatedAt    = DateTimeOffset.UtcNow,
            UpdatedAt    = DateTimeOffset.UtcNow
        };
    }

    public void Actualizar(
        string cargo,
        string? area,
        decimal sueldoBasico,
        long monedaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cargo);
        Cargo        = cargo.Trim();
        Area         = area?.Trim();
        SueldoBasico = sueldoBasico;
        MonedaId     = monedaId;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }

    public void Egresar(DateOnly fechaEgreso)
    {
        FechaEgreso = fechaEgreso;
        Estado      = EstadoEmpleado.Inactivo;
        UpdatedAt   = DateTimeOffset.UtcNow;
    }
}