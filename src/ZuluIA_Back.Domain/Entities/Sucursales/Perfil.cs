using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Sucursales;

/// <summary>
/// Perfil de acceso/rol asignado a un empleado dentro de un área.
/// Referencia: tabla SUC_PERFIL del VB6 (clsEmpleadoXPerfil.mTablaPerfil).
/// </summary>
public class Perfil : BaseEntity
{
    public string  Codigo      { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }

    private Perfil() { }

    public static Perfil Crear(string codigo, string? descripcion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        return new Perfil
        {
            Codigo      = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion?.Trim()
        };
    }

    public void Actualizar(string codigo, string? descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        Codigo      = codigo.Trim().ToUpperInvariant();
        Descripcion = descripcion?.Trim();
    }
}
