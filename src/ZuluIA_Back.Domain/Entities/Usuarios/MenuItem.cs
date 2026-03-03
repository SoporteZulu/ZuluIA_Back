using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Usuarios;

public class MenuItem : BaseEntity
{
    public long? ParentId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public string? Formulario { get; private set; }
    public string? Icono { get; private set; }
    public short Nivel { get; private set; } = 1;
    public short Orden { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<MenuItem> _hijos = [];
    public IReadOnlyCollection<MenuItem> Hijos => _hijos.AsReadOnly();

    private MenuItem() { }

    public static MenuItem Crear(
        long? parentId,
        string descripcion,
        string? formulario,
        string? icono,
        short nivel,
        short orden)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new MenuItem
        {
            ParentId    = parentId,
            Descripcion = descripcion.Trim(),
            Formulario  = formulario?.Trim(),
            Icono       = icono?.Trim(),
            Nivel       = nivel,
            Orden       = orden,
            Activo      = true,
            CreatedAt   = DateTimeOffset.UtcNow
        };
    }

    public void Actualizar(
        string descripcion,
        string? formulario,
        string? icono,
        short orden)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
        Formulario  = formulario?.Trim();
        Icono       = icono?.Trim();
        Orden       = orden;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}