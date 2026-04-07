using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Logistica;

public class OrdenCarga : AuditableEntity
{
    public long CartaPorteId { get; private set; }
    public long? TransportistaId { get; private set; }
    public DateOnly FechaCarga { get; private set; }
    public string Origen { get; private set; } = string.Empty;
    public string Destino { get; private set; } = string.Empty;
    public string? Patente { get; private set; }
    public bool Confirmada { get; private set; }
    public string? Observacion { get; private set; }

    private OrdenCarga() { }

    public static OrdenCarga Crear(
        long cartaPorteId,
        long? transportistaId,
        DateOnly fechaCarga,
        string origen,
        string destino,
        string? patente,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origen);
        ArgumentException.ThrowIfNullOrWhiteSpace(destino);

        var orden = new OrdenCarga
        {
            CartaPorteId = cartaPorteId,
            TransportistaId = transportistaId,
            FechaCarga = fechaCarga,
            Origen = origen.Trim(),
            Destino = destino.Trim(),
            Patente = patente?.Trim().ToUpperInvariant(),
            Observacion = observacion?.Trim(),
            Confirmada = false
        };

        orden.SetCreated(userId);
        return orden;
    }

    public void Confirmar(long? userId)
    {
        Confirmada = true;
        SetUpdated(userId);
    }
}
