using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Integraciones;

public class BatchProgramacion : AuditableEntity
{
    public TipoProcesoIntegracion TipoProceso { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public int IntervaloMinutos { get; private set; }
    public DateTimeOffset ProximaEjecucion { get; private set; }
    public DateTimeOffset? UltimaEjecucion { get; private set; }
    public bool Activa { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public string? Observacion { get; private set; }

    private BatchProgramacion() { }

    public static BatchProgramacion Crear(TipoProcesoIntegracion tipoProceso, string nombre, int intervaloMinutos, DateTimeOffset primeraEjecucion, string payloadJson, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);
        if (intervaloMinutos <= 0)
            throw new InvalidOperationException("El intervalo del scheduler debe ser mayor a 0 minutos.");

        var entity = new BatchProgramacion
        {
            TipoProceso = tipoProceso,
            Nombre = nombre.Trim(),
            IntervaloMinutos = intervaloMinutos,
            ProximaEjecucion = primeraEjecucion,
            Activa = true,
            PayloadJson = payloadJson.Trim(),
            Observacion = observacion?.Trim()
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void MarcarEjecutada(DateTimeOffset fechaEjecucion, long? userId)
    {
        UltimaEjecucion = fechaEjecucion;
        ProximaEjecucion = fechaEjecucion.AddMinutes(IntervaloMinutos);
        SetUpdated(userId);
    }

    public void RegistrarError(DateTimeOffset fechaEjecucion, int reintentoMinutos, string? observacion, long? userId)
    {
        if (reintentoMinutos <= 0)
            throw new InvalidOperationException("El reintento del scheduler debe ser mayor a 0 minutos.");

        UltimaEjecucion = fechaEjecucion;
        ProximaEjecucion = fechaEjecucion.AddMinutes(reintentoMinutos);
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }

    public void ActualizarProgramacion(int intervaloMinutos, DateTimeOffset proximaEjecucion, string? observacion, long? userId)
    {
        if (intervaloMinutos <= 0)
            throw new InvalidOperationException("El intervalo del scheduler debe ser mayor a 0 minutos.");

        IntervaloMinutos = intervaloMinutos;
        ProximaEjecucion = proximaEjecucion;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }

    public void Reactivar(DateTimeOffset proximaEjecucion, string? observacion, long? userId)
    {
        Activa = true;
        ProximaEjecucion = proximaEjecucion;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }

    public void Desactivar(string? observacion, long? userId)
    {
        Activa = false;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }
}
