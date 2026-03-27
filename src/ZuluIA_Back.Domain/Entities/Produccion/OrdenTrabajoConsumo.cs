using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Produccion;

public class OrdenTrabajoConsumo : AuditableEntity
{
    public long OrdenTrabajoId { get; private set; }
    public long ItemId { get; private set; }
    public long DepositoId { get; private set; }
    public decimal CantidadPlanificada { get; private set; }
    public decimal CantidadConsumida { get; private set; }
    public long? MovimientoStockId { get; private set; }
    public string? Observacion { get; private set; }

    private OrdenTrabajoConsumo() { }

    public static OrdenTrabajoConsumo Registrar(
        long ordenTrabajoId,
        long itemId,
        long depositoId,
        decimal cantidadPlanificada,
        decimal cantidadConsumida,
        long? movimientoStockId,
        string? observacion,
        long? userId)
    {
        if (cantidadPlanificada < 0 || cantidadConsumida < 0)
            throw new InvalidOperationException("Las cantidades de consumo no pueden ser negativas.");

        var consumo = new OrdenTrabajoConsumo
        {
            OrdenTrabajoId = ordenTrabajoId,
            ItemId = itemId,
            DepositoId = depositoId,
            CantidadPlanificada = cantidadPlanificada,
            CantidadConsumida = cantidadConsumida,
            MovimientoStockId = movimientoStockId,
            Observacion = observacion?.Trim()
        };

        consumo.SetCreated(userId);
        return consumo;
    }
}
