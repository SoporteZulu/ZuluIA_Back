using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Categoría de proveedor para clasificación comercial.
/// Migrado desde VB6: CATEGORIAPROVEEDORES.
/// </summary>
public class CategoriaProveedor : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   Activa      { get; private set; } = true;

    private CategoriaProveedor() { }

    public static CategoriaProveedor Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var c = new CategoriaProveedor
        {
            Codigo      = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            Activa      = true
        };
        c.SetCreated(userId);
        return c;
    }

    public void Actualizar(string codigo, string descripcion, long? userId)
    {
        Codigo      = codigo.Trim().ToUpperInvariant();
        Descripcion = descripcion.Trim();
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activa = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activa = true; SetUpdated(userId); }
}
