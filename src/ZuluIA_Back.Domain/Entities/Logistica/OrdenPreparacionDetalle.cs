using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Logistica;

/// <summary>
/// Detalle de una orden de preparación (picking).
/// Equivale a clsOrdenDePreparacionDetalle del VB6.
/// </summary>
public class OrdenPreparacionDetalle : BaseEntity
{
    public long OrdenPreparacionId { get; private set; }
    public long ItemId { get; private set; }
    public long DepositoId { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal CantidadEntregada { get; private set; }
    public string? Observacion { get; private set; }

    private OrdenPreparacionDetalle() { }

    internal static OrdenPreparacionDetalle Crear(
        long ordenId,
        long itemId,
        long depositoId,
        decimal cantidad,
        string? observacion = null)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

        return new OrdenPreparacionDetalle
        {
            OrdenPreparacionId = ordenId,
            ItemId             = itemId,
            DepositoId         = depositoId,
            Cantidad           = cantidad,
            CantidadEntregada  = 0,
            Observacion        = observacion?.Trim()
        };
    }

    public void RegistrarEntrega(decimal cantidadEntregada)
    {
        if (cantidadEntregada < 0)
            throw new InvalidOperationException("La cantidad entregada no puede ser negativa.");
        if (cantidadEntregada > Cantidad)
            throw new InvalidOperationException("La cantidad entregada no puede superar la cantidad solicitada.");

        CantidadEntregada = cantidadEntregada;
    }

    public bool EstaCompleto => CantidadEntregada >= Cantidad;
}
