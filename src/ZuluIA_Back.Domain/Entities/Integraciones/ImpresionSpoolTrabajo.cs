using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Integraciones;

public class ImpresionSpoolTrabajo : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public string TipoTrabajo { get; private set; } = string.Empty;
    public string Destino { get; private set; } = string.Empty;
    public EstadoImpresionSpool Estado { get; private set; }
    public int Intentos { get; private set; }
    public DateTimeOffset? ProximoIntento { get; private set; }
    public string? PayloadGenerado { get; private set; }
    public string? MensajeError { get; private set; }

    private ImpresionSpoolTrabajo() { }

    public static ImpresionSpoolTrabajo Encolar(long comprobanteId, string tipoTrabajo, string destino, long? userId)
    {
        if (comprobanteId <= 0)
            throw new InvalidOperationException("El comprobante a imprimir es obligatorio.");
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoTrabajo);
        ArgumentException.ThrowIfNullOrWhiteSpace(destino);

        var entity = new ImpresionSpoolTrabajo
        {
            ComprobanteId = comprobanteId,
            TipoTrabajo = tipoTrabajo.Trim().ToUpperInvariant(),
            Destino = destino.Trim().ToUpperInvariant(),
            Estado = EstadoImpresionSpool.Pendiente,
            Intentos = 0,
            ProximoIntento = DateTimeOffset.UtcNow
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void MarcarProcesando(long? userId)
    {
        Estado = EstadoImpresionSpool.EnProceso;
        Intentos++;
        SetUpdated(userId);
    }

    public void Completar(string payloadGenerado, long? userId)
    {
        PayloadGenerado = payloadGenerado?.Trim();
        Estado = EstadoImpresionSpool.Completado;
        ProximoIntento = null;
        MensajeError = null;
        SetUpdated(userId);
    }

    public void Fallar(string mensajeError, DateTimeOffset? proximoIntento, long? userId)
    {
        MensajeError = mensajeError.Trim();
        Estado = EstadoImpresionSpool.Error;
        ProximoIntento = proximoIntento;
        SetUpdated(userId);
    }

    public void Reencolar(DateTimeOffset proximoIntento, long? userId)
    {
        Estado = EstadoImpresionSpool.Pendiente;
        ProximoIntento = proximoIntento;
        MensajeError = null;
        SetUpdated(userId);
    }
}
