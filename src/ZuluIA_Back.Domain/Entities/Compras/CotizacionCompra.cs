using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Compras;

public class CotizacionCompra : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long? RequisicionId { get; private set; }
    public long ProveedorId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public DateOnly? FechaVencimiento { get; private set; }
    public decimal Total { get; private set; }
    public EstadoCotizacionCompra Estado { get; private set; }
    public string? Observacion { get; private set; }

    private readonly List<CotizacionCompraItem> _items = [];
    public IReadOnlyCollection<CotizacionCompraItem> Items => _items.AsReadOnly();

    private CotizacionCompra() { }

    public static CotizacionCompra Crear(
        long sucursalId,
        long? requisicionId,
        long proveedorId,
        DateOnly fecha,
        DateOnly? fechaVencimiento,
        string? observacion,
        long? userId)
    {
        var cotizacion = new CotizacionCompra
        {
            SucursalId      = sucursalId,
            RequisicionId   = requisicionId,
            ProveedorId     = proveedorId,
            Fecha           = fecha,
            FechaVencimiento = fechaVencimiento,
            Estado          = EstadoCotizacionCompra.Pendiente,
            Observacion     = observacion?.Trim(),
            Total           = 0
        };

        cotizacion.SetCreated(userId);
        return cotizacion;
    }

    public void AgregarItem(CotizacionCompraItem item)
    {
        if (Estado != EstadoCotizacionCompra.Pendiente)
            throw new InvalidOperationException("Solo se pueden agregar ítems a cotizaciones pendientes.");
        _items.Add(item);
        RecalcularTotal();
    }

    private void RecalcularTotal() =>
        Total = _items.Sum(x => x.Total);

    public void Aceptar(long? userId)
    {
        if (Estado != EstadoCotizacionCompra.Pendiente)
            throw new InvalidOperationException("Solo se pueden aceptar cotizaciones pendientes.");
        Estado = EstadoCotizacionCompra.Aceptada;
        SetUpdated(userId);
    }

    public void Rechazar(long? userId)
    {
        if (Estado != EstadoCotizacionCompra.Pendiente)
            throw new InvalidOperationException("Solo se pueden rechazar cotizaciones pendientes.");
        Estado = EstadoCotizacionCompra.Rechazada;
        SetUpdated(userId);
    }

    public void MarcarProcesada(long? userId)
    {
        if (Estado != EstadoCotizacionCompra.Aceptada)
            throw new InvalidOperationException("Solo se pueden procesar cotizaciones aceptadas.");
        Estado = EstadoCotizacionCompra.Procesada;
        SetUpdated(userId);
    }
}
