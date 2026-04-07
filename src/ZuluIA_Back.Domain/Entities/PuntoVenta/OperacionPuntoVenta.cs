using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.PuntoVenta;

public class OperacionPuntoVenta : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public long SucursalId { get; private set; }
    public long PuntoFacturacionId { get; private set; }
    public CanalOperacionPuntoVenta Canal { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string? ReferenciaExterna { get; private set; }
    public string? Observacion { get; private set; }

    private OperacionPuntoVenta() { }

    public static OperacionPuntoVenta Registrar(long comprobanteId, long sucursalId, long puntoFacturacionId, CanalOperacionPuntoVenta canal, DateOnly fecha, string? referenciaExterna, string? observacion, long? userId)
    {
        var operacion = new OperacionPuntoVenta
        {
            ComprobanteId = comprobanteId,
            SucursalId = sucursalId,
            PuntoFacturacionId = puntoFacturacionId,
            Canal = canal,
            Fecha = fecha,
            ReferenciaExterna = referenciaExterna?.Trim(),
            Observacion = observacion?.Trim()
        };

        operacion.SetCreated(userId);
        return operacion;
    }
}
