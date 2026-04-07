using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Fiscal;

public class SalidaRegulatoria : AuditableEntity
{
    public TipoSalidaRegulatoria Tipo { get; private set; }
    public long SucursalId { get; private set; }
    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public EstadoSalidaRegulatoria Estado { get; private set; }
    public string NombreArchivo { get; private set; } = string.Empty;
    public string Contenido { get; private set; } = string.Empty;

    private SalidaRegulatoria() { }

    public static SalidaRegulatoria Crear(TipoSalidaRegulatoria tipo, long sucursalId, DateOnly desde, DateOnly hasta, string nombreArchivo, string contenido, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreArchivo);
        ArgumentException.ThrowIfNullOrWhiteSpace(contenido);

        var salida = new SalidaRegulatoria
        {
            Tipo = tipo,
            SucursalId = sucursalId,
            Desde = desde,
            Hasta = hasta,
            Estado = EstadoSalidaRegulatoria.Generada,
            NombreArchivo = nombreArchivo.Trim(),
            Contenido = contenido
        };

        salida.SetCreated(userId);
        return salida;
    }

    public void MarcarPresentada(long? userId)
    {
        Estado = EstadoSalidaRegulatoria.Presentada;
        SetUpdated(userId);
    }
}
