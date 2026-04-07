using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Catálogo legacy de estado civil para personas físicas de terceros.
/// Referencia histórica del combo `EstadoCivil` usado por `frmCliente`.
/// </summary>
public class EstadoCivilCatalogo : AuditableEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private EstadoCivilCatalogo() { }

    public static EstadoCivilCatalogo Crear(string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var entity = new EstadoCivilCatalogo
        {
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
