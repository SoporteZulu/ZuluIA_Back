using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Services;

public class NotaDebitoValidationService(IApplicationDbContext db)
{
    /// <summary>
    /// Valida la coherencia comercial básica de una nota de débito contra su comprobante origen.
    /// </summary>
    public async Task<string?> ValidateNotaDebitoAgainstFacturaAsync(
        long? facturaOrigenId,
        long terceroId,
        long monedaId,
        IReadOnlyList<ValidacionItemND> items,
        CancellationToken ct)
    {
        if (!facturaOrigenId.HasValue)
            return null;

        var factura = await db.Comprobantes
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == facturaOrigenId.Value, ct);

        if (factura is null)
            return "No se encontró la factura origen especificada.";

        var tipoFactura = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == factura.TipoComprobanteId, ct);

        if (tipoFactura is null || !tipoFactura.EsVenta)
            return "El comprobante origen no es un comprobante de venta válido.";

        if (factura.Estado == EstadoComprobante.Borrador)
            return "No se puede crear una nota de débito sobre un comprobante en estado borrador.";

        if (factura.Estado == EstadoComprobante.Anulado)
            return "No se puede crear una nota de débito sobre un comprobante anulado.";

        if (factura.TerceroId != terceroId)
            return "El cliente de la nota de débito no coincide con el del comprobante origen.";

        if (factura.MonedaId != monedaId)
            return "La moneda de la nota de débito no coincide con la del comprobante origen.";

        if (items.Count == 0)
            return null;

        var itemsFactura = factura.Items.ToDictionary(x => x.Id);

        foreach (var item in items)
        {
            if (!item.ComprobanteItemOrigenId.HasValue)
                continue;

            if (!itemsFactura.TryGetValue(item.ComprobanteItemOrigenId.Value, out var itemOrigen))
                return $"El renglón origen ID {item.ComprobanteItemOrigenId.Value} no existe en el comprobante origen.";

            if (item.ItemId > 0 && itemOrigen.ItemId != item.ItemId)
                return $"El item ID {item.ItemId} no coincide con el renglón origen seleccionado.";

            if (item.CantidadDocumentoOrigen.HasValue && item.CantidadDocumentoOrigen.Value > itemOrigen.Cantidad)
                return $"La cantidad referenciada del item '{itemOrigen.Descripcion}' excede la cantidad del comprobante origen.";

            if (item.PrecioDocumentoOrigen.HasValue && item.PrecioDocumentoOrigen.Value < 0)
                return $"El precio de referencia del item '{itemOrigen.Descripcion}' no puede ser negativo.";
        }

        return null;
    }
}

public record ValidacionItemND(
    long ItemId,
    long? ComprobanteItemOrigenId,
    decimal? CantidadDocumentoOrigen,
    decimal? PrecioDocumentoOrigen);
