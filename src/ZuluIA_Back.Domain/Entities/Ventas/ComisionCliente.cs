using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Ventas;

/// <summary>
/// Comisión asignada directamente a un cliente sobre operaciones realizadas.
/// Migrado desde VB6: COMISIONESCLIENTES.
/// </summary>
public class ComisionCliente : AuditableEntity
{
    public long    TerceroId         { get; private set; }
    public long    SucursalId        { get; private set; }
    public int     Periodo           { get; private set; }
    public decimal MontoBase         { get; private set; }
    public decimal PorcentajeComision { get; private set; }
    public decimal MontoComision     { get; private set; }
    public EstadoComision Estado     { get; private set; }

    private ComisionCliente() { }

    public static ComisionCliente Crear(
        long terceroId, long sucursalId, int periodo,
        decimal montoBase, decimal porcentajeComision, long? userId)
    {
        if (porcentajeComision < 0) throw new InvalidOperationException("El porcentaje no puede ser negativo.");
        var c = new ComisionCliente
        {
            TerceroId          = terceroId,
            SucursalId         = sucursalId,
            Periodo            = periodo,
            MontoBase          = montoBase,
            PorcentajeComision = porcentajeComision,
            MontoComision      = montoBase * porcentajeComision / 100m,
            Estado             = EstadoComision.Pendiente
        };
        c.SetCreated(userId);
        return c;
    }

    public void Aprobar(long? userId) { Estado = EstadoComision.Aprobada; SetUpdated(userId); }
    public void Anular(long? userId)  { Estado = EstadoComision.Anulada;  SetUpdated(userId); }
}
