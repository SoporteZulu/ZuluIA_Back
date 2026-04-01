using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Application.Features.Ventas.DTOs;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public class GetPedidoVinculacionesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPedidoVinculacionesQuery, PedidoVinculacionesDto?>
{
    public async Task<PedidoVinculacionesDto?> Handle(GetPedidoVinculacionesQuery request, CancellationToken ct)
    {
        var pedido = await db.Comprobantes
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.PedidoId, ct);

        if (pedido is null)
            return null;

        var tipoPedido = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => x.Id == pedido.TipoComprobanteId)
            .Select(x => new { x.Codigo, x.Descripcion })
            .FirstOrDefaultAsync(ct);

        if (tipoPedido is null || !PedidoWorkflowRules.EsPedido(tipoPedido.Codigo, tipoPedido.Descripcion))
            return null;

        var hijosDirectos = await db.Comprobantes
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.ComprobanteOrigenId == pedido.Id && x.Estado != EstadoComprobante.Anulado)
            .OrderBy(x => x.Fecha)
            .ToListAsync(ct);

        var hijosDirectosIds = hijosDirectos.Select(x => x.Id).ToList();
        var nietos = hijosDirectosIds.Count == 0
            ? []
            : await db.Comprobantes
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.ComprobanteOrigenId.HasValue
                    && hijosDirectosIds.Contains(x.ComprobanteOrigenId.Value)
                    && x.Estado != EstadoComprobante.Anulado)
                .OrderBy(x => x.Fecha)
                .ToListAsync(ct);

        var relacionados = hijosDirectos.Concat(nietos).ToList();
        var tipoIds = relacionados.Select(x => x.TipoComprobanteId).Append(pedido.TipoComprobanteId).Distinct().ToList();
        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.AfectaStock })
            .ToDictionaryAsync(x => x.Id, ct);

        var remitos = relacionados
            .Where(x => tipos.TryGetValue(x.TipoComprobanteId, out var tipo)
                && PedidoWorkflowRules.EsRemito(tipo.Codigo, tipo.Descripcion, tipo.AfectaStock))
            .ToList();

        var facturas = relacionados
            .Where(x => tipos.TryGetValue(x.TipoComprobanteId, out var tipo)
                && PedidoWorkflowRules.EsFactura(tipo.Codigo, tipo.Descripcion))
            .ToList();

        var itemIds = pedido.Items.Select(x => x.ItemId).Distinct().ToList();
        var items = await db.Items
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var entregas = remitos
            .SelectMany(remito => remito.Items.Select(item => new EntregaProjection(
                remito.Id,
                remito.Numero.Formateado,
                remito.Fecha,
                item.ItemId,
                item.ComprobanteItemOrigenId,
                item.Cantidad - item.CantidadBonificada)))
            .Where(x => x.CantidadEntregada > 0)
            .ToList();

        var hayReferenciasExplicitas = entregas.Any(x => x.ComprobanteItemOrigenId.HasValue);
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var cumplimiento = pedido.Items
            .OrderBy(x => x.Orden)
            .Select(itemPedido =>
            {
                var entregasDelItem = entregas
                    .Where(x => x.ComprobanteItemOrigenId == itemPedido.Id)
                    .ToList();

                if (entregasDelItem.Count == 0 && !hayReferenciasExplicitas)
                {
                    entregasDelItem = entregas
                        .Where(x => x.ItemId == itemPedido.ItemId)
                        .ToList();
                }

                var cantidadEntregada = entregasDelItem.Sum(x => x.CantidadEntregada);
                var diferencia = itemPedido.Cantidad - cantidadEntregada;
                var estadoEntrega = itemPedido.EstadoEntrega
                    ?? (cantidadEntregada <= 0
                        ? EstadoEntregaItem.NoEntregado
                        : cantidadEntregada < itemPedido.Cantidad
                            ? EstadoEntregaItem.EntregaParcial
                            : cantidadEntregada == itemPedido.Cantidad
                                ? EstadoEntregaItem.EntregaCompleta
                                : EstadoEntregaItem.EntregaSobrepasada);
                var esAtrasado = itemPedido.EsAtrasado
                    || (pedido.FechaEntregaCompromiso.HasValue
                        && pedido.FechaEntregaCompromiso.Value < hoy
                        && diferencia > 0);

                return new CumplimientoRenglonDto
                {
                    ComprobanteItemId = itemPedido.Id,
                    ItemId = itemPedido.ItemId,
                    ItemCodigo = items.GetValueOrDefault(itemPedido.ItemId)?.Codigo ?? "—",
                    ItemDescripcion = items.GetValueOrDefault(itemPedido.ItemId)?.Descripcion ?? itemPedido.Descripcion,
                    CantidadPedida = itemPedido.Cantidad,
                    CantidadEntregada = cantidadEntregada,
                    Diferencia = diferencia,
                    EstadoEntregaDescripcion = PedidoWorkflowRules.ObtenerDescripcionEstadoEntrega(estadoEntrega),
                    EsAtrasado = esAtrasado,
                    Entregas = entregasDelItem
                        .Select(x => new EntregaDetalleDto
                        {
                            ComprobanteId = x.ComprobanteId,
                            Numero = x.Numero,
                            Fecha = x.Fecha,
                            CantidadEntregada = x.CantidadEntregada
                        })
                        .ToList()
                        .AsReadOnly()
                };
            })
            .ToList();

        var estadoPedido = pedido.Estado == EstadoComprobante.Anulado
            ? Domain.Enums.EstadoPedido.Anulado
            : pedido.EstadoPedido ?? Domain.Enums.EstadoPedido.Pendiente;

        return new PedidoVinculacionesDto
        {
            PedidoId = pedido.Id,
            PedidoNumero = pedido.Numero.Formateado,
            EstadoPedido = estadoPedido,
            EstadoPedidoDescripcion = PedidoWorkflowRules.ObtenerDescripcionEstadoPedido(estadoPedido),
            RemitosGenerados = remitos
                .Select(x => new ComprobanteVinculadoDto
                {
                    Id = x.Id,
                    TipoComprobanteId = x.TipoComprobanteId,
                    Tipo = tipos.GetValueOrDefault(x.TipoComprobanteId)?.Descripcion ?? "Remito",
                    Numero = x.Numero.Formateado,
                    Fecha = x.Fecha,
                    Estado = x.Estado.ToString().ToUpperInvariant(),
                    Total = x.Total
                })
                .ToList()
                .AsReadOnly(),
            FacturasAsociadas = facturas
                .Select(x => new ComprobanteVinculadoDto
                {
                    Id = x.Id,
                    TipoComprobanteId = x.TipoComprobanteId,
                    Tipo = tipos.GetValueOrDefault(x.TipoComprobanteId)?.Descripcion ?? "Factura",
                    Numero = x.Numero.Formateado,
                    Fecha = x.Fecha,
                    Estado = x.Estado.ToString().ToUpperInvariant(),
                    Total = x.Total
                })
                .ToList()
                .AsReadOnly(),
            CumplimientoPorRenglon = cumplimiento.AsReadOnly()
        };
    }

    private sealed record EntregaProjection(
        long ComprobanteId,
        string Numero,
        DateOnly Fecha,
        long ItemId,
        long? ComprobanteItemOrigenId,
        decimal CantidadEntregada);
}
