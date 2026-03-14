using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Registra la transferencia de fondos entre dos cajas/cuentas bancarias.
/// Equivale a la funcionalidad de frmTransferenciasCajasCuentasBancarias del VB6.
/// </summary>
public class TransferenciaCaja : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long CajaOrigenId { get; private set; }
    public long CajaDestinoId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal Importe { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; } = 1;
    public string? Concepto { get; private set; }

    /// <summary>
    /// ID del movimiento de egreso en la caja origen (generado automáticamente).
    /// </summary>
    public long? MovimientoOrigenId { get; private set; }

    /// <summary>
    /// ID del movimiento de ingreso en la caja destino (generado automáticamente).
    /// </summary>
    public long? MovimientoDestinoId { get; private set; }

    public bool Anulada { get; private set; }

    private TransferenciaCaja() { }

    public static TransferenciaCaja Registrar(
        long sucursalId,
        long cajaOrigenId,
        long cajaDestinoId,
        DateOnly fecha,
        decimal importe,
        long monedaId,
        decimal cotizacion,
        string? concepto,
        long? userId)
    {
        if (cajaOrigenId == cajaDestinoId)
            throw new InvalidOperationException("La caja origen y destino no pueden ser la misma.");
        if (importe <= 0)
            throw new InvalidOperationException("El importe debe ser mayor a cero.");

        var t = new TransferenciaCaja
        {
            SucursalId     = sucursalId,
            CajaOrigenId   = cajaOrigenId,
            CajaDestinoId  = cajaDestinoId,
            Fecha          = fecha,
            Importe        = importe,
            MonedaId       = monedaId,
            Cotizacion     = cotizacion <= 0 ? 1 : cotizacion,
            Concepto       = concepto?.Trim(),
            Anulada        = false
        };

        t.SetCreated(userId);
        return t;
    }

    public void AsignarMovimientos(long movOrigenId, long movDestinoId)
    {
        MovimientoOrigenId  = movOrigenId;
        MovimientoDestinoId = movDestinoId;
    }

    public void Anular(long? userId)
    {
        if (Anulada)
            throw new InvalidOperationException("La transferencia ya está anulada.");

        Anulada = true;
        SetDeleted();
        SetUpdated(userId);
    }
}
