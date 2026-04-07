using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Items;

/// <summary>
/// Definición de un atributo (característica) que puede asignarse a ítems.
/// Migrado desde VB6: clsAtributos / ATRIBUTOS.
/// </summary>
public class Atributo : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    /// <summary>Tipo de valor: "texto", "numero", "fecha", "boolean".</summary>
    public string Tipo { get; private set; } = "texto";
    public bool Requerido { get; private set; }
    public bool Activo { get; private set; } = true;

    private Atributo() { }

    public static Atributo Crear(string descripcion, string tipo = "texto", bool requerido = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return new Atributo
        {
            Descripcion = descripcion.Trim(),
            Tipo        = tipo.Trim().ToLowerInvariant(),
            Requerido   = requerido,
            Activo      = true
        };
    }

    public void Actualizar(string descripcion, string tipo, bool requerido)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion = descripcion.Trim();
        Tipo        = tipo.Trim().ToLowerInvariant();
        Requerido   = requerido;
    }

    public void Activar()    => Activo = true;
    public void Desactivar() => Activo = false;
}
