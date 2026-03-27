using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

public class TerceroContacto : AuditableEntity
{
    public long TerceroId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Cargo { get; private set; }
    public string? Email { get; private set; }
    public string? Telefono { get; private set; }
    public string? Sector { get; private set; }
    public bool Principal { get; private set; }
    public int Orden { get; private set; }

    private TerceroContacto() { }

    public static TerceroContacto Crear(
        long terceroId,
        string nombre,
        string? cargo,
        string? email,
        string? telefono,
        string? sector,
        bool principal,
        int orden,
        long? userId)
    {
        if (terceroId <= 0)
            throw new ArgumentException("El tercero es obligatorio.", nameof(terceroId));

        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (orden < 0)
            throw new ArgumentException("El orden del contacto no es válido.", nameof(orden));

        var contacto = new TerceroContacto
        {
            TerceroId = terceroId,
            Nombre = nombre.Trim(),
            Cargo = Normalize(cargo),
            Email = NormalizeEmail(email),
            Telefono = Normalize(telefono),
            Sector = Normalize(sector),
            Principal = principal,
            Orden = orden
        };

        contacto.SetCreated(userId);
        return contacto;
    }

    public void Actualizar(
        string nombre,
        string? cargo,
        string? email,
        string? telefono,
        string? sector,
        bool principal,
        int orden,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (orden < 0)
            throw new ArgumentException("El orden del contacto no es válido.", nameof(orden));

        Nombre = nombre.Trim();
        Cargo = Normalize(cargo);
        Email = NormalizeEmail(email);
        Telefono = Normalize(telefono);
        Sector = Normalize(sector);
        Principal = principal;
        Orden = orden;
        SetUpdated(userId);
    }

    public void MarcarComoEliminado(long? userId)
    {
        SetDeleted();
        SetUpdated(userId);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeEmail(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
