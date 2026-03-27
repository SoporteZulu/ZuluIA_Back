using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class Caea : AuditableEntity
{
    public long PuntoFacturacionId { get; private set; }
    public string NroCaea { get; private set; } = string.Empty;
    public DateOnly FechaDesde { get; private set; }
    public DateOnly FechaHasta { get; private set; }
    public DateOnly? FechaProcesoAfip { get; private set; }
    public DateOnly? FechaTopeInformarAfip { get; private set; }
    public string TipoComprobante { get; private set; } = string.Empty;
    public int CantidadAsignada { get; private set; }
    public int CantidadUsada { get; private set; }
    public EstadoCaea Estado { get; private set; }

    private Caea() { }

    public static Caea Crear(
        long puntoFacturacionId,
        string nroCaea,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        DateOnly? fechaProcesoAfip,
        DateOnly? fechaTopeInformarAfip,
        string tipoComprobante,
        int cantidadAsignada,
        long? userId)
    {
        var caea = new Caea
        {
            PuntoFacturacionId = puntoFacturacionId,
            NroCaea            = nroCaea.Trim(),
            FechaDesde         = fechaDesde,
            FechaHasta         = fechaHasta,
            FechaProcesoAfip   = fechaProcesoAfip,
            FechaTopeInformarAfip = fechaTopeInformarAfip,
            TipoComprobante    = tipoComprobante.Trim(),
            CantidadAsignada   = cantidadAsignada,
            CantidadUsada      = 0,
            Estado             = EstadoCaea.Activo
        };

        caea.SetCreated(userId);
        return caea;
    }

    public void UsarUnidad(long? userId)
    {
        if (Estado != EstadoCaea.Activo)
            throw new InvalidOperationException("El CAEA no está activo.");
        if (CantidadUsada >= CantidadAsignada)
            throw new InvalidOperationException("Se han agotado los comprobantes del CAEA.");
        CantidadUsada++;
        SetUpdated(userId);
    }

    public void MarcarInformado(long? userId)
    {
        if (Estado != EstadoCaea.Activo)
            throw new InvalidOperationException("Solo se pueden informar CAEAs activos.");
        Estado = EstadoCaea.Informado;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoCaea.Anulado)
            throw new InvalidOperationException("El CAEA ya está anulado.");
        Estado = EstadoCaea.Anulado;
        SetDeleted();
        SetUpdated(userId);
    }
}
