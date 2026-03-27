using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Categoría de cliente para clasificación comercial.
/// Migrado desde VB6: CATEGORIACLIENTES.
/// </summary>
public class CategoriaCliente : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   Activa      { get; private set; } = true;

    private CategoriaCliente() { }

    public static CategoriaCliente Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var c = new CategoriaCliente
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
