using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Catálogo semántico para resolver el tipo de relación entre dos personas en contactos legacy.
/// Referencia histórica del campo `trel_id` usado por `CONTACTOS` y `PER_VINCULACIONPERSONA`.
/// </summary>
public class TipoRelacionContactoCatalogo : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private TipoRelacionContactoCatalogo() { }

    public static TipoRelacionContactoCatalogo Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var entity = new TipoRelacionContactoCatalogo
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            Activo = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        Descripcion = descripcion.Trim();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activo = true;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }
}
