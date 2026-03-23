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
    // Usa el value object que corresponda en tu dominio:
    public NroComprobante Numero { get; private set; } = null!;
    // Alternativa si usas el nuevo VO:
    // public NumeroComprobante Numero { get; private set; } = null!;
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
    public long? TimbradoId { get; private set; }
    public string? NroTimbrado { get; private set; }
    public EstadoSifenParaguay? EstadoSifen { get; private set; }
    public string? SifenCodigoRespuesta { get; private set; }
    public string? SifenMensajeRespuesta { get; private set; }
    public string? SifenTrackingId { get; private set; }
    public string? SifenCdc { get; private set; }
    public string? SifenNumeroLote { get; private set; }
    public DateTimeOffset? SifenFechaRespuesta { get; private set; }
    public string? Cae { get; private set; }
    public DateOnly? FechaVtoCae { get; private set; }
    public string? QrData { get; private set; }
    public EstadoComprobante Estado { get; private set; }
    public string? Observacion { get; private set; }

    /// <summary>
    /// Cuando este comprobante fue generado desde un presupuesto,
    /// almacena el ID del presupuesto de origen.
    /// </summary>
    public long? ComprobanteOrigenId { get; private set; }

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
        DateOnly? fechaVencimiento,
        long terceroId,
        long monedaId,
        decimal cotizacion,
        string? observacion,
        long? userId)
    {
        var comp = new Comprobante
        {
            SucursalId          = sucursalId,
            PuntoFacturacionId  = puntoFacturacionId,
            TipoComprobanteId   = tipoComprobanteId,
            Numero              = new NroComprobante(prefijo, numero),
            // Si usas el nuevo VO, reemplaza por:
            // Numero = NumeroComprobante.Crear(prefijo, numero),
            Fecha               = fecha,
            FechaVencimiento    = fechaVencimiento,
            TerceroId           = terceroId,
            MonedaId            = monedaId,
            Cotizacion          = cotizacion <= 0 ? 1 : cotizacion,
            Estado              = EstadoComprobante.Borrador,
            Observacion         = observacion?.Trim()
        };

        comp.SetCreated(userId);
        return comp;
    }

    public void AgregarItem(ComprobanteItem item)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se pueden agregar ítems a comprobantes en borrador.");

        _items.Add(item);
        RecalcularTotales();
    }

    public void RemoverItem(long itemId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se pueden remover ítems de comprobantes en borrador.");

        var item = _items.FirstOrDefault(x => x.Id == itemId);
        if (item is not null)
        {
            _items.Remove(item);
            RecalcularTotales();
        }
    }

    public void RecalcularTotales()
    {
        Subtotal        = _items.Sum(x => x.TotalLinea + x.IvaImporte);
        NetoGravado     = _items.Where(x => x.EsGravado).Sum(x => x.SubtotalNeto);
        NetoNoGravado   = _items.Where(x => !x.EsGravado).Sum(x => x.SubtotalNeto);
        IvaRi           = _items.Sum(x => x.IvaImporte);
        IvaRni          = 0;
        DescuentoImporte = _items.Sum(x =>
            x.PrecioUnitario * x.Cantidad * (x.DescuentoPct / 100));

        Total  = NetoGravado + NetoNoGravado + IvaRi + IvaRni +
                 Percepciones - Retenciones;
        Saldo  = Total;
    }

    public void SetPercepciones(decimal percepciones, long? userId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se pueden modificar percepciones en comprobantes borrador.");

        Percepciones = percepciones;
        RecalcularTotales();
        SetUpdated(userId);
    }

    public void SetRetenciones(decimal retenciones, long? userId)
    {
        Retenciones = retenciones;
        RecalcularTotales();
        SetUpdated(userId);
    }

    public void AplicarPago(decimal importe)
    {
        if (Estado != EstadoComprobante.Emitido)
        {
            throw new InvalidOperationException("Solo se puede aplicar un pago a un comprobante emitido.");
        }

        if (importe <= 0)
        {
            throw new ArgumentException("El importe debe ser mayor a cero.", nameof(importe));
        }

        Saldo -= importe;

        if (Saldo <= 0)
        {
            Saldo = 0;
            Estado = EstadoComprobante.Pagado;
        }
        else
        {
            Estado = EstadoComprobante.PagadoParcial;
        }
    }

    public void Emitir(long? userId)
    {
        if (Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException($"No se puede emitir un comprobante en estado {Estado}.");

        if (!_items.Any())
            throw new InvalidOperationException("No se puede emitir un comprobante sin ítems.");

        Estado = EstadoComprobante.Emitido;
        Saldo  = Total;
        AddDomainEvent(new ComprobanteEmitidoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
        SetUpdated(userId);
    }

    public void AsignarCae(string cae, DateOnly fechaVto, string? qrData, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cae);
        Cae         = cae.Trim();
        FechaVtoCae = fechaVto;
        QrData      = qrData;
        SetUpdated(userId);
    }

    public void AsignarTimbrado(long timbradoId, string nroTimbrado, long? userId)
    {
        if (timbradoId <= 0)
            throw new ArgumentException("El timbrado es requerido.", nameof(timbradoId));

        ArgumentException.ThrowIfNullOrWhiteSpace(nroTimbrado);

        TimbradoId = timbradoId;
        NroTimbrado = nroTimbrado.Trim();
        SetUpdated(userId);
    }

    public void RegistrarResultadoSifen(
        EstadoSifenParaguay estado,
        string? codigoRespuesta,
        string? mensajeRespuesta,
        string? trackingId,
        string? cdc,
        string? numeroLote,
        DateTimeOffset? fechaRespuesta,
        long? userId)
    {
        EstadoSifen = estado;
        SifenCodigoRespuesta = string.IsNullOrWhiteSpace(codigoRespuesta) ? null : codigoRespuesta.Trim();
        SifenMensajeRespuesta = string.IsNullOrWhiteSpace(mensajeRespuesta) ? null : mensajeRespuesta.Trim();
        SifenTrackingId = string.IsNullOrWhiteSpace(trackingId) ? null : trackingId.Trim();
        SifenCdc = string.IsNullOrWhiteSpace(cdc) ? null : cdc.Trim();
        SifenNumeroLote = string.IsNullOrWhiteSpace(numeroLote) ? null : numeroLote.Trim();
        SifenFechaRespuesta = fechaRespuesta ?? DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("El comprobante ya está anulado.");

        Estado = EstadoComprobante.Anulado;
        Saldo  = 0;
        AddDomainEvent(new ComprobanteAnuladoEvent(Id, SucursalId, TerceroId, Total, MonedaId));
        SetDeleted();
        SetUpdated(userId);
    }

    /// <summary>
    /// Marca un presupuesto/comprobante borrador como convertido a un comprobante definitivo.
    /// Equivale a la acción "Convertir Presupuesto a Factura" del VB6.
    /// </summary>
    public void MarcarComoConvertido(long? userId)
    {
        if (Estado is not (EstadoComprobante.Borrador or EstadoComprobante.Emitido))
            throw new InvalidOperationException("Solo se puede marcar como Convertido un presupuesto en estado Borrador o Emitido.");

        Estado = EstadoComprobante.Convertido;
        SetUpdated(userId);
    }

    /// <summary>Establece el comprobante de origen cuando se convierte un presupuesto.</summary>
    public void SetComprobanteOrigen(long origenId) => ComprobanteOrigenId = origenId;

    public void ActualizarSaldo(decimal importeImputado, long? userId)
    {
        Saldo -= importeImputado;
        if (Saldo < 0) Saldo = 0;

        if (Saldo == 0)
            Estado = EstadoComprobante.Pagado;
        else if (Saldo < Total)
            Estado = EstadoComprobante.PagadoParcial;

        SetUpdated(userId);
    }

    public void SetFechaVencimiento(DateOnly fecha, long? userId)
    {
        FechaVencimiento = fecha;
        SetUpdated(userId);
    }
}
