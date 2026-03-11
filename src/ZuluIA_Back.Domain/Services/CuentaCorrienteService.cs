using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Servicio de dominio que centraliza los movimientos de cuenta corriente.
/// </summary>
public class CuentaCorrienteService(
    ICuentaCorrienteRepository ctaCteRepo,
    IMovimientoCtaCteRepository movimientoRepo)
{
    /// <summary>
    /// Registra un débito en la cuenta corriente del tercero.
    /// Usado al emitir una factura/comprobante a cobrar.
    /// </summary>
    public async Task<MovimientoCtaCte> DebitarAsync(
        long terceroId,
        long? sucursalId,
        long monedaId,
        decimal importe,
        long? comprobanteId,
        DateOnly fecha,
        string? descripcion,
        CancellationToken ct = default)
    {
        var cta = await ctaCteRepo.GetOrCreateAsync(
            terceroId, monedaId, sucursalId, ct);

        cta.Debitar(importe);
        ctaCteRepo.Update(cta);

        var mov = MovimientoCtaCte.Crear(
            terceroId, sucursalId, monedaId,
            comprobanteId, fecha,
            importe, 0, cta.Saldo, descripcion);

        await movimientoRepo.AddAsync(mov, ct);
        return mov;
    }

    /// <summary>
    /// Registra un crédito en la cuenta corriente del tercero.
    /// Usado al registrar un cobro/pago o NC.
    /// </summary>
    public async Task<MovimientoCtaCte> AcreditarAsync(
        long terceroId,
        long? sucursalId,
        long monedaId,
        decimal importe,
        long? comprobanteId,
        DateOnly fecha,
        string? descripcion,
        CancellationToken ct = default)
    {
        var cta = await ctaCteRepo.GetOrCreateAsync(
            terceroId, monedaId, sucursalId, ct);

        cta.Acreditar(importe);
        ctaCteRepo.Update(cta);

        var mov = MovimientoCtaCte.Crear(
            terceroId, sucursalId, monedaId,
            comprobanteId, fecha,
            0, importe, cta.Saldo, descripcion);

        await movimientoRepo.AddAsync(mov, ct);
        return mov;
    }
}