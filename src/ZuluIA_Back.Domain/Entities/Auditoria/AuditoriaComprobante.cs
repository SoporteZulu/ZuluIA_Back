using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Auditoria;

public class AuditoriaComprobante : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public long? UsuarioId { get; private set; }
    public AccionAuditoria Accion { get; private set; }
    public DateTime FechaHora { get; private set; }
    public string? DetalleCambio { get; private set; }
    public string? IpOrigen { get; private set; }

    private AuditoriaComprobante() { }

    public static AuditoriaComprobante Registrar(
        long comprobanteId,
        long? usuarioId,
        AccionAuditoria accion,
        string? detalleCambio,
        string? ipOrigen)
    {
        return new AuditoriaComprobante
        {
            ComprobanteId = comprobanteId,
            UsuarioId     = usuarioId,
            Accion        = accion,
            FechaHora     = DateTime.UtcNow,
            DetalleCambio = detalleCambio,
            IpOrigen      = ipOrigen
        };
    }
}
