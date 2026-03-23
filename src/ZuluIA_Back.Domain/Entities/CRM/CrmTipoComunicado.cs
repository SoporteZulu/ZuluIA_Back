using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

/// <summary>
/// Tipo de comunicado CRM (mail, llamada, visita, etc.).
/// Migrado desde VB6: CRMTIPOCOMUNICADOS.
/// </summary>
public class CrmTipoComunicado : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   Activo      { get; private set; } = true;

    private CrmTipoComunicado() { }

    public static CrmTipoComunicado Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var t = new CrmTipoComunicado { Codigo = codigo.Trim().ToUpperInvariant(), Descripcion = descripcion.Trim(), Activo = true };
        t.SetCreated(userId);
        return t;
    }

    public void Actualizar(string descripcion, long? userId) { Descripcion = descripcion.Trim(); SetUpdated(userId); }
    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activo = true; SetUpdated(userId); }
}
