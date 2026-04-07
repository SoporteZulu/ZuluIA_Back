using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Catálogo legacy de estado general de persona/tercero.
/// Referencia histórica del combo `ESTADOSPERSONAS` (`eper_id`) usado por `frmCliente`.
/// </summary>
public class EstadoPersonaCatalogo : AuditableEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private EstadoPersonaCatalogo() { }

    public static EstadoPersonaCatalogo Crear(string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var entity = new EstadoPersonaCatalogo
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
