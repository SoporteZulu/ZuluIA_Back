using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comercial;

public class MaestroAuxiliarComercial : AuditableEntity
{
    public string Grupo { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private MaestroAuxiliarComercial() { }

    public static MaestroAuxiliarComercial Crear(string grupo, string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(grupo);
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var entity = new MaestroAuxiliarComercial
        {
            Grupo = grupo.Trim().ToUpperInvariant(),
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
