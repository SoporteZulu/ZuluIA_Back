using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public class GetComprobanteDetalleQueryHandler(
    IComprobanteRepository repo,
    IImputacionRepository imputRepo,
    IApplicationDbContext db)
    : IRequestHandler<GetComprobanteDetalleQuery, ComprobanteDetalleDto?>
{
    public async Task<ComprobanteDetalleDto?> Handle(
        GetComprobanteDetalleQuery request,
        CancellationToken ct)
    {
        var comp = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (comp is null) return null;

        // Lookups paralelos
        var terceroTask = db.Terceros.AsNoTracking()
            .Where(x => x.Id == comp.TerceroId)
            .Select(x => new { x.RazonSocial, x.NroDocumento })
            .FirstOrDefaultAsync(ct);

        var tipoTask = db.TiposComprobante.AsNoTracking()
            .Where(x => x.Id == comp.TipoComprobanteId)
            .Select(x => new { x.Descripcion, x.Codigo })
            .FirstOrDefaultAsync(ct);

        var monedaTask = db.Monedas.AsNoTracking()
            .Where(x => x.Id == comp.MonedaId)
            .Select(x => new { x.Simbolo })
            .FirstOrDefaultAsync(ct);

        var sucursalTask = db.Sucursales.AsNoTracking()
            .Where(x => x.Id == comp.SucursalId)
            .Select(x => new { x.RazonSocial })
            .FirstOrDefaultAsync(ct);

        await Task.WhenAll(terceroTask, tipoTask, monedaTask, sucursalTask);

        var tercero = await terceroTask;
        var tipo = await tipoTask;
        var moneda = await monedaTask;
        var sucursal = await sucursalTask;

        // Condición IVA del tercero
        var condIva = await db.CondicionesIva
            .AsNoTracking()
            .Join(db.Terceros.AsNoTracking().Where(t => t.Id == comp.TerceroId),
                c => c.Id,
                t => t.CondicionIvaId,
                (c, _) => c.Descripcion)
            .FirstOrDefaultAsync(ct);

        // Ítems con descripciones
        var itemIds = comp.Items.Select(x => x.ItemId).Distinct().ToList();
        var depositoIds = comp.Items.Where(x => x.DepositoId.HasValue)
                              .Select(x => x.DepositoId!.Value).Distinct().ToList();

        var items = await db.Items.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo })
            .ToDictionaryAsync(x => x.Id, ct);

        var depositos = await db.Depositos.AsNoTracking()
            .Where(x => depositoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        // Imputaciones
        var imputaciones = await imputRepo.GetByComprobanteDestinoAsync(comp.Id, ct);
        var origenIds = imputaciones.Select(x => x.ComprobanteOrigenId).ToList();

        var numerosOrigen = await db.Comprobantes.AsNoTracking()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Numero.Prefijo, x.Numero.Numero })
            .ToDictionaryAsync(x => x.Id, ct);

        var imputDtos = imputaciones.Select(i => new ImputacionDto
        {
            Id                   = i.Id,
            ComprobanteOrigenId  = i.ComprobanteOrigenId,
            NumeroOrigen         = numerosOrigen.ContainsKey(i.ComprobanteOrigenId)
                ? $"{numerosOrigen[i.ComprobanteOrigenId].Prefijo:D4}-{numerosOrigen[i.ComprobanteOrigenId].Numero:D8}"
                : "—",
            ComprobanteDestinoId = i.ComprobanteDestinoId,
            NumeroDestino        = comp.Numero.Formateado,
            Importe              = i.Importe,
            Fecha                = i.Fecha,
            CreatedAt            = i.CreatedAt
        }).ToList();

        return new ComprobanteDetalleDto
        {
            Id                         = comp.Id,
            SucursalId                 = comp.SucursalId,
            SucursalRazonSocial        = sucursal?.RazonSocial ?? "—",
            PuntoFacturacionId         = comp.PuntoFacturacionId,
            TipoComprobanteId          = comp.TipoComprobanteId,
            TipoComprobanteDescripcion = tipo?.Descripcion ?? "—",
            TipoComprobanteCodigo      = tipo?.Codigo,
            Prefijo                    = comp.Numero.Prefijo,
            Numero                     = comp.Numero.Numero,
            NumeroFormateado           = comp.Numero.Formateado,
            Fecha                      = comp.Fecha,
            FechaVencimiento           = comp.FechaVencimiento,
            TerceroId                  = comp.TerceroId,
            TerceroRazonSocial         = tercero?.RazonSocial ?? "—",
            TerceroCuit                = tercero?.NroDocumento ?? "—",
            TerceroCondicionIva        = condIva ?? "—",
            MonedaId                   = comp.MonedaId,
            MonedaSimbolo              = moneda?.Simbolo ?? "$",
            Cotizacion                 = comp.Cotizacion,
            Subtotal                   = comp.Subtotal,
            DescuentoImporte           = comp.DescuentoImporte,
            NetoGravado                = comp.NetoGravado,
            NetoNoGravado              = comp.NetoNoGravado,
            IvaRi                      = comp.IvaRi,
            IvaRni                     = comp.IvaRni,
            Percepciones               = comp.Percepciones,
            Retenciones                = comp.Retenciones,
            Total                      = comp.Total,
            Saldo                      = comp.Saldo,
            Cae                        = comp.Cae,
            FechaVtoCae                = comp.FechaVtoCae,
            QrData                     = comp.QrData,
            Estado                     = comp.Estado.ToString().ToUpperInvariant(),
            Observacion                = comp.Observacion,
            CreatedAt                  = comp.CreatedAt,
            UpdatedAt                  = comp.UpdatedAt,
            Items = comp.Items.OrderBy(x => x.Orden).Select(i => new ComprobanteItemDto
            {
                Id                 = i.Id,
                ItemId             = i.ItemId,
                ItemCodigo         = items.GetValueOrDefault(i.ItemId)?.Codigo ?? "—",
                Descripcion        = i.Descripcion,
                Cantidad           = i.Cantidad,
                CantidadBonificada = i.CantidadBonificada,
                PrecioUnitario     = i.PrecioUnitario,
                DescuentoPct       = i.DescuentoPct,
                AlicuotaIvaId      = i.AlicuotaIvaId,
                PorcentajeIva      = i.PorcentajeIva,
                SubtotalNeto       = i.SubtotalNeto,
                IvaImporte         = i.IvaImporte,
                TotalLinea         = i.TotalLinea,
                DepositoId         = i.DepositoId,
                DepositoDescripcion = i.DepositoId.HasValue
                    ? depositos.GetValueOrDefault(i.DepositoId.Value)?.Descripcion
                    : null,
                Orden              = i.Orden,
                EsGravado          = i.EsGravado
            }).ToList().AsReadOnly(),
            Imputaciones = imputDtos.AsReadOnly()
        };
    }
}