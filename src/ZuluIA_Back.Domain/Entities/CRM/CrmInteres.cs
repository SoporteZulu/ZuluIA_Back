using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

/// <summary>
/// Catálogo de intereses del cliente (productos, servicios, líneas de interés).
/// Migrado desde VB6: CRMINTERES.
/// </summary>
public class CrmInteres : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   Activo      { get; private set; } = true;

    private CrmInteres() { }

    public static CrmInteres Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var i = new CrmInteres { Codigo = codigo.Trim().ToUpperInvariant(), Descripcion = descripcion.Trim(), Activo = true };
        i.SetCreated(userId);
        return i;
    }

    public void Actualizar(string descripcion, long? userId) { Descripcion = descripcion.Trim(); SetUpdated(userId); }
    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activo = true; SetUpdated(userId); }
}
