using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Services;

public sealed class EmitirDocumentoVentaValidationService(
    IApplicationDbContext db,
    TerceroOperacionValidationService terceroOperacionValidationService,
    ItemCommercialStockService itemCommercialStockService)
{
    public async Task<(string? Error, Dictionary<long, Item> ItemsById)> ValidateAsync(
        Comprobante comprobante,
        TipoComprobante tipo,
        EmitirDocumentoVentaCommand request,
        CancellationToken ct)
    {
        var validationError = await terceroOperacionValidationService.ValidateClienteAsync(comprobante.TerceroId, ct);
        if (validationError is not null)
            return (validationError, []);

        var cantidadesSolicitadas = comprobante.Items
            .GroupBy(x => x.ItemId)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(v => Math.Max(0m, v.Cantidad - v.CantidadBonificada)));

        var itemsById = await VentaItemValidationHelper.LoadItemsByIdAsync(db, cantidadesSolicitadas.Keys.ToList(), ct);

        var itemValidationError = VentaItemValidationHelper.ValidateItemsVendibles(itemsById, cantidadesSolicitadas);
        if (itemValidationError is not null)
            return (itemValidationError, itemsById);

        var esPedido = PedidoWorkflowRules.EsPedido(tipo.Codigo, tipo.Descripcion);
        var esRemito = PedidoWorkflowRules.EsRemito(tipo.Codigo, tipo.Descripcion, tipo.AfectaStock);
        var requiereValidarStock = request.OperacionStock == OperacionStockVenta.Egreso || esPedido || esRemito;

        if (requiereValidarStock)
        {
            var stockValidationError = await VentaItemValidationHelper.ValidateStockDisponibleAsync(
                itemCommercialStockService,
                itemsById,
                cantidadesSolicitadas,
                tipo.AfectaStock ? null : cantidadesSolicitadas,
                ct);

            if (stockValidationError is not null)
                return (stockValidationError, itemsById);
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

                return ($"El remito valorizado requiere precio mayor a cero para el ítem '{descripcion}'.", itemsById);
            }
        }

        return (null, itemsById);
    }
}
