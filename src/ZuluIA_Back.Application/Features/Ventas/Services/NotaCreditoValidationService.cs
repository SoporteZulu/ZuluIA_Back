using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Services;

public class NotaCreditoValidationService(IApplicationDbContext db)
{
    public async Task<string?> ValidateNotaCreditoAgainstFacturaAsync(
        long? facturaOrigenId,
        long terceroId,
        long monedaId,
        decimal totalNC,
        IReadOnlyList<ValidacionItemNC> items,
        CancellationToken ct)
    {
        if (!facturaOrigenId.HasValue)
            return null; // NC manual, no requiere validación contra factura

        var factura = await db.Comprobantes
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == facturaOrigenId.Value, ct);

        if (factura is null)
            return "No se encontró la factura origen especificada.";

        // Validar tipo de comprobante origen
        var tipoFactura = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == factura.TipoComprobanteId, ct);

        if (tipoFactura is null || !tipoFactura.EsVenta)
            return "El comprobante origen no es una factura de venta válida.";

        // Validar estado de la factura
        if (factura.Estado == EstadoComprobante.Borrador)
            return "No se puede crear una nota de crédito sobre una factura en estado borrador.";

        if (factura.Estado == EstadoComprobante.Anulado)
            return "No se puede crear una nota de crédito sobre una factura anulada.";

        // Validar tercero coincidente
        if (factura.TerceroId != terceroId)
            return "El cliente de la nota de crédito no coincide con el de la factura origen.";

        // Validar moneda coincidente
        if (factura.MonedaId != monedaId)
            return "La moneda de la nota de crédito no coincide con la de la factura origen.";

        // Validar monto total no excede factura
        var totalNotasCreditoExistentes = await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.ComprobanteOrigenId == facturaOrigenId.Value
                && x.Estado != EstadoComprobante.Anulado)
            .SumAsync(x => x.Total, ct);

        var totalDisponible = factura.Total - totalNotasCreditoExistentes;
        
        if (totalNC > totalDisponible)
            return $"El monto de la nota de crédito ({totalNC:C}) excede el monto disponible de la factura ({totalDisponible:C}).";

        // Validar items y cantidades
        if (items.Any())
        {
            var itemsFactura = factura.Items.ToDictionary(x => x.ItemId);
            
            foreach (var itemNC in items)
            {
                if (!itemsFactura.TryGetValue(itemNC.ItemId, out var itemFactura))
                    return $"El item ID {itemNC.ItemId} no existe en la factura origen.";

                // Obtener cantidad ya devuelta en otras NC
                var cantidadDevueltaAnteriormente = await db.Comprobantes
                    .AsNoTracking()
                    .Where(x => x.ComprobanteOrigenId == facturaOrigenId.Value
                        && x.Estado != EstadoComprobante.Anulado)
                    .SelectMany(x => x.Items)
                    .Where(x => x.ItemId == itemNC.ItemId)
                    .SumAsync(x => x.Cantidad, ct);

                var cantidadDisponible = itemFactura.Cantidad - cantidadDevueltaAnteriormente;

                if (itemNC.Cantidad > cantidadDisponible)
                    return $"La cantidad del item '{itemFactura.Descripcion}' ({itemNC.Cantidad}) excede la cantidad disponible ({cantidadDisponible}).";
            }
        }

        return null; // Validación exitosa
    }

    public async Task<bool> RequiereFiscalizacionAsync(long tipoComprobanteId, CancellationToken ct)
    {
        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tipoComprobanteId, ct);

        return tipo?.TipoAfip.HasValue ?? false;
    }

    public async Task<decimal> GetSaldoDisponibleFacturaAsync(long facturaId, CancellationToken ct)
    {
        var factura = await db.Comprobantes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == facturaId, ct);

        if (factura is null)
            return 0;

        var totalNotasCreditoExistentes = await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.ComprobanteOrigenId == facturaId
                && x.Estado != EstadoComprobante.Anulado)
            .SumAsync(x => x.Total, ct);

        return factura.Total - totalNotasCreditoExistentes;
    }
}

public record ValidacionItemNC(
    long ItemId,
    decimal Cantidad
);
