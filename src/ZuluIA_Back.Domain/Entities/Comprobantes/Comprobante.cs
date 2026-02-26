using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Comprobantes;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class Comprobante : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long? PuntoFacturacionId { get; private set; }
    public long TipoComprobanteId { get; private set; }
    public NroComprobante Numero { get; private set; } = null!;
    public DateOnly Fecha { get; private set; }
    public DateOnly? FechaVencimiento { get; private set; }
    public long TerceroId { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1;
    public decimal Subtotal { get; private set; }
    public decimal DescuentoImporte { get; private set; }
    public decimal NetoGravado { get; private set; }
    public decimal NetoNoGravado { get; private set; }
    public decimal IvaRi { get; private set; }
    public decimal IvaRni { get; private set; }
    public decimal Percepciones { get; private set; }
    public decimal Retenciones { get; private set; }
    public decimal Total { get; private set; }
    public decimal Saldo { get; private set; }
    public string? Cae { get; private set; }
    public DateOnly? FechaVtoCae { get; private set; }
    public string? QrData { get; private set; }
    public EstadoComprobante Estado { get; private set; }
    public string? Observacion { get; private set; }

    private readonly List<ComprobanteItem> _items = [];
    public IReadOnlyCollection<ComprobanteItem> Items => _items.AsReadOnly();

    private Comprobante() { }

    public static Comprobante Crear(
        long sucursalId,
        long? puntoFacturacionId,
        long tipoComprobanteId,
        short prefijo,
        long numero,
        DateOnly fecha,
        long terceroId,
        long monedaId,
        decimal cotizacion,
        long? userId)
    {
        var comprobante = new Comprobante
        {
            SucursalId         = sucursalId,
            PuntoFacturacionId = puntoFacturacionId,
            TipoComprobanteId  = tipoComprobanteId,
            Numero             = new NroComprobante(prefijo, numero),
            Fecha              = fecha,
            TerceroId          = terceroId,
            MonedaId           = monedaId,
            Cotizacion         = cotizacion,
            Estado             = EstadoComprobante.Borrador
        };

        comprobante.SetCreated(userId);
        return comprobante;
    }

    public void AgregarItem(ComprobanteItem item)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("No se pueden agregar ítems a un comprobante que no está en borrador.");

        _items.Add(item);
        RecalcularTotales();
    }

    public void Emitir(string? cae, DateOnly? fechaVtoCae, long? userId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se pueden emitir comprobantes en estado Borrador.");

        if (!_items.Any())
            throw new InvalidOperationException("No se puede emitir un comprobante sin ítems.");

        Estado      = EstadoComprobante.Emitido;
        Cae         = cae;
        FechaVtoCae = fechaVtoCae;
        SetUpdated(userId);
        AddDomainEvent(new ComprobanteEmitidoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("El comprobante ya está anulado.");

        Estado = EstadoComprobante.Anulado;
        SetDeleted();
        SetUpdated(userId);
        AddDomainEvent(new ComprobanteAnuladoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
    }

    public void AplicarPago(decimal importe)
    {
        if (importe <= 0)
            throw new InvalidOperationException("El importe a aplicar debe ser mayor a 0.");

        Saldo -= importe;

        if (Saldo <= 0)
        {
            Saldo  = 0;
            Estado = EstadoComprobante.Pagado;
        }
        else
        {
            Estado = EstadoComprobante.PagadoParcial;
        }
    }

    public void SetFechaVencimiento(DateOnly fecha) =>
        FechaVencimiento = fecha;

    public void SetObservacion(string? obs) =>
        Observacion = obs?.Trim();

    public void SetQrData(string? qr) =>
        QrData = qr;

    private void RecalcularTotales()
    {
        Subtotal         = _items.Sum(i => i.TotalLinea);
        NetoGravado      = _items.Sum(i => i.SubtotalNeto);
        IvaRi            = _items.Sum(i => i.IvaImporte);
        Total            = NetoGravado + NetoNoGravado + IvaRi + IvaRni + Percepciones - Retenciones - DescuentoImporte;
        Saldo            = Total;
    }
}