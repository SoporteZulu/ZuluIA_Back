using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Integraciones;

public class IntegracionExternaAudit : AuditableEntity
{
    public ProveedorIntegracionExterna Proveedor { get; private set; }
    public string Operacion { get; private set; } = string.Empty;
    public string? ReferenciaTipo { get; private set; }
    public long? ReferenciaId { get; private set; }
    public bool Exitoso { get; private set; }
    public int Reintentos { get; private set; }
    public int TimeoutMs { get; private set; }
    public bool CircuitBreakerAbierto { get; private set; }
    public long DuracionMs { get; private set; }
    public string Ambiente { get; private set; } = string.Empty;
    public string Endpoint { get; private set; } = string.Empty;
    public string? CodigoError { get; private set; }
    public bool ErrorFuncional { get; private set; }
    public string RequestPayload { get; private set; } = string.Empty;
    public string ResponsePayload { get; private set; } = string.Empty;
    public string? MensajeError { get; private set; }

    private IntegracionExternaAudit() { }

    public static IntegracionExternaAudit Registrar(
        ProveedorIntegracionExterna proveedor,
        string operacion,
        string? referenciaTipo,
        long? referenciaId,
        bool exitoso,
        int reintentos,
        int timeoutMs,
        bool circuitBreakerAbierto,
        long duracionMs,
        string ambiente,
        string endpoint,
        string? codigoError,
        bool errorFuncional,
        string requestPayload,
        string responsePayload,
        string? mensajeError,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operacion);
        ArgumentException.ThrowIfNullOrWhiteSpace(ambiente);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        var audit = new IntegracionExternaAudit
        {
            Proveedor = proveedor,
            Operacion = operacion.Trim().ToUpperInvariant(),
            ReferenciaTipo = referenciaTipo?.Trim().ToUpperInvariant(),
            ReferenciaId = referenciaId,
            Exitoso = exitoso,
            Reintentos = reintentos,
            TimeoutMs = timeoutMs,
            CircuitBreakerAbierto = circuitBreakerAbierto,
            DuracionMs = duracionMs,
            Ambiente = ambiente.Trim().ToUpperInvariant(),
            Endpoint = endpoint.Trim(),
            CodigoError = codigoError?.Trim().ToUpperInvariant(),
            ErrorFuncional = errorFuncional,
            RequestPayload = requestPayload?.Trim() ?? string.Empty,
            ResponsePayload = responsePayload?.Trim() ?? string.Empty,
            MensajeError = mensajeError?.Trim()
        };

        audit.SetCreated(userId);
        return audit;
    }
}
