using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class EmitirDocumentoVentaCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    CircuitoVentasService circuitoVentas,
    TerceroOperacionValidationService terceroOperacionValidationService,
    ISender sender,
    ItemCommercialStockService itemCommercialStockService)
    : IRequestHandler<EmitirDocumentoVentaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(EmitirDocumentoVentaCommand request, CancellationToken ct)
    {
        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(request.ComprobanteId, ct);
        if (comprobante is null)
            return Result.Failure<long>($"No se encontró el comprobante ID {request.ComprobanteId}.");

        if (comprobante.Estado != EstadoComprobante.Borrador)
            return Result.Failure<long>($"Solo se pueden emitir documentos en borrador. Estado actual: {comprobante.Estado}.");

        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TipoComprobanteId, ct);

        if (tipo is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {comprobante.TipoComprobanteId}.");

        if (!tipo.EsVenta)
            return Result.Failure<long>("El comprobante no pertenece al circuito de ventas.");

        var validationError = await terceroOperacionValidationService.ValidateClienteAsync(comprobante.TerceroId, ct);
        if (validationError is not null)
            return Result.Failure<long>(validationError);

        var cantidadesSolicitadas = comprobante.Items
            .GroupBy(x => x.ItemId)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(v => Math.Max(0m, v.Cantidad - v.CantidadBonificada)));

        var itemsById = await VentaItemValidationHelper.LoadItemsByIdAsync(db, cantidadesSolicitadas.Keys.ToList(), ct);

        var itemValidationError = VentaItemValidationHelper.ValidateItemsVendibles(itemsById, cantidadesSolicitadas);
        if (itemValidationError is not null)
            return Result.Failure<long>(itemValidationError);

        var esPedido = PedidoWorkflowRules.EsPedido(tipo.Codigo, tipo.Descripcion);
        var esRemito = PedidoWorkflowRules.EsRemito(tipo.Codigo, tipo.Descripcion, tipo.AfectaStock);

        var requiereValidarStock = request.OperacionStock == OperacionStockVenta.Egreso
            || esPedido
            || esRemito;

        if (requiereValidarStock)
        {
            var stockValidationError = await VentaItemValidationHelper.ValidateStockDisponibleAsync(
                itemCommercialStockService,
                itemsById,
                cantidadesSolicitadas,
                tipo.AfectaStock ? null : cantidadesSolicitadas,
                ct);

            if (stockValidationError is not null)
                return Result.Failure<long>(stockValidationError);
        }

        if (esRemito && request.OperacionCuentaCorriente == OperacionCuentaCorrienteVenta.Debito)
        {
            var itemSinPrecio = comprobante.Items
                .FirstOrDefault(x => x.Cantidad - x.CantidadBonificada > 0 && x.PrecioUnitario <= 0);

            if (itemSinPrecio is not null)
            {
                var item = itemsById.GetValueOrDefault(itemSinPrecio.ItemId);
                var descripcion = item is null
                    ? itemSinPrecio.Descripcion
                    : $"{item.Codigo} - {item.Descripcion}";

                return Result.Failure<long>($"El remito valorizado requiere precio mayor a cero para el ítem '{descripcion}'.");
            }
        }

        comprobante.Emitir(currentUser.UserId);
        comprobanteRepo.Update(comprobante);

        await circuitoVentas.AplicarEfectosAsync(
            comprobante,
            tipo,
            request.OperacionStock,
            request.OperacionCuentaCorriente,
            ct);

        await uow.SaveChangesAsync(ct);

        if (comprobante.ComprobanteOrigenId.HasValue
            && PedidoWorkflowRules.EsRemito(tipo.Codigo, tipo.Descripcion, tipo.AfectaStock))
        {
            var origen = await db.Comprobantes
                .AsNoTracking()
                .Where(x => x.Id == comprobante.ComprobanteOrigenId.Value)
                .Select(x => new { x.Id, x.TipoComprobanteId, x.EstadoPedido, x.FechaEntregaCompromiso })
                .FirstOrDefaultAsync(ct);

            if (origen is not null)
            {
                var tipoOrigen = await db.TiposComprobante
                    .AsNoTracking()
                    .Where(x => x.Id == origen.TipoComprobanteId)
                    .Select(x => new { x.Codigo, x.Descripcion })
                    .FirstOrDefaultAsync(ct);

                if (origen.EstadoPedido.HasValue
                    || origen.FechaEntregaCompromiso.HasValue
                    || (tipoOrigen is not null && PedidoWorkflowRules.EsPedido(tipoOrigen.Codigo, tipoOrigen.Descripcion)))
                {
                    await sender.Send(new ActualizarCumplimientoPedidoCommand(origen.Id), ct);
                }
            }
        }

        return Result.Success(comprobante.Id);
    }
}
