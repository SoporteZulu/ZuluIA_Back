using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Servicio de dominio que centraliza la lÛgica de imputaciÛn entre comprobantes.
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
        long? tipoComprobanteOrigenId,
        long? tipoComprobanteDestinoId,
        long? userId,
        CancellationToken ct = default)
    {
        var origen = await comprobanteRepo.GetByIdAsync(comprobanteOrigenId, ct)
            ?? throw new InvalidOperationException(
                $"No se encontrÛ el comprobante origen ID {comprobanteOrigenId}.");

        var destino = await comprobanteRepo.GetByIdAsync(comprobanteDestinoId, ct)
            ?? throw new InvalidOperationException(
                $"No se encontrÛ el comprobante destino ID {comprobanteDestinoId}.");

        if (tipoComprobanteOrigenId.HasValue && origen.TipoComprobanteId != tipoComprobanteOrigenId.Value)
            throw new InvalidOperationException("El comprobante origen no coincide con el tipo de documento requerido.");

        if (tipoComprobanteDestinoId.HasValue && destino.TipoComprobanteId != tipoComprobanteDestinoId.Value)
            throw new InvalidOperationException("El comprobante destino no coincide con el tipo de documento requerido.");

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

    public async Task<Imputacion> DesimputarAsync(
        long imputacionId,
        DateOnly fecha,
        string? motivo,
        long? userId,
        CancellationToken ct = default)
    {
        var imputacion = await imputacionRepo.GetByIdAsync(imputacionId, ct)
            ?? throw new InvalidOperationException($"No se encontr√≥ la imputaci√≥n ID {imputacionId}.");

        if (imputacion.Anulada)
            throw new InvalidOperationException("La imputaci√≥n indicada ya fue desimputada.");

        var origen = await comprobanteRepo.GetByIdAsync(imputacion.ComprobanteOrigenId, ct)
            ?? throw new InvalidOperationException($"No se encontr√≥ el comprobante origen ID {imputacion.ComprobanteOrigenId}.");

        var destino = await comprobanteRepo.GetByIdAsync(imputacion.ComprobanteDestinoId, ct)
            ?? throw new InvalidOperationException($"No se encontr√≥ el comprobante destino ID {imputacion.ComprobanteDestinoId}.");

        origen.RevertirSaldo(imputacion.Importe, userId);
        destino.RevertirSaldo(imputacion.Importe, userId);
        imputacion.Desimputar(fecha, motivo, userId);

        comprobanteRepo.Update(origen);
        comprobanteRepo.Update(destino);
        imputacionRepo.Update(imputacion);

        return imputacion;
    }
}