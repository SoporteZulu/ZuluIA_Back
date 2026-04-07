using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Referencia;

/// <summary>
/// Jurisdicción fiscal para Ingresos Brutos (IIBB) provinciales.
/// Tabla VB6: JURISDICCIONES
/// </summary>
public class Jurisdiccion : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private Jurisdiccion() { }

    public static Jurisdiccion Crear(string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return new Jurisdiccion
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
