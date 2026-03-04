using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Items;

public class CategoriaItem : AuditableEntity
{
    public long? ParentId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public short Nivel { get; private set; } = 1;
    public string? OrdenNivel { get; private set; }
    public bool Activo { get; private set; } = true;

    private readonly List<CategoriaItem> _hijos = [];
    public IReadOnlyCollection<CategoriaItem> Hijos => _hijos.AsReadOnly();

    private CategoriaItem() { }

    public static CategoriaItem Crear(
        long? parentId,
        string codigo,
        string descripcion,
        short nivel,
        string? ordenNivel,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        var cat = new CategoriaItem
        {
            ParentId    = parentId,
            Codigo      = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            Nivel       = nivel,
            OrdenNivel  = ordenNivel?.Trim(),
            Activo      = true
        };

        cat.SetCreated(userId);
        return cat;
    }

    public void Actualizar(
        string codigo,
        string descripcion,
        string? ordenNivel,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        Codigo      = codigo.Trim().ToUpperInvariant();
        Descripcion = descripcion.Trim();
        OrdenNivel  = ordenNivel?.Trim();
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activo = true;
        SetUpdated(userId);
    }
}