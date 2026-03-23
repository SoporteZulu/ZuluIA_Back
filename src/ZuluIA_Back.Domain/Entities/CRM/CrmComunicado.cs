using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

/// <summary>
/// Comunicado enviado o recibido en el módulo CRM (mail, llamada, visita, etc.).
/// Migrado desde VB6: CRMCOMUNICADOS.
/// </summary>
public class CrmComunicado : AuditableEntity
{
    public long     SucursalId   { get; private set; }
    public long     TerceroId    { get; private set; }
    public long?    CampanaId    { get; private set; }
    public long?    TipoId       { get; private set; }
    public DateOnly Fecha        { get; private set; }
    public string   Asunto       { get; private set; } = string.Empty;
    public string?  Contenido    { get; private set; }
    public long?    UsuarioId    { get; private set; }

    private CrmComunicado() { }

    public static CrmComunicado Crear(
        long sucursalId, long terceroId, long? campanaId, long? tipoId,
        DateOnly fecha, string asunto, string? contenido, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(asunto);
        var c = new CrmComunicado
        {
            SucursalId = sucursalId,
            TerceroId  = terceroId,
            CampanaId  = campanaId,
            TipoId     = tipoId,
            Fecha      = fecha,
            Asunto     = asunto.Trim(),
            Contenido  = contenido?.Trim(),
            UsuarioId  = userId
        };
        c.SetCreated(userId);
        return c;
    }

    public void Actualizar(string asunto, string? contenido, long? userId)
    {
        Asunto    = asunto.Trim();
        Contenido = contenido?.Trim();
        SetUpdated(userId);
    }
}
