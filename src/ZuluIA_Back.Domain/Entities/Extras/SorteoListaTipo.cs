using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Extras;

/// <summary>
/// Tipo de sorteo/rifa para clasificación.
/// Migrado desde VB6: SorteoListaTipos.
/// </summary>
public class SorteoListaTipo : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   Activo      { get; private set; } = true;

    private SorteoListaTipo() { }

    public static SorteoListaTipo Crear(string codigo, string descripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var t = new SorteoListaTipo { Codigo = codigo.Trim().ToUpperInvariant(), Descripcion = descripcion.Trim(), Activo = true };
        t.SetCreated(userId);
        return t;
    }

    public void Actualizar(string descripcion, long? userId) { Descripcion = descripcion.Trim(); SetUpdated(userId); }
    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activo = true; SetUpdated(userId); }
}
