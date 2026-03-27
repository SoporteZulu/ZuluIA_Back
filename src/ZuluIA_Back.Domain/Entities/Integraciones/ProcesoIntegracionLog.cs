using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Integraciones;

public class ProcesoIntegracionLog : AuditableEntity
{
    public long JobId { get; private set; }
    public NivelLogIntegracion Nivel { get; private set; }
    public string Mensaje { get; private set; } = string.Empty;
    public string? Referencia { get; private set; }
    public string? Payload { get; private set; }

    private ProcesoIntegracionLog() { }

    public static ProcesoIntegracionLog Registrar(long jobId, NivelLogIntegracion nivel, string mensaje, string? referencia, string? payload, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mensaje);

        var log = new ProcesoIntegracionLog
        {
            JobId = jobId,
            Nivel = nivel,
            Mensaje = mensaje.Trim(),
            Referencia = referencia?.Trim(),
            Payload = payload?.Trim()
        };

        log.SetCreated(userId);
        return log;
    }
}
