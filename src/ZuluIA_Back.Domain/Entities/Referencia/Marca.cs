using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

/// <summary>Marca/Fabricante asociada a un ítem. Tabla VB6: MARCAS</summary>
public class Marca : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private Marca() { }

    public static Marca Crear(string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return new Marca
        {
            Descripcion = descripcion.Trim(),
            Activo      = true
        };
    }

    public void Actualizar(string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
    }

    public void Desactivar() => Activo = false;
    public void Activar()    => Activo = true;
}
