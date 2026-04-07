using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Configuracion;

/// <summary>
/// Aspecto: agrupador jerárquico del sistema de Variables dinámicas.
/// Un Aspecto puede tener variables asociadas que definen campos opcionales en formularios.
/// Migrado desde VB6: clsAspectos / FRA_ASPECTOS.
/// </summary>
public class Aspecto : BaseEntity
{
    public string  Codigo          { get; private set; } = string.Empty;
    public string  Descripcion     { get; private set; } = string.Empty;
    public string? CodigoEstructura{ get; private set; }
    public int     Orden           { get; private set; }
    public int     Nivel           { get; private set; }
    public long?   AspectoPadreId  { get; private set; }
    public string? Observacion     { get; private set; }

    private Aspecto() { }

    public static Aspecto Crear(string codigo, string descripcion,
        long? aspectoPadreId = null, int orden = 0, int nivel = 0,
        string? codigoEstructura = null, string? observacion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        return new Aspecto
        {
            Codigo           = codigo.Trim().ToUpperInvariant(),
            Descripcion      = descripcion.Trim(),
            AspectoPadreId   = aspectoPadreId,
            Orden            = orden,
            Nivel            = nivel,
            CodigoEstructura = codigoEstructura?.Trim(),
            Observacion      = observacion?.Trim()
        };
    }

    public void Actualizar(string descripcion, long? aspectoPadreId,
        int orden, int nivel, string? codigoEstructura, string? observacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        Descripcion      = descripcion.Trim();
        AspectoPadreId   = aspectoPadreId;
        Orden            = orden;
        Nivel            = nivel;
        CodigoEstructura = codigoEstructura?.Trim();
        Observacion      = observacion?.Trim();
    }
}
