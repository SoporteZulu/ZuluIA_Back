using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Stock;

/// <summary>
/// Auditoría/conteo físico de inventario (toma de inventario).
/// Registra aperturas y cierres de inventario para conciliación.
/// Migrado desde VB6: clsInventario / Inventario.
/// </summary>
public class InventarioConteo : BaseEntity
{
    public long           UsuarioId      { get; private set; }
    public DateTimeOffset FechaApertura  { get; private set; }
    public DateTimeOffset? FechaCierre   { get; private set; }
    public DateTimeOffset FechaAlta      { get; private set; }
    public int            NroAuditoria   { get; private set; }

    private InventarioConteo() { }

    public static InventarioConteo Crear(long usuarioId, DateTimeOffset fechaApertura,
        int nroAuditoria)
    {
        if (usuarioId    <= 0) throw new ArgumentException("El usuario es requerido.");
        if (nroAuditoria <= 0) throw new ArgumentException("El número de auditoría debe ser mayor a cero.");

        return new InventarioConteo
        {
            UsuarioId     = usuarioId,
            FechaApertura = fechaApertura,
            FechaAlta     = DateTimeOffset.UtcNow,
            NroAuditoria  = nroAuditoria
        };
    }

    public void Cerrar(DateTimeOffset fechaCierre)
    {
        if (FechaCierre.HasValue)
            throw new InvalidOperationException("El inventario ya fue cerrado.");
        FechaCierre = fechaCierre;
    }
}
