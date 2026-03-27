using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Produccion;

public class OrdenEmpaque : AuditableEntity
{
    public long OrdenTrabajoId { get; private set; }
    public long ItemId { get; private set; }
    public long DepositoId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal Cantidad { get; private set; }
    public string? Lote { get; private set; }
    public string? Observacion { get; private set; }
    public EstadoOrdenEmpaque Estado { get; private set; }

    private OrdenEmpaque() { }

    public static OrdenEmpaque Crear(
        long ordenTrabajoId,
        long itemId,
        long depositoId,
        DateOnly fecha,
        decimal cantidad,
        string? lote,
        string? observacion,
        long? userId)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad a empacar debe ser mayor a 0.");

        var orden = new OrdenEmpaque
        {
            OrdenTrabajoId = ordenTrabajoId,
            ItemId = itemId,
            DepositoId = depositoId,
            Fecha = fecha,
            Cantidad = cantidad,
            Lote = lote?.Trim(),
            Observacion = observacion?.Trim(),
            Estado = EstadoOrdenEmpaque.Pendiente
        };

        orden.SetCreated(userId);
        return orden;
    }

    public void Confirmar(long? userId)
    {
        if (Estado != EstadoOrdenEmpaque.Pendiente)
            throw new InvalidOperationException("Solo se puede confirmar una orden de empaque pendiente.");

        Estado = EstadoOrdenEmpaque.Confirmada;
        SetUpdated(userId);
    }
}
