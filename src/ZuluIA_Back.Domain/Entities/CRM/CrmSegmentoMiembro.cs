using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

public class CrmSegmentoMiembro : AuditableEntity
{
    public long SegmentoId { get; private set; }
    public long ClienteId { get; private set; }
    public bool Activo { get; private set; } = true;

    private CrmSegmentoMiembro() { }

    /// <summary>
    /// Registra un cliente como miembro manual de un segmento CRM estático.
    /// </summary>
    public static CrmSegmentoMiembro Crear(long segmentoId, long clienteId, long? userId)
    {
        if (segmentoId <= 0)
            throw new ArgumentException("El miembro de segmento CRM requiere un segmento válido.", nameof(segmentoId));
        if (clienteId <= 0)
            throw new ArgumentException("El miembro de segmento CRM requiere un cliente válido.", nameof(clienteId));

        var entity = new CrmSegmentoMiembro
        {
            SegmentoId = segmentoId,
            ClienteId = clienteId,
            Activo = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    /// <summary>
    /// Reactiva una membresía manual previamente dada de baja.
    /// </summary>
    public void Reactivar(long? userId)
    {
        Activo = true;
        SetDeletedAt(null);
        SetUpdated(userId);
    }

    /// <summary>
    /// Da de baja la membresía manual del cliente dentro del segmento.
    /// </summary>
    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }
}
