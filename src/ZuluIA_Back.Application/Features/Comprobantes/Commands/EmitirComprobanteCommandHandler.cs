using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Services;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class EmitirComprobanteCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IPeriodoIvaRepository periodoRepo,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db,
    IServiceProvider serviceProvider)
    : IRequestHandler<EmitirComprobanteCommand, Result<long>>
{
    private StockService stockService => serviceProvider.GetRequiredService<StockService>();
    private IAfipCaeComprobanteService afipCaeComprobanteService => serviceProvider.GetRequiredService<IAfipCaeComprobanteService>();
    private TerceroOperacionValidationService terceroOperacionValidationService => serviceProvider.GetRequiredService<TerceroOperacionValidationService>();
    private ItemCommercialStockService itemCommercialStockService => serviceProvider.GetRequiredService<ItemCommercialStockService>();

    public async Task<Result<long>> Handle(
        EmitirComprobanteCommand request,
        CancellationToken ct)
    {
        // 1. Validar período IVA abierto
        var periodoAbierto = await periodoRepo.EstaAbiertoPeriodoAsync(
            request.SucursalId, request.Fecha, ct);

        if (!periodoAbierto)
            return Result.Failure<long>(
                $"El período IVA para {request.Fecha:yyyy-MM} está cerrado.");

        // 2. Validar ítems
        if (!request.Items.Any())
            return Result.Failure<long>(
                "El comprobante debe tener al menos un ítem.");

        // 3. Obtener tipo de comprobante
        var tipoComp = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TipoComprobanteId, ct);

        if (tipoComp is null)
            return Result.Failure<long>(
                $"No se encontró el tipo de comprobante ID {request.TipoComprobanteId}.");

        if (tipoComp.EsVenta)
        {
            var validationError = await terceroOperacionValidationService.ValidateClienteAsync(request.TerceroId, ct);
            if (validationError is not null)
                return Result.Failure<long>(validationError);

            var cantidadesSolicitadas = request.Items
                .GroupBy(x => x.ItemId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Sum(v => Math.Max(0m, v.Cantidad - v.CantidadBonificada)));

            var descuentoValidationError = await ClienteDescuentoMaximoValidator.ValidateAsync(
                db,
                request.TerceroId,
                request.Items.Select(x => x.DescuentoPct),
                ct);

            if (descuentoValidationError is not null)
                return Result.Failure<long>(descuentoValidationError);

            var itemsById = await VentaItemValidationHelper.LoadItemsByIdAsync(db, cantidadesSolicitadas.Keys.ToList(), ct);

            var itemValidationError = VentaItemValidationHelper.ValidateItemsVendibles(itemsById, cantidadesSolicitadas);
            if (itemValidationError is not null)
                return Result.Failure<long>(itemValidationError);

            if (request.AfectaStock && tipoComp.AfectaStock)
            {
                var stockValidationError = await VentaItemValidationHelper.ValidateStockDisponibleAsync(
                    itemCommercialStockService,
                    itemsById,
                    cantidadesSolicitadas,
                    null,
                    ct);

                if (stockValidationError is not null)
                    return Result.Failure<long>(stockValidationError);
            }
        }

        if (tipoComp.EsCompra)
        {
            var validationError = await terceroOperacionValidationService.ValidateProveedorAsync(request.TerceroId, ct);
            if (validationError is not null)
                return Result.Failure<long>(validationError);
        }

        // 4. Obtener próximo número
        short prefijo = 1;
        long numero = 1;

        if (request.PuntoFacturacionId.HasValue)
        {
            var punto = await db.PuntosFacturacion
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.PuntoFacturacionId.Value, ct);

            if (punto is null)
                return Result.Failure<long>(
                    $"No se encontró el punto de facturación ID {request.PuntoFacturacionId}.");

            prefijo = punto.Numero;
            numero  = await comprobanteRepo.GetProximoNumeroAsync(
                request.PuntoFacturacionId.Value,
                request.TipoComprobanteId, ct);
        }

        var timbradoValidation = await ParaguayTimbradoResolver.ValidarAsync(
            db,
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Fecha,
            numero,
            ct);
        if (timbradoValidation.RequiereTimbrado && !timbradoValidation.EsValido)
            return Result.Failure<long>(timbradoValidation.Mensaje!);

        // 5. Crear comprobante
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

        // 6. Agregar ítems
        var alicuotaIds = request.Items.Select(x => x.AlicuotaIvaId).Distinct().ToList();
        var alicuotas = await db.AlicuotasIva
            .AsNoTracking()
            .Where(x => alicuotaIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        short orden = 0;
        foreach (var itemInput in request.Items)
        {
            if (!alicuotas.TryGetValue(itemInput.AlicuotaIvaId, out var alicuota))
                return Result.Failure<long>(
                    $"No se encontró la alícuota de IVA ID {itemInput.AlicuotaIvaId}.");

            var itemDb = await db.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == itemInput.ItemId, ct);

            if (itemDb is null)
                return Result.Failure<long>(
                    $"No se encontró el ítem ID {itemInput.ItemId}.");

            var descripcion = itemInput.Descripcion ?? itemDb.Descripcion;

            var linea = ComprobanteItem.Crear(
                0, // Se asigna al persistir
                itemInput.ItemId,
                descripcion,
                itemInput.Cantidad,
                itemInput.CantidadBonificada,
                itemInput.PrecioUnitario,
                itemInput.DescuentoPct,
                itemInput.AlicuotaIvaId,
                alicuota.Porcentaje,
                itemInput.DepositoId,
                orden++);

            comprobante.AgregarItem(linea);
        }

        // 7. Percepciones
        if (request.Percepciones > 0)
            comprobante.SetPercepciones(request.Percepciones, currentUser.UserId);

        // 8. Emitir
        comprobante.Emitir(currentUser.UserId);

        if (timbradoValidation.TimbradoId.HasValue && !string.IsNullOrWhiteSpace(timbradoValidation.NroTimbrado))
        {
            comprobante.AsignarTimbrado(
                timbradoValidation.TimbradoId.Value,
                timbradoValidation.NroTimbrado,
                currentUser.UserId);
        }

        if (!string.IsNullOrWhiteSpace(request.Cae))
        {
            if (!request.FechaVtoCae.HasValue)
                return Result.Failure<long>("Debe indicar FechaVtoCae cuando informa un CAE manual.");

            comprobante.AsignarCae(request.Cae, request.FechaVtoCae.Value, null, currentUser.UserId);
        }

        // 9. Persistir
        await comprobanteRepo.AddAsync(comprobante, ct);

        foreach (var impuesto in BuildImpuestos(comprobante))
            db.ComprobantesImpuestos.Add(impuesto);

        foreach (var tributo in await ComprobanteTributoResolver.ResolveAsync(db, comprobante, ct))
            db.ComprobantesTributos.Add(tributo);

        var requiereCaeAutomatico = await RequiereCaeAutomaticoAsync(request, tipoComp, ct);
        if (requiereCaeAutomatico && string.IsNullOrWhiteSpace(comprobante.Cae))
        {
            await uow.SaveChangesAsync(ct);

            var caeResult = await afipCaeComprobanteService.SolicitarYAsignarAsync(comprobante, ct);
            if (caeResult.IsFailure)
                return Result.Failure<long>(caeResult.Error ?? "No se pudo emitir CAE automaticamente en AFIP.");
        }

        // 10. Afectar stock si corresponde
        if (request.AfectaStock && tipoComp.AfectaStock)
        {
            foreach (var itemInput in request.Items)
            {
                var depositoId = itemInput.DepositoId;
                if (!depositoId.HasValue)
                {
                    var defDeposito = await db.Depositos
                        .AsNoTracking()
                        .Where(x => x.SucursalId == request.SucursalId &&
                                    x.EsDefault  == true &&
                                    x.Activo     == true)
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync(ct);

                    depositoId = defDeposito == 0 ? null : defDeposito;
                }

                if (!depositoId.HasValue) continue;

                var tipoMov = tipoComp.EsVenta
                    ? TipoMovimientoStock.VentaDespacho
                    : TipoMovimientoStock.CompraRecepcion;

                if (tipoComp.EsVenta)
                    await stockService.EgresarAsync(
                        itemInput.ItemId, depositoId.Value,
                        itemInput.Cantidad - itemInput.CantidadBonificada,
                        tipoMov, "comprobantes", 0,
                        null, currentUser.UserId, false, ct);
                else
                    await stockService.IngresarAsync(
                        itemInput.ItemId, depositoId.Value,
                        itemInput.Cantidad - itemInput.CantidadBonificada,
                        tipoMov, "comprobantes", 0,
                        null, currentUser.UserId, ct);
            }
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success(comprobante.Id);
    }

    private async Task<bool> RequiereCaeAutomaticoAsync(
        EmitirComprobanteCommand request,
        ZuluIA_Back.Domain.Entities.Referencia.TipoComprobante tipoComp,
        CancellationToken ct)
    {
        if (!request.PuntoFacturacionId.HasValue ||
            !tipoComp.EsVenta ||
            !tipoComp.TipoAfip.HasValue)
        {
            return false;
        }

        return await db.ConfiguracionesFiscales
            .AsNoTracking()
            .AnyAsync(
                x => x.PuntoFacturacionId == request.PuntoFacturacionId.Value
                     && x.TipoComprobanteId == request.TipoComprobanteId
                     && x.Online,
                ct);
    }

    private static IReadOnlyCollection<ComprobanteImpuesto> BuildImpuestos(Comprobante comprobante)
    {
        return comprobante.Items
            .Where(x => x.IvaImporte > 0)
            .GroupBy(x => new { x.AlicuotaIvaId, x.PorcentajeIva })
            .Select(group => ComprobanteImpuesto.Crear(
                comprobante.Id,
                group.Key.AlicuotaIvaId,
                group.Key.PorcentajeIva,
                Math.Round(group.Sum(x => x.SubtotalNeto), 2),
                Math.Round(group.Sum(x => x.IvaImporte), 2)))
            .ToList();
    }

}
