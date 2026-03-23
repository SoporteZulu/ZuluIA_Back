using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

/// <summary>
/// Timbrado de comprobantes para Paraguay (SIFEN/SET).
/// Autorización fiscal del SET que habilita la emisión de un rango de comprobantes
/// para una sucursal + punto de facturación + tipo de comprobante determinado.
/// Migrado desde VB6: frmTimbrado / TIMBRADO.
/// </summary>
public class Timbrado : BaseEntity
{
    public long SucursalId { get; private set; }
    public long PuntoFacturacionId { get; private set; }
    public long TipoComprobanteId { get; private set; }
    /// <summary>Número de timbrado asignado por el SET.</summary>
    public string NroTimbrado { get; private set; } = string.Empty;
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public int NroComprobanteDesde { get; private set; }
    public int NroComprobanteHasta { get; private set; }
    public bool Activo { get; private set; } = true;

    private Timbrado() { }

    public static Timbrado Crear(
        long sucursalId,
        long puntoFacturacionId,
        long tipoComprobanteId,
        string nroTimbrado,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int nroComprobanteDesde,
        int nroComprobanteHasta)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroTimbrado);
        if (fechaFin < fechaInicio)
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        if (nroComprobanteDesde <= 0 || nroComprobanteHasta < nroComprobanteDesde)
            throw new ArgumentException("El rango de comprobantes no es válido.");

        return new Timbrado
        {
            SucursalId            = sucursalId,
            PuntoFacturacionId    = puntoFacturacionId,
            TipoComprobanteId     = tipoComprobanteId,
            NroTimbrado           = nroTimbrado.Trim(),
            FechaInicio           = fechaInicio,
            FechaFin              = fechaFin,
            NroComprobanteDesde   = nroComprobanteDesde,
            NroComprobanteHasta   = nroComprobanteHasta,
            Activo                = true
        };
    }

    public void Actualizar(DateOnly fechaInicio, DateOnly fechaFin, int nroDesde, int nroHasta)
    {
        if (fechaFin < fechaInicio) throw new ArgumentException("Rango de fechas inválido.");
        if (nroHasta < nroDesde)    throw new ArgumentException("Rango de comprobantes inválido.");
        FechaInicio         = fechaInicio;
        FechaFin            = fechaFin;
        NroComprobanteDesde = nroDesde;
        NroComprobanteHasta = nroHasta;
    }

    public void Desactivar() => Activo = false;
    public void Activar()    => Activo = true;

    /// <summary>Indica si el timbrado está vigente a la fecha dada.</summary>
    public bool EsVigente(DateOnly fecha) => Activo && fecha >= FechaInicio && fecha <= FechaFin;
}
