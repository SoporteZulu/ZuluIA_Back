using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Ventas;

/// <summary>
/// Contrato de servicio/abono con facturación periódica recurrente.
/// Basado en el modelo VB6: frmContratos / CONTRATOSDETALLES.
/// El macroestado del ciclo de vida: Vigente → Vencido → Anulado.
/// </summary>
public class Contrato : AuditableEntity
{
    public long TerceroId { get; private set; }
    public long? SucursalTerceroId { get; private set; }
    public long? VendedorId { get; private set; }
    public long? TipoComprobanteId { get; private set; }
    public long? PuntoFacturacionId { get; private set; }
    public int? CondicionVentaId { get; private set; }
    public int? MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1m;
    public DateOnly FechaDesde { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public DateOnly FechaInicioFacturacion { get; private set; }
    /// <summary>Frecuencia de facturación en meses (1=mensual, 2=bimestral…)</summary>
    public int PeriodoMeses { get; private set; } = 1;
    /// <summary>Duración total en cantidad de cuotas/periodos.</summary>
    public int Duracion { get; private set; } = 0;
    public int CuotasRestantes { get; private set; } = 0;
    public string Estado { get; private set; } = "VIGENTE";
    public bool Anulado { get; private set; } = false;
    public string? Observacion { get; private set; }
    public decimal Total { get; private set; }

    private readonly List<ContratoDetalle> _detalles = [];
    public IReadOnlyCollection<ContratoDetalle> Detalles => _detalles.AsReadOnly();

    private Contrato() { }

    public static Contrato Crear(
        long terceroId,
        long? sucursalTerceroId,
        long? vendedorId,
        long? tipoComprobanteId,
        long? puntoFacturacionId,
        int? condicionVentaId,
        int? monedaId,
        decimal cotizacion,
        DateOnly fechaDesde,
        DateOnly fechaVencimiento,
        DateOnly fechaInicioFacturacion,
        int periodoMeses,
        int duracion,
        decimal total,
        string? observacion,
        long? creadoPor = null)
    {
        if (terceroId <= 0)
            throw new ArgumentException("El tercero es requerido.");
        if (fechaVencimiento < fechaDesde)
            throw new ArgumentException("La fecha de vencimiento no puede ser anterior a la fecha de inicio.");
        if (periodoMeses <= 0)
            throw new ArgumentException("El período debe ser mayor a 0.");
        if (total < 0)
            throw new ArgumentException("El total no puede ser negativo.");

        var contrato = new Contrato
        {
            TerceroId              = terceroId,
            SucursalTerceroId      = sucursalTerceroId,
            VendedorId             = vendedorId,
            TipoComprobanteId      = tipoComprobanteId,
            PuntoFacturacionId     = puntoFacturacionId,
            CondicionVentaId       = condicionVentaId,
            MonedaId               = monedaId,
            Cotizacion             = cotizacion <= 0 ? 1m : cotizacion,
            FechaDesde             = fechaDesde,
            FechaVencimiento       = fechaVencimiento,
            FechaInicioFacturacion = fechaInicioFacturacion,
            PeriodoMeses           = periodoMeses,
            Duracion               = duracion,
            CuotasRestantes        = duracion,
            Estado                 = "VIGENTE",
            Anulado                = false,
            Observacion            = observacion?.Trim(),
            Total                  = total
        };
        contrato.SetCreated(creadoPor);
        return contrato;
    }

    public void Actualizar(
        long? vendedorId,
        int? condicionVentaId,
        DateOnly fechaVencimiento,
        int periodoMeses,
        int duracion,
        decimal total,
        string? observacion,
        long? actualizadoPor = null)
    {
        if (Anulado) throw new InvalidOperationException("No se puede modificar un contrato anulado.");
        VendedorId      = vendedorId;
        CondicionVentaId = condicionVentaId;
        FechaVencimiento = fechaVencimiento;
        PeriodoMeses    = periodoMeses;
        Duracion        = duracion;
        Total           = total;
        Observacion     = observacion?.Trim();
        SetUpdated(actualizadoPor);
    }

    public void Anular(string? motivoAnulacion = null, long? anuladoPor = null)
    {
        if (Anulado) throw new InvalidOperationException("El contrato ya está anulado.");
        Anulado   = true;
        Estado    = "ANULADO";
        Observacion = string.IsNullOrWhiteSpace(motivoAnulacion)
            ? Observacion
            : $"{Observacion} | ANULACIÓN: {motivoAnulacion}".Trim(' ', '|', ' ');
        SetUpdated(anuladoPor);
    }

    public void MarcarVencido(long? actualizadoPor = null)
    {
        if (!Anulado) Estado = "VENCIDO";
        SetUpdated(actualizadoPor);
    }

    public void RegistrarFacturacion(long? actualizadoPor = null)
    {
        if (CuotasRestantes > 0) CuotasRestantes--;
        if (CuotasRestantes == 0 && Estado == "VIGENTE") Estado = "VENCIDO";
        SetUpdated(actualizadoPor);
    }

    public ContratoDetalle AgregarDetalle(
        long? itemId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal? porcentajeIva,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        DateOnly fechaPrimeraFactura,
        int frecuenciaMeses,
        int corte,
        string? dominio)
    {
        var det = ContratoDetalle.Crear(
            Id, itemId, descripcion, cantidad, precioUnitario,
            porcentajeIva, fechaDesde, fechaHasta, fechaPrimeraFactura,
            frecuenciaMeses, corte, dominio);
        _detalles.Add(det);
        return det;
    }
}
