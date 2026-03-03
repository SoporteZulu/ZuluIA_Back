using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class CartaPorte : AuditableEntity
{
    public long? ComprobanteId { get; private set; }
    public string? NroCtg { get; private set; }
    public string CuitRemitente { get; private set; } = string.Empty;
    public string CuitDestinatario { get; private set; } = string.Empty;
    public string? CuitTransportista { get; private set; }
    public DateOnly FechaEmision { get; private set; }
    public EstadoCartaPorte Estado { get; private set; }
    public string? Observacion { get; private set; }

    private CartaPorte() { }

    public static CartaPorte Crear(
        long? comprobanteId,
        string cuitRemitente,
        string cuitDestinatario,
        string? cuitTransportista,
        DateOnly fechaEmision,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuitRemitente);
        ArgumentException.ThrowIfNullOrWhiteSpace(cuitDestinatario);

        var carta = new CartaPorte
        {
            ComprobanteId     = comprobanteId,
            CuitRemitente     = cuitRemitente.Trim(),
            CuitDestinatario  = cuitDestinatario.Trim(),
            CuitTransportista = cuitTransportista?.Trim(),
            FechaEmision      = fechaEmision,
            Estado            = EstadoCartaPorte.Pendiente,
            Observacion       = observacion?.Trim()
        };

        carta.SetCreated(userId);
        return carta;
    }

    /// <summary>
    /// Asigna el número de CTG obtenido de AFIP.
    /// </summary>
    public void AsignarCtg(string nroCtg, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroCtg);

        if (Estado != EstadoCartaPorte.Pendiente)
            throw new InvalidOperationException(
                "Solo se puede asignar CTG a cartas de porte pendientes.");

        NroCtg = nroCtg.Trim();
        Estado = EstadoCartaPorte.Activa;
        SetUpdated(userId);
    }

    public void Confirmar(long? userId)
    {
        if (Estado != EstadoCartaPorte.Activa)
            throw new InvalidOperationException(
                "Solo se pueden confirmar cartas de porte activas.");

        Estado = EstadoCartaPorte.Confirmada;
        SetUpdated(userId);
    }

    public void Anular(string? observacion, long? userId)
    {
        if (Estado == EstadoCartaPorte.Anulada)
            throw new InvalidOperationException("La carta de porte ya está anulada.");

        Estado      = EstadoCartaPorte.Anulada;
        Observacion = observacion?.Trim();
        SetDeleted();
        SetUpdated(userId);
    }

    public void SetObservacion(string? obs) => Observacion = obs?.Trim();
    public void SetComprobanteId(long id) => ComprobanteId = id;
}