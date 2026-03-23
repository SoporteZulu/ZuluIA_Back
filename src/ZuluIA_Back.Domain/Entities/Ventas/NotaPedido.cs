using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Ventas;

public class NotaPedido : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long TerceroId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public DateOnly? FechaVencimiento { get; private set; }
    public decimal Total { get; private set; }
    public EstadoNotaPedido Estado { get; private set; }
    public string? Observacion { get; private set; }
    public long? VendedorId { get; private set; }

    private readonly List<NotaPedidoItem> _items = [];
    public IReadOnlyCollection<NotaPedidoItem> Items => _items.AsReadOnly();

    private NotaPedido() { }

    public static NotaPedido Crear(
        long sucursalId,
        long terceroId,
        DateOnly fecha,
        DateOnly? fechaVencimiento,
        string? observacion,
        long? vendedorId,
        long? userId)
    {
        var np = new NotaPedido
        {
            SucursalId       = sucursalId,
            TerceroId        = terceroId,
            Fecha            = fecha,
            FechaVencimiento = fechaVencimiento,
            Estado           = EstadoNotaPedido.Abierta,
            Observacion      = observacion?.Trim(),
            VendedorId       = vendedorId,
            Total            = 0
        };

        np.SetCreated(userId);
        return np;
    }

    public void AgregarItem(NotaPedidoItem item)
    {
        if (Estado == EstadoNotaPedido.Cerrada || Estado == EstadoNotaPedido.Anulada)
            throw new InvalidOperationException($"No se pueden agregar ítems a una nota de pedido {Estado}.");
        _items.Add(item);
        RecalcularTotal();
    }

    private void RecalcularTotal() =>
        Total = _items.Sum(x => x.Subtotal);

    public void ActualizarEstadoParcial(long? userId)
    {
        if (Estado == EstadoNotaPedido.Anulada) return;
        var tienePendientes = _items.Any(x => x.CantidadPendiente > 0);
        Estado = tienePendientes ? EstadoNotaPedido.Parcial : EstadoNotaPedido.Cerrada;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoNotaPedido.Anulada)
            throw new InvalidOperationException("La nota de pedido ya está anulada.");
        if (Estado == EstadoNotaPedido.Cerrada)
            throw new InvalidOperationException("No se puede anular una nota de pedido cerrada.");
        Estado = EstadoNotaPedido.Anulada;
        SetDeleted();
        SetUpdated(userId);
    }
}
