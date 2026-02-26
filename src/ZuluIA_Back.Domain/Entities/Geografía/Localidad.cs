using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Geografia;

public class Localidad : AuditableEntity
{
    public long ProvinciaId { get; private set; }
    public string? CodigoPostal { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;

    public Provincia? Provincia { get; private set; }

    private Localidad() { }

    public static Localidad Crear(long provinciaId, string descripcion, string? codigoPostal = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Localidad
        {
            ProvinciaId  = provinciaId,
            Descripcion  = descripcion.Trim(),
            CodigoPostal = codigoPostal?.Trim()
        };
    }
}