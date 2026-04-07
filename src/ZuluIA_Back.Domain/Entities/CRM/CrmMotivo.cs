using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

/// <summary>
/// Catálogo de motivos CRM (motivo de visita, de llamada, de reclamo, etc.).
/// Migrado desde VB6: CRMMOTIVOS.
/// </summary>
public class CrmMotivo : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   Activo      { get; private set; } = true;

    private CrmMotivo() { }

    public static CrmMotivo Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var m = new CrmMotivo { Codigo = codigo.Trim().ToUpperInvariant(), Descripcion = descripcion.Trim(), Activo = true };
        m.SetCreated(userId);
        return m;
    }

    public void Actualizar(string descripcion, long? userId) { Descripcion = descripcion.Trim(); SetUpdated(userId); }
    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activo = true; SetUpdated(userId); }
}
