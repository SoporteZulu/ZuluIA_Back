using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class Cedulon : AuditableEntity
{
    public long TerceroId { get; private set; }
    public long SucursalId { get; private set; }
    public long? PlanPagoId { get; private set; }
    public long? PlanGeneralColegioId { get; private set; }
    public long? LoteColegioId { get; private set; }
    public long? ComprobanteId { get; private set; }
    public string NroCedulon { get; private set; } = string.Empty;
    public DateOnly FechaEmision { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public decimal Importe { get; private set; }
    public decimal ImportePagado { get; private set; }
    public EstadoCedulon Estado { get; private set; }

    private Cedulon() { }

    public static Cedulon Crear(
        long terceroId,
        long sucursalId,
        long? planPagoId,
        string nroCedulon,
        DateOnly fechaEmision,
        DateOnly fechaVencimiento,
        decimal importe,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroCedulon);

        if (importe <= 0)
            throw new InvalidOperationException(
                "El importe del cedulón debe ser mayor a 0.");

        if (fechaVencimiento < fechaEmision)
            throw new InvalidOperationException(
                "La fecha de vencimiento no puede ser anterior a la de emisión.");

        var ced = new Cedulon
        {
            TerceroId        = terceroId,
            SucursalId       = sucursalId,
            PlanPagoId       = planPagoId,
            NroCedulon       = nroCedulon.Trim(),
            FechaEmision     = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Importe          = importe,
            ImportePagado    = 0,
            Estado           = EstadoCedulon.Pendiente
        };

        ced.SetCreated(userId);
        return ced;
    }

    public void RegistrarPago(decimal importe, long? userId)
    {
        if (importe <= 0)
            throw new InvalidOperationException(
                "El importe a pagar debe ser mayor a 0.");

        if (ImportePagado + importe > Importe)
            throw new InvalidOperationException(
                "El importe pagado supera el total del cedulón.");

        ImportePagado += importe;
        Estado = ImportePagado >= Importe
            ? EstadoCedulon.Pagado
            : EstadoCedulon.PagadoParcial;

        SetUpdated(userId);
    }

    public void Vencer(long? userId)
    {
        if (Estado == EstadoCedulon.Pagado)
            return;

        Estado = EstadoCedulon.Vencido;
        SetUpdated(userId);
    }

    public void VincularColegio(long planGeneralColegioId, long loteColegioId, long? userId)
    {
        if (planGeneralColegioId <= 0 || loteColegioId <= 0)
            throw new InvalidOperationException("El plan general y el lote de colegio son obligatorios.");

        PlanGeneralColegioId = planGeneralColegioId;
        LoteColegioId = loteColegioId;
        SetUpdated(userId);
    }

    public void AsociarComprobante(long comprobanteId, long? userId)
    {
        if (comprobanteId <= 0)
            throw new InvalidOperationException("El comprobante asociado es obligatorio.");

        if (ComprobanteId.HasValue && ComprobanteId.Value != comprobanteId)
            throw new InvalidOperationException("El cedulón ya posee un comprobante asociado.");

        ComprobanteId = comprobanteId;
        SetUpdated(userId);
    }
}