using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Integraciones;

public class ProcesoIntegracionJob : AuditableEntity
{
    public TipoProcesoIntegracion Tipo { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? ClaveIdempotencia { get; private set; }
    public EstadoProcesoIntegracion Estado { get; private set; }
    public DateTimeOffset FechaInicio { get; private set; }
    public DateTimeOffset? FechaFin { get; private set; }
    public int TotalRegistros { get; private set; }
    public int RegistrosProcesados { get; private set; }
    public int RegistrosExitosos { get; private set; }
    public int RegistrosConError { get; private set; }
    public string? PayloadResumen { get; private set; }
    public string? Observacion { get; private set; }

    private ProcesoIntegracionJob() { }

    public static ProcesoIntegracionJob Crear(TipoProcesoIntegracion tipo, string nombre, int totalRegistros, string? payloadResumen, long? userId, string? claveIdempotencia = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (totalRegistros < 0)
            throw new InvalidOperationException("El total de registros no puede ser negativo.");

        var job = new ProcesoIntegracionJob
        {
            Tipo = tipo,
            Nombre = nombre.Trim(),
            ClaveIdempotencia = NormalizarClaveIdempotencia(claveIdempotencia),
            Estado = EstadoProcesoIntegracion.Pendiente,
            FechaInicio = DateTimeOffset.UtcNow,
            TotalRegistros = totalRegistros,
            PayloadResumen = payloadResumen?.Trim()
        };

        job.SetCreated(userId);
        return job;
    }

    public void Iniciar(long? userId)
    {
        Estado = EstadoProcesoIntegracion.EnProceso;
        FechaInicio = DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }

    public void RegistrarExito(long? userId)
    {
        RegistrosProcesados++;
        RegistrosExitosos++;
        SetUpdated(userId);
    }

    public void RegistrarError(string? observacion, long? userId)
    {
        RegistrosProcesados++;
        RegistrosConError++;
        if (!string.IsNullOrWhiteSpace(observacion))
            Observacion = observacion.Trim();
        SetUpdated(userId);
    }

    public void Finalizar(string? observacion, long? userId)
    {
        FechaFin = DateTimeOffset.UtcNow;
        Estado = RegistrosConError > 0
            ? EstadoProcesoIntegracion.FinalizadoConErrores
            : EstadoProcesoIntegracion.Finalizado;
        if (!string.IsNullOrWhiteSpace(observacion))
            Observacion = observacion.Trim();
        SetUpdated(userId);
    }

    public void Fallar(string observacion, long? userId)
    {
        FechaFin = DateTimeOffset.UtcNow;
        Estado = EstadoProcesoIntegracion.Fallido;
        Observacion = observacion.Trim();
        SetUpdated(userId);
    }

    private static string? NormalizarClaveIdempotencia(string? claveIdempotencia)
    {
        if (string.IsNullOrWhiteSpace(claveIdempotencia))
            return null;

        return claveIdempotencia.Trim().ToUpperInvariant();
    }
}
