using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Contabilidad;

public class Ejercicio : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public string MascaraCuentaContable { get; private set; } = "00.00.00.00";
    public bool Cerrado { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<EjercicioSucursal> _sucursales = [];
    public IReadOnlyCollection<EjercicioSucursal> Sucursales => _sucursales.AsReadOnly();

    private Ejercicio() { }

    public static Ejercicio Crear(
        string descripcion,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        string mascara = "00.00.00.00")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (fechaFin <= fechaInicio)
            throw new InvalidOperationException(
                "La fecha de fin debe ser posterior a la fecha de inicio.");

        return new Ejercicio
        {
            Descripcion             = descripcion.Trim(),
            FechaInicio             = fechaInicio,
            FechaFin                = fechaFin,
            MascaraCuentaContable   = mascara.Trim(),
            Cerrado                 = false,
            CreatedAt               = DateTimeOffset.UtcNow,
            UpdatedAt               = DateTimeOffset.UtcNow
        };
    }

    public void AsignarSucursal(long sucursalId, bool usaContabilidad = true)
    {
        if (_sucursales.Any(x => x.SucursalId == sucursalId))
            return;

        _sucursales.Add(EjercicioSucursal.Crear(Id, sucursalId, usaContabilidad));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cerrar()
    {
        if (Cerrado)
            throw new InvalidOperationException("El ejercicio ya está cerrado.");

        Cerrado   = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reabrir()
    {
        if (!Cerrado)
            throw new InvalidOperationException("El ejercicio no está cerrado.");

        Cerrado   = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ActualizarDescripcion(string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
        UpdatedAt   = DateTimeOffset.UtcNow;
    }

    public bool ContienesFecha(DateOnly fecha) =>
        fecha >= FechaInicio && fecha <= FechaFin;
}