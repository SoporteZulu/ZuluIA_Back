using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Logistica;

/// <summary>
/// Asignación de ruta de reparto a un cliente.
/// Migrado desde VB6: REPARTOSCLIENTES.
/// </summary>
public class RepartoCliente : AuditableEntity
{
    public long   SucursalId  { get; private set; }
    public long   TerceroId   { get; private set; }
    public string Ruta        { get; private set; } = string.Empty;
    public int    Orden       { get; private set; }
    public string? DiaSemana  { get; private set; }  // LUN, MAR, MIE, JUE, VIE, SAB
    public bool   Activo      { get; private set; } = true;

    private RepartoCliente() { }

    public static RepartoCliente Crear(long sucursalId, long terceroId, string ruta, int orden, string? diaSemana, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruta);
        var r = new RepartoCliente
        {
            SucursalId = sucursalId,
            TerceroId  = terceroId,
            Ruta       = ruta.Trim().ToUpperInvariant(),
            Orden      = orden,
            DiaSemana  = diaSemana?.Trim().ToUpperInvariant(),
            Activo     = true
        };
        r.SetCreated(userId);
        return r;
    }

    public void Actualizar(string ruta, int orden, string? diaSemana, long? userId)
    {
        Ruta      = ruta.Trim().ToUpperInvariant();
        Orden     = orden;
        DiaSemana = diaSemana?.Trim().ToUpperInvariant();
        SetUpdated(userId);
    }

    public void Desactivar(long? userId) { Activo = false; SetDeleted(); SetUpdated(userId); }
}
