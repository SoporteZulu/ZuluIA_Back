using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Geografia;

public class Provincia : AuditableEntity
{
    public long PaisId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;

    public Pais? Pais { get; private set; }

    private Provincia() { }

    public static Provincia Crear(long paisId, string codigo, string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Provincia
        {
            PaisId      = paisId,
            Codigo      = codigo.ToUpperInvariant().Trim(),
            Descripcion = descripcion.Trim()
        };
    }
}