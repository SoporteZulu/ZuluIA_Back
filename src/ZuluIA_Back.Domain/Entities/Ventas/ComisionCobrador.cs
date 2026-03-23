using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Ventas;

/// <summary>
/// Comisión calculada sobre cobranzas realizadas.
/// Migrado desde VB6: COMISIONESCOBRANZAS.
/// </summary>
public class ComisionCobrador : AuditableEntity
{
    public long     SucursalId        { get; private set; }
    public long     CobradorId        { get; private set; }
    public int      Periodo           { get; private set; }
    public decimal  MontoCobranzas    { get; private set; }
    public decimal  PorcentajeComision { get; private set; }
    public decimal  MontoComision     { get; private set; }
    public EstadoComision Estado      { get; private set; }

    private ComisionCobrador() { }

    public static ComisionCobrador Crear(
        long sucursalId, long cobradorId, int periodo,
        decimal montoCobranzas, decimal porcentajeComision, long? userId)
    {
        if (porcentajeComision < 0) throw new InvalidOperationException("El porcentaje no puede ser negativo.");
        var c = new ComisionCobrador
        {
            SucursalId         = sucursalId,
            CobradorId         = cobradorId,
            Periodo            = periodo,
            MontoCobranzas     = montoCobranzas,
            PorcentajeComision = porcentajeComision,
            MontoComision      = montoCobranzas * porcentajeComision / 100m,
            Estado             = EstadoComision.Pendiente
        };
        c.SetCreated(userId);
        return c;
    }

    public void Aprobar(long? userId) { Estado = EstadoComision.Aprobada; SetUpdated(userId); }
    public void Anular(long? userId)  { Estado = EstadoComision.Anulada;  SetUpdated(userId); }
}
