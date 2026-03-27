using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
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
    public string? Cae { get; private set; }
    public string? Caea { get; private set; }
    public DateOnly? FechaVtoCae { get; private set; }
    public string? QrData { get; private set; }
    public EstadoAfipWsfe EstadoAfip { get; private set; } = EstadoAfipWsfe.Pendiente;
    public string? UltimoErrorAfip { get; private set; }
    public DateTimeOffset? FechaUltimaConsultaAfip { get; private set; }
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
        SetUpdated(userId);
    }

    public void AsignarCae(string cae, DateOnly fechaVto, string? qrData, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cae);
        Cae         = cae.Trim();
        Caea        = null;
        FechaVtoCae = fechaVto;
        QrData      = qrData;
        EstadoAfip  = EstadoAfipWsfe.AutorizadoCae;
        UltimoErrorAfip = null;
        SetUpdated(userId);
    }

    public void AsignarCaea(string caea, DateOnly fechaVto, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caea);
        Caea = caea.Trim();
        FechaVtoCae = fechaVto;
        EstadoAfip = EstadoAfipWsfe.AutorizadoCaea;
        UltimoErrorAfip = null;
        SetUpdated(userId);
    }

    public void RegistrarEstadoAfip(EstadoAfipWsfe estadoAfip, string? ultimoError, DateTimeOffset fechaConsulta, long? userId)
    {
        EstadoAfip = estadoAfip;
        UltimoErrorAfip = ultimoError?.Trim();
        FechaUltimaConsultaAfip = fechaConsulta;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("El comprobante ya está anulado.");

        Estado = EstadoComprobante.Anulado;
        Saldo  = 0;
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

    public void VincularAComprobanteOrigen(long origenId, long? userId)
    {
        if (origenId <= 0)
            throw new InvalidOperationException("El comprobante origen es obligatorio.");

        if (Id != 0 && origenId == Id)
            throw new InvalidOperationException("No se puede vincular un comprobante consigo mismo.");

        if (ComprobanteOrigenId.HasValue && ComprobanteOrigenId.Value != origenId)
            throw new InvalidOperationException("El comprobante ya tiene un origen comercial distinto.");

        ComprobanteOrigenId = origenId;
        SetUpdated(userId);
    }

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

    public void RevertirSaldo(decimal importeDesimputado, long? userId)
    {
        if (importeDesimputado <= 0)
            throw new InvalidOperationException("El importe a revertir debe ser mayor a 0.");

        if (Estado == EstadoComprobante.Anulado)
            throw new InvalidOperationException("No se puede revertir saldo sobre un comprobante anulado.");

        Saldo += importeDesimputado;
        if (Saldo > Total)
            Saldo = Total;

        if (Saldo == 0)
            Estado = EstadoComprobante.Pagado;
        else if (Saldo < Total)
            Estado = EstadoComprobante.PagadoParcial;
        else
            Estado = EstadoComprobante.Emitido;

        SetUpdated(userId);
    }

    public void SetFechaVencimiento(DateOnly fecha, long? userId)
    {
        FechaVencimiento = fecha;
        SetUpdated(userId);
    }
}
