using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

public class TerceroSucursalEntrega : AuditableEntity
{
    public long TerceroId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public string? Direccion { get; private set; }
    public string? Localidad { get; private set; }
    public string? Responsable { get; private set; }
    public string? Telefono { get; private set; }
    public string? Horario { get; private set; }
    public bool Principal { get; private set; }
    public int Orden { get; private set; }

    private TerceroSucursalEntrega() { }

    public static TerceroSucursalEntrega Crear(
        long terceroId,
        string descripcion,
        string? direccion,
        string? localidad,
        string? responsable,
        string? telefono,
        string? horario,
        bool principal,
        int orden,
        long? userId)
    {
        if (terceroId <= 0)
            throw new ArgumentException("El tercero es obligatorio.", nameof(terceroId));

        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (orden < 0)
            throw new ArgumentException("El orden de la sucursal/punto de entrega no es válido.", nameof(orden));

        var sucursal = new TerceroSucursalEntrega
        {
            TerceroId = terceroId,
            Descripcion = descripcion.Trim(),
            Direccion = Normalize(direccion),
            Localidad = Normalize(localidad),
            Responsable = Normalize(responsable),
            Telefono = Normalize(telefono),
            Horario = Normalize(horario),
            Principal = principal,
            Orden = orden
        };

        sucursal.SetCreated(userId);
        return sucursal;
    }

    public void Actualizar(
        string descripcion,
        string? direccion,
        string? localidad,
        string? responsable,
        string? telefono,
        string? horario,
        bool principal,
        int orden,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (orden < 0)
            throw new ArgumentException("El orden de la sucursal/punto de entrega no es válido.", nameof(orden));

        Descripcion = descripcion.Trim();
        Direccion = Normalize(direccion);
        Localidad = Normalize(localidad);
        Responsable = Normalize(responsable);
        Telefono = Normalize(telefono);
        Horario = Normalize(horario);
        Principal = principal;
        Orden = orden;
        SetUpdated(userId);
    }

    public void MarcarComoEliminada(long? userId)
    {
        SetDeleted();
        SetUpdated(userId);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
