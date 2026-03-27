using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Franquicias;

/// <summary>
/// Grupo económico al que puede pertenecer una o más franquicias.
/// Migrado desde VB6: FRA_GRUPOSECONOMICOS.
/// </summary>
public class GrupoEconomico : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   Activo      { get; private set; } = true;

    private GrupoEconomico() { }

    public static GrupoEconomico Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var g = new GrupoEconomico { Codigo = codigo.Trim().ToUpperInvariant(), Descripcion = descripcion.Trim(), Activo = true };
        g.SetCreated(userId);
        return g;
    }

    public void Actualizar(string descripcion, long? userId) { Descripcion = descripcion.Trim(); SetUpdated(userId); }
    public void Activar(long? userId) { Activo = true; SetUpdated(userId); }
    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
}
