using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Geografia;

public class Pais : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;

    private Pais() { }

    public static Pais Crear(string codigo, string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Pais
        {
            Codigo      = codigo.ToUpperInvariant().Trim(),
            Descripcion = descripcion.Trim()
        };
    }
}