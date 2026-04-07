using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class RegistrarNotaDebitoVentaCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    CircuitoVentasService circuitoVentas,
    TerceroOperacionValidationService terceroOperacionValidationService,
    NotaDebitoValidationService notaDebitoValidationService)
    : IRequestHandler<RegistrarNotaDebitoVentaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarNotaDebitoVentaCommand request, CancellationToken ct)
    {
        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TipoComprobanteId, ct);

        if (tipo is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {request.TipoComprobanteId}.");

        if (!NotaDebitoWorkflowRules.EsNotaDebitoVenta(tipo))
            return Result.Failure<long>("El tipo de comprobante indicado no corresponde a una nota de débito de venta.");

        var motivo = await db.MotivosDebito
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.MotivoDebitoId && x.Activo, ct);

        if (motivo is null)
            return Result.Failure<long>($"No se encontró el motivo de débito ID {request.MotivoDebitoId}.");

        if (motivo.RequiereDocumentoOrigen && !request.ComprobanteOrigenId.HasValue)
            return Result.Failure<long>("El motivo de débito seleccionado requiere comprobante origen.");

        var validationError = await terceroOperacionValidationService.ValidateClienteAsync(request.TerceroId, ct);
        if (validationError is not null)
            return Result.Failure<long>(validationError);

        var descuentoValidationError = await ClienteDescuentoMaximoValidator.ValidateAsync(
            db,
            request.TerceroId,
            request.Items.Select(x => x.DescuentoPct),
            ct);

        if (descuentoValidationError is not null)
            return Result.Failure<long>(descuentoValidationError);

        var origen = request.ComprobanteOrigenId.HasValue
            ? await db.Comprobantes
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == request.ComprobanteOrigenId.Value && !x.IsDeleted, ct)
            : null;

        if (request.ComprobanteOrigenId.HasValue && origen is null)
            return Result.Failure<long>($"No se encontró el comprobante origen ID {request.ComprobanteOrigenId.Value}.");

        var notaDebitoError = await notaDebitoValidationService.ValidateNotaDebitoAgainstFacturaAsync(
            request.ComprobanteOrigenId,
            request.TerceroId,
            request.MonedaId,
            request.Items.Select(x => new ValidacionItemND(
                x.ItemId,
                x.ComprobanteItemOrigenId,
                x.CantidadDocumentoOrigen,
                x.PrecioDocumentoOrigen)).ToList(),
            ct);

        if (notaDebitoError is not null)
            return Result.Failure<long>(notaDebitoError);

        short prefijo = 1;
        long numero = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (request.PuntoFacturacionId.HasValue)
        {
            var punto = await db.PuntosFacturacion
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.PuntoFacturacionId.Value, ct);

            if (punto is null)
                return Result.Failure<long>($"No se encontró el punto de facturación ID {request.PuntoFacturacionId.Value}.");

            prefijo = punto.Numero;
            numero = await comprobanteRepo.GetProximoNumeroAsync(
                request.PuntoFacturacionId.Value,
                request.TipoComprobanteId,
                ct);
        }

        var comprobante = Comprobante.Crear(
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            prefijo,
            numero,
            request.Fecha,
            request.FechaVencimiento,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            currentUser.UserId);

        comprobante.SetMotivoDebito(motivo.Id, request.MotivoDebitoObservacion, currentUser.UserId);

        if (request.ComprobanteOrigenId.HasValue)
            comprobante.VincularAComprobanteOrigen(request.ComprobanteOrigenId.Value, currentUser.UserId);

        if (origen is not null)
        {
            comprobante.SetDatosComerciales(
                request.VendedorId ?? origen.VendedorId,
                origen.CobradorId,
                origen.ZonaComercialId,
                request.ListaPreciosId ?? origen.ListaPreciosId,
                request.CondicionPagoId ?? origen.CondicionPagoId,
                request.PlazoDias ?? origen.PlazoDias,
                request.CanalVentaId ?? origen.CanalVentaId,
                origen.PorcentajeComisionVendedor,
                origen.PorcentajeComisionCobrador,
                currentUser.UserId);

            comprobante.SetSnapshotsTercero(
                origen.TerceroRazonSocialSnapshot,
                origen.TerceroCondicionIvaSnapshot,
                origen.TerceroDomicilioSnapshot,
                currentUser.UserId);
        }
        else
        {
            var zonaComercialId = await db.TercerosPerfilesComerciales
                .AsNoTracking()
                .Where(x => x.TerceroId == request.TerceroId)
                .Select(x => x.ZonaComercialId)
                .FirstOrDefaultAsync(ct);

            comprobante.SetDatosComerciales(
                request.VendedorId,
                null,
                zonaComercialId,
                request.ListaPreciosId,
                request.CondicionPagoId,
                request.PlazoDias,
                request.CanalVentaId,
                null,
                null,
                currentUser.UserId);
        }

        var alicuotaIds = request.Items.Select(x => x.AlicuotaIvaId).Distinct().ToList();
        var alicuotas = await db.AlicuotasIva
            .AsNoTracking()
            .Where(x => alicuotaIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var itemIds = request.Items.Select(x => x.ItemId).Distinct().ToList();
        var itemsDb = await db.Items
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var itemsOrigen = origen?.Items.ToDictionary(x => x.Id) ?? new Dictionary<long, ComprobanteItem>();

        short orden = 0;
        foreach (var itemInput in request.Items.OrderBy(x => x.Orden))
        {
            if (!alicuotas.TryGetValue(itemInput.AlicuotaIvaId, out var alicuota))
                return Result.Failure<long>($"No se encontró la alícuota de IVA ID {itemInput.AlicuotaIvaId}.");

            if (!itemsDb.TryGetValue(itemInput.ItemId, out var itemDb))
                return Result.Failure<long>($"No se encontró el ítem ID {itemInput.ItemId}.");

            itemsOrigen.TryGetValue(itemInput.ComprobanteItemOrigenId ?? 0, out var itemOrigen);

            var linea = ComprobanteItem.Crear(
                0,
                itemInput.ItemId,
                itemInput.Descripcion ?? itemDb.Descripcion,
                itemInput.Cantidad,
                itemInput.CantidadBonificada,
                itemInput.PrecioUnitario,
                itemInput.DescuentoPct,
                itemInput.AlicuotaIvaId,
                alicuota.Porcentaje,
                itemInput.DepositoId,
                orden++,
                true,
                itemInput.Lote,
                itemInput.Serie,
                itemInput.FechaVencimiento,
                itemInput.UnidadMedidaId,
                itemInput.ObservacionRenglon,
                itemInput.PrecioListaOriginal,
                itemInput.ComisionVendedorRenglon,
                itemInput.ComprobanteItemOrigenId);

            linea.SetDatosDocumentoOrigen(
                itemInput.CantidadDocumentoOrigen ?? itemOrigen?.Cantidad,
                itemInput.PrecioDocumentoOrigen ?? itemOrigen?.PrecioUnitario);

            comprobante.AgregarItem(linea);
        }

        if (request.Percepciones > 0)
            comprobante.SetPercepciones(request.Percepciones, currentUser.UserId);

        await comprobanteRepo.AddAsync(comprobante, ct);
        await uow.SaveChangesAsync(ct);

        if (request.Emitir)
        {
            comprobante.Emitir(currentUser.UserId);

            await circuitoVentas.AplicarEfectosAsync(
                comprobante,
                tipo,
                OperacionStockVenta.Ninguna,
                motivo.AfectaCuentaCorriente ? OperacionCuentaCorrienteVenta.Debito : OperacionCuentaCorrienteVenta.Ninguna,
                ct);

            await uow.SaveChangesAsync(ct);
        }

        return Result.Success(comprobante.Id);
    }
}
