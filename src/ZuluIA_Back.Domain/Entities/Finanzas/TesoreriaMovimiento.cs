using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class TesoreriaMovimiento : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long CajaCuentaId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public TipoOperacionTesoreria TipoOperacion { get; private set; }
    public SentidoMovimientoTesoreria Sentido { get; private set; }
    public long? TerceroId { get; private set; }
    public decimal Importe { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1m;
    public string? ReferenciaTipo { get; private set; }
    public long? ReferenciaId { get; private set; }
    public string? Observacion { get; private set; }
    public bool Anulado { get; private set; }

    private TesoreriaMovimiento() { }

    public static TesoreriaMovimiento Registrar(
        long sucursalId,
        long cajaCuentaId,
        DateOnly fecha,
        TipoOperacionTesoreria tipoOperacion,
        SentidoMovimientoTesoreria sentido,
        decimal importe,
        long monedaId,
        decimal cotizacion,
        long? terceroId,
        string? referenciaTipo,
        long? referenciaId,
        string? observacion,
        long? userId)
    {
        if (importe <= 0)
            throw new InvalidOperationException("El importe del movimiento debe ser mayor a cero.");

        var movimiento = new TesoreriaMovimiento
        {
            SucursalId = sucursalId,
            CajaCuentaId = cajaCuentaId,
            Fecha = fecha,
            TipoOperacion = tipoOperacion,
            Sentido = sentido,
            Importe = importe,
            MonedaId = monedaId,
            Cotizacion = cotizacion <= 0 ? 1m : cotizacion,
            TerceroId = terceroId,
            ReferenciaTipo = referenciaTipo?.Trim(),
            ReferenciaId = referenciaId,
            Observacion = observacion?.Trim(),
            Anulado = false
        };

        movimiento.SetCreated(userId);
        return movimiento;
    }

    public void Anular(long? userId)
    {
        if (Anulado)
            throw new InvalidOperationException("El movimiento de tesorería ya está anulado.");

        Anulado = true;
        SetDeleted();
        SetUpdated(userId);
    }
}
