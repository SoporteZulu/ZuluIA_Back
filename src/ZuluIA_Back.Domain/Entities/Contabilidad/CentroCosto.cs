using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Contabilidad;

public class CentroCosto : BaseEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }

    private CentroCosto() { }

    public static CentroCosto Crear(string codigo, string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new CentroCosto
        {
            Codigo      = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            Activo      = true,
            CreatedAt   = DateTimeOffset.UtcNow
        };
    }

    public void Actualizar(string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}