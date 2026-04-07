using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Documentos;

/// <summary>
/// Estado de seguimiento de un documento en Mesa de Entrada.
/// Migrado desde VB6: MesaEntradaEstados.
/// </summary>
public class MesaEntradaEstado : AuditableEntity
{
    public string Codigo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool   EsFinal     { get; private set; }
    public bool   Activo      { get; private set; } = true;

    private MesaEntradaEstado() { }

    public static MesaEntradaEstado Crear(string codigo, string descripcion, bool esFinal, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var e = new MesaEntradaEstado { Codigo = codigo.Trim().ToUpperInvariant(), Descripcion = descripcion.Trim(), EsFinal = esFinal, Activo = true };
        e.SetCreated(userId);
        return e;
    }

    public void Actualizar(string descripcion, bool esFinal, long? userId) { Descripcion = descripcion.Trim(); EsFinal = esFinal; SetUpdated(userId); }
    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activo = true; SetUpdated(userId); }
}
