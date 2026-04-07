using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class CartaPorte : AuditableEntity
{
    public long? ComprobanteId { get; private set; }
    public long? OrdenCargaId { get; private set; }
    public long? TransportistaId { get; private set; }
    public string? NroCtg { get; private set; }
    public string CuitRemitente { get; private set; } = string.Empty;
    public string CuitDestinatario { get; private set; } = string.Empty;
    public string? CuitTransportista { get; private set; }
    public DateOnly FechaEmision { get; private set; }
    public DateOnly? FechaSolicitudCtg { get; private set; }
    public int IntentosCtg { get; private set; }
    public string? UltimoErrorCtg { get; private set; }
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
            IntentosCtg       = 0,
            Estado            = EstadoCartaPorte.Pendiente,
            Observacion       = observacion?.Trim()
        };

        carta.SetCreated(userId);
        return carta;
    }

    public void AsignarOrdenCarga(long ordenCargaId, long? transportistaId, string? cuitTransportista, string? observacion, long? userId)
    {
        if (Estado is EstadoCartaPorte.Confirmada or EstadoCartaPorte.Anulada)
            throw new InvalidOperationException("La carta de porte no admite orden de carga en el estado actual.");

        OrdenCargaId = ordenCargaId;
        TransportistaId = transportistaId;
        if (!string.IsNullOrWhiteSpace(cuitTransportista))
            CuitTransportista = cuitTransportista.Trim();
        if (!string.IsNullOrWhiteSpace(observacion))
            Observacion = observacion.Trim();

        Estado = EstadoCartaPorte.OrdenCargaAsignada;
        SetUpdated(userId);
    }

    public void SolicitarCtg(DateOnly fechaSolicitud, string? observacion, long? userId)
    {
        if (Estado is not EstadoCartaPorte.OrdenCargaAsignada and not EstadoCartaPorte.CtgError)
            throw new InvalidOperationException("Solo se puede solicitar CTG con orden de carga asignada o luego de un error de CTG.");

        FechaSolicitudCtg = fechaSolicitud;
        IntentosCtg++;
        UltimoErrorCtg = null;
        if (!string.IsNullOrWhiteSpace(observacion))
            Observacion = observacion.Trim();

        Estado = EstadoCartaPorte.CtgSolicitado;
        SetUpdated(userId);
    }

    public void RegistrarErrorCtg(string mensajeError, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mensajeError);

        if (Estado is not EstadoCartaPorte.CtgSolicitado and not EstadoCartaPorte.OrdenCargaAsignada)
            throw new InvalidOperationException("Solo se puede registrar error de CTG luego de solicitar o consultar CTG.");

        UltimoErrorCtg = mensajeError.Trim();
        if (!string.IsNullOrWhiteSpace(observacion))
            Observacion = observacion.Trim();

        Estado = EstadoCartaPorte.CtgError;
        SetUpdated(userId);
    }

    /// <summary>
    /// Asigna el número de CTG obtenido de AFIP.
    /// </summary>
    public void AsignarCtg(string nroCtg, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroCtg);

        if (Estado is not EstadoCartaPorte.Pendiente and not EstadoCartaPorte.CtgSolicitado)
            throw new InvalidOperationException(
                "La carta de porte no admite asignación de CTG en el estado actual.");

        NroCtg = nroCtg.Trim();
        UltimoErrorCtg = null;
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