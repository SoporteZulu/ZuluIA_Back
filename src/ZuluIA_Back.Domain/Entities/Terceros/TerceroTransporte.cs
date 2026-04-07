using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

public class TerceroTransporte : AuditableEntity
{
    public long TerceroId { get; private set; }
    public long? TransportistaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Servicio { get; private set; }
    public string? Zona { get; private set; }
    public string? Frecuencia { get; private set; }
    public string? Observacion { get; private set; }
    public bool Activo { get; private set; } = true;
    public bool Principal { get; private set; }
    public int Orden { get; private set; }

    private TerceroTransporte() { }

    public static TerceroTransporte Crear(
        long terceroId,
        long? transportistaId,
        string nombre,
        string? servicio,
        string? zona,
        string? frecuencia,
        string? observacion,
        bool activo,
        bool principal,
        int orden,
        long? userId)
    {
        if (terceroId <= 0)
            throw new ArgumentException("El tercero es obligatorio.", nameof(terceroId));

        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (orden < 0)
            throw new ArgumentException("El orden del transporte no es válido.", nameof(orden));

        var transporte = new TerceroTransporte
        {
            TerceroId = terceroId,
            TransportistaId = transportistaId,
            Nombre = nombre.Trim(),
            Servicio = Normalize(servicio),
            Zona = Normalize(zona),
            Frecuencia = Normalize(frecuencia),
            Observacion = Normalize(observacion),
            Activo = activo,
            Principal = principal,
            Orden = orden
        };

        transporte.SetCreated(userId);
        return transporte;
    }

    public void Actualizar(
        long? transportistaId,
        string nombre,
        string? servicio,
        string? zona,
        string? frecuencia,
        string? observacion,
        bool activo,
        bool principal,
        int orden,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (orden < 0)
            throw new ArgumentException("El orden del transporte no es válido.", nameof(orden));

        TransportistaId = transportistaId;
        Nombre = nombre.Trim();
        Servicio = Normalize(servicio);
        Zona = Normalize(zona);
        Frecuencia = Normalize(frecuencia);
        Observacion = Normalize(observacion);
        Activo = activo;
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
}
