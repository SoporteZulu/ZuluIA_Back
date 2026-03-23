using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Auditoria;

public class AuditoriaCaea : BaseEntity
{
    public long CaeaId { get; private set; }
    public long? UsuarioId { get; private set; }
    public AccionAuditoria Accion { get; private set; }
    public DateTime FechaHora { get; private set; }
    public string? DetalleCambio { get; private set; }
    public string? IpOrigen { get; private set; }

    private AuditoriaCaea() { }

    public static AuditoriaCaea Registrar(
        long caeaId,
        long? usuarioId,
        AccionAuditoria accion,
        string? detalleCambio,
        string? ipOrigen)
    {
        return new AuditoriaCaea
        {
            CaeaId = caeaId,
            UsuarioId = usuarioId,
            Accion = accion,
            FechaHora = DateTime.UtcNow,
            DetalleCambio = detalleCambio,
            IpOrigen = ipOrigen
        };
    }
}