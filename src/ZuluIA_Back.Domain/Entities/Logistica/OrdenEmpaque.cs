using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Logistica;

/// <summary>
/// Orden de Empaque para exportaciones y envíos especiales.
/// Basado en VB6: frmOrdenEmpaque.
/// </summary>
public class OrdenEmpaque : AuditableEntity
{
    public long TerceroId { get; private set; }
    public long? SucursalTerceroId { get; private set; }
    public long? VendedorId { get; private set; }
    public long? DepositoId { get; private set; }
    public long? TransportistaId { get; private set; }
    /// <summary>Agente de carga / freight forwarder.</summary>
    public long? AgenteId { get; private set; }
    public long? TipoComprobanteId { get; private set; }
    public long? PuntoFacturacionId { get; private set; }
    public int? MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1m;

    // Numeración
    public int? Prefijo { get; private set; }
    public int? NroComprobante { get; private set; }
    public DateOnly Fecha { get; private set; }
    public DateOnly? FechaEmbarque { get; private set; }
    public DateOnly? FechaVencimiento { get; private set; }

    // Datos de embarque
    public string? OrigenObservacion { get; private set; }
    public string? DestinoObservacion { get; private set; }

    public decimal Total { get; private set; }
    public string Estado { get; private set; } = "PENDIENTE";
    public bool Anulada { get; private set; } = false;
    public string? Observacion { get; private set; }

    private readonly List<OrdenEmpaqueDetalle> _detalles = [];
    public IReadOnlyCollection<OrdenEmpaqueDetalle> Detalles => _detalles.AsReadOnly();

    private OrdenEmpaque() { }

    public static OrdenEmpaque Crear(
        long terceroId,
        long? sucursalTerceroId,
        long? vendedorId,
        long? depositoId,
        long? transportistaId,
        long? agenteId,
        long? tipoComprobanteId,
        long? puntoFacturacionId,
        int? monedaId,
        decimal cotizacion,
        DateOnly fecha,
        DateOnly? fechaEmbarque,
        DateOnly? fechaVencimiento,
        string? origenObservacion,
        string? destinoObservacion,
        decimal total,
        string? observacion,
        long? creadoPor = null)
    {
        if (terceroId <= 0)
            throw new ArgumentException("El tercero es requerido.");

        var orden = new OrdenEmpaque
        {
            TerceroId          = terceroId,
            SucursalTerceroId  = sucursalTerceroId,
            VendedorId         = vendedorId,
            DepositoId         = depositoId,
            TransportistaId    = transportistaId,
            AgenteId           = agenteId,
            TipoComprobanteId  = tipoComprobanteId,
            PuntoFacturacionId = puntoFacturacionId,
            MonedaId           = monedaId,
            Cotizacion         = cotizacion <= 0 ? 1m : cotizacion,
            Fecha              = fecha,
            FechaEmbarque      = fechaEmbarque,
            FechaVencimiento   = fechaVencimiento,
            OrigenObservacion  = origenObservacion?.Trim(),
            DestinoObservacion = destinoObservacion?.Trim(),
            Total              = total,
            Estado             = "PENDIENTE",
            Anulada            = false,
            Observacion        = observacion?.Trim()
        };
        orden.SetCreated(creadoPor);
        return orden;
    }

    public void Confirmar(long? actualizadoPor = null)
    {
        if (Anulada) throw new InvalidOperationException("La orden de empaque está anulada.");
        Estado = "CONFIRMADO";
        SetUpdated(actualizadoPor);
    }

    public void Anular(long? actualizadoPor = null)
    {
        if (Anulada) throw new InvalidOperationException("La orden de empaque ya está anulada.");
        Anulada = true;
        Estado  = "ANULADO";
        SetUpdated(actualizadoPor);
    }

    public void AgregarDetalle(
        long? itemId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal? porcentajeIva,
        string? observacion)
    {
        var det = OrdenEmpaqueDetalle.Crear(Id, itemId, descripcion, cantidad, precioUnitario, porcentajeIva, observacion);
        _detalles.Add(det);
    }
}
