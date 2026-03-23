using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Estado operativo de un cliente (activo, bloqueado, inhabilitado, etc.).
/// Migrado desde VB6: ESTADOSCLIENTES.
/// </summary>
public class EstadoCliente : AuditableEntity
{
    public string Codigo        { get; private set; } = string.Empty;
    public string Descripcion   { get; private set; } = string.Empty;
    public bool   Bloquea       { get; private set; }
    public bool   Activo        { get; private set; } = true;

    private EstadoCliente() { }

    public static EstadoCliente Crear(string codigo, string descripcion, bool bloquea, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        var e = new EstadoCliente
        {
            Codigo      = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            Bloquea     = bloquea,
            Activo      = true
        };
        e.SetCreated(userId);
        return e;
    }

    public void Actualizar(string descripcion, bool bloquea, long? userId)
    {
        Descripcion = descripcion.Trim();
        Bloquea     = bloquea;
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
    public void Activar(long? userId) { Activo = true; SetUpdated(userId); }
}
