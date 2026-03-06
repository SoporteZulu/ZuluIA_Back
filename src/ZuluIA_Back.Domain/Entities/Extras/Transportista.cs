using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

public class Transportista : BaseEntity
{
    public long TerceroId { get; private set; }
    public string? NroCuitTransportista { get; private set; }
    public string? DomicilioPartida { get; private set; }
    public string? Patente { get; private set; }
    public string? MarcaVehiculo { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Transportista() { }

    public static Transportista Crear(
        long terceroId,
        string? nroCuit,
        string? domicilioPartida,
        string? patente,
        string? marcaVehiculo)
    {
        return new Transportista
        {
            TerceroId             = terceroId,
            NroCuitTransportista  = nroCuit?.Trim(),
            DomicilioPartida      = domicilioPartida?.Trim(),
            Patente               = patente?.Trim().ToUpperInvariant(),
            MarcaVehiculo         = marcaVehiculo?.Trim(),
            Activo                = true,
            CreatedAt             = DateTimeOffset.UtcNow,
            UpdatedAt             = DateTimeOffset.UtcNow
        };
    }

    public void Actualizar(
        string? domicilioPartida,
        string? patente,
        string? marcaVehiculo)
    {
        DomicilioPartida = domicilioPartida?.Trim();
        Patente          = patente?.Trim().ToUpperInvariant();
        MarcaVehiculo    = marcaVehiculo?.Trim();
        UpdatedAt        = DateTimeOffset.UtcNow;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}