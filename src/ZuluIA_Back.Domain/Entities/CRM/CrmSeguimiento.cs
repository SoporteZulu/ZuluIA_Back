using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

/// <summary>
/// Registro de seguimiento CRM de un cliente (visita, llamada, gestión comercial).
/// Migrado desde VB6: CRMSEGUIMIETOS.
/// </summary>
public class CrmSeguimiento : AuditableEntity
{
    public long     SucursalId   { get; private set; }
    public long     TerceroId    { get; private set; }
    public long?    MotivoId     { get; private set; }
    public long?    InteresId    { get; private set; }
    public long?    CampanaId    { get; private set; }
    public DateOnly Fecha        { get; private set; }
    public string   Descripcion  { get; private set; } = string.Empty;
    public DateOnly? ProximaAccion { get; private set; }
    public long?    UsuarioId    { get; private set; }

    private CrmSeguimiento() { }

    public static CrmSeguimiento Crear(
        long sucursalId, long terceroId, long? motivoId, long? interesId,
        long? campanaId, DateOnly fecha, string descripcion, DateOnly? proximaAccion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var s = new CrmSeguimiento
        {
            SucursalId    = sucursalId,
            TerceroId     = terceroId,
            MotivoId      = motivoId,
            InteresId     = interesId,
            CampanaId     = campanaId,
            Fecha         = fecha,
            Descripcion   = descripcion.Trim(),
            ProximaAccion = proximaAccion,
            UsuarioId     = userId
        };
        s.SetCreated(userId);
        return s;
    }

    public void Actualizar(string descripcion, DateOnly? proximaAccion, long? userId)
    {
        Descripcion   = descripcion.Trim();
        ProximaAccion = proximaAccion;
        SetUpdated(userId);
    }
}
