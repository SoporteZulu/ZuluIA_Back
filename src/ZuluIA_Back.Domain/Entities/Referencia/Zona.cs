using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

/// <summary>Zona geográfica/comercial. Tabla VB6: ZONAS</summary>
public class Zona : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private Zona() { }

    public static Zona Crear(string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return new Zona
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
