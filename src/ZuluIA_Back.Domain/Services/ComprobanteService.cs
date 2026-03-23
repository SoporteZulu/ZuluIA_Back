using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Servicio de dominio que centraliza la lógica de imputación entre comprobantes.
/// </summary>
public class ComprobanteService(
    IComprobanteRepository comprobanteRepo,
    IImputacionRepository imputacionRepo)
{
    /// <summary>
    /// Imputa un comprobante origen (NC, anticipo) contra un destino (Factura).
    /// Actualiza los saldos de ambos comprobantes.
    /// </summary>
    public virtual async Task<Imputacion> ImputarAsync(
        long comprobanteOrigenId,
        long comprobanteDestinoId,
        decimal importe,
        DateOnly fecha,
        long? userId,
        CancellationToken ct = default)
    {
        var origen = await comprobanteRepo.GetByIdAsync(comprobanteOrigenId, ct)
            ?? throw new InvalidOperationException(
                $"No se encontró el comprobante origen ID {comprobanteOrigenId}.");

        var destino = await comprobanteRepo.GetByIdAsync(comprobanteDestinoId, ct)
            ?? throw new InvalidOperationException(
                $"No se encontró el comprobante destino ID {comprobanteDestinoId}.");

        if (importe > origen.Saldo)
            throw new InvalidOperationException(
                $"El importe a imputar ({importe}) supera el saldo " +
                $"disponible del comprobante origen ({origen.Saldo}).");

        if (importe > destino.Saldo)
            throw new InvalidOperationException(
                $"El importe a imputar ({importe}) supera el saldo " +
                $"pendiente del comprobante destino ({destino.Saldo}).");

        var imputacion = Imputacion.Crear(
            comprobanteOrigenId,
            comprobanteDestinoId,
            importe,
            fecha,
            userId);

        origen.ActualizarSaldo(importe, userId);
        destino.ActualizarSaldo(importe, userId);

        comprobanteRepo.Update(origen);
        comprobanteRepo.Update(destino);
        await imputacionRepo.AddAsync(imputacion, ct);

        return imputacion;
    }
}