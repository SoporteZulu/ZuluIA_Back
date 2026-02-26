using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Geografia;

public class Barrio : AuditableEntity
{
    public long LocalidadId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;

    public Localidad? Localidad { get; private set; }

    private Barrio() { }

    public static Barrio Crear(long localidadId, string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Barrio
        {
            LocalidadId = localidadId,
            Descripcion = descripcion.Trim()
        };
    }
}