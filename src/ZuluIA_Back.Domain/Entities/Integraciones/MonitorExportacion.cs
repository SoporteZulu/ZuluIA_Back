using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Integraciones;

public class MonitorExportacion : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public long? UltimoJobId { get; private set; }
    public DateTimeOffset? UltimaEjecucion { get; private set; }
    public EstadoProcesoIntegracion? UltimoEstado { get; private set; }
    public int RegistrosPendientes { get; private set; }
    public string? UltimoMensaje { get; private set; }

    private MonitorExportacion() { }

    public static MonitorExportacion Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var monitor = new MonitorExportacion
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim()
        };

        monitor.SetCreated(userId);
        return monitor;
    }

    public void Actualizar(long? ultimoJobId, EstadoProcesoIntegracion? ultimoEstado, int registrosPendientes, string? ultimoMensaje, long? userId)
    {
        if (registrosPendientes < 0)
            throw new InvalidOperationException("Los registros pendientes no pueden ser negativos.");

        UltimoJobId = ultimoJobId;
        UltimaEjecucion = DateTimeOffset.UtcNow;
        UltimoEstado = ultimoEstado;
        RegistrosPendientes = registrosPendientes;
        UltimoMensaje = ultimoMensaje?.Trim();
        SetUpdated(userId);
    }
}
