using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comercial;

public class ZonaComercial : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private ZonaComercial() { }

    public static ZonaComercial Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var entity = new ZonaComercial
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

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }
}
