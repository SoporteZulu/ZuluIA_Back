using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Application.Features.Ventas.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public class GetPedidosConEstadoQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPedidosConEstadoQuery, PagedResult<PedidoConEstadoDto>>
{
    public async Task<PagedResult<PedidoConEstadoDto>> Handle(GetPedidosConEstadoQuery request, CancellationToken ct)
    {
        var pedidoTypes = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => x.EsVenta)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToListAsync(ct);

        var pedidoTypeIds = pedidoTypes
            .Where(x => PedidoWorkflowRules.EsPedido(x.Codigo, x.Descripcion))
            .Select(x => x.Id)
            .ToHashSet();

        var query = db.Comprobantes
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => pedidoTypeIds.Contains(x.TipoComprobanteId)
                || x.EstadoPedido.HasValue
                || x.FechaEntregaCompromiso.HasValue);

        if (request.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == request.SucursalId.Value);

        if (request.TerceroId.HasValue)
            query = query.Where(x => x.TerceroId == request.TerceroId.Value);

        if (request.FechaDesde.HasValue)
            query = query.Where(x => x.Fecha >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(x => x.Fecha <= request.FechaHasta.Value);

        if (request.FechaEntregaDesde.HasValue)
            query = query.Where(x => x.FechaEntregaCompromiso >= request.FechaEntregaDesde.Value);

        if (request.FechaEntregaHasta.HasValue)
            query = query.Where(x => x.FechaEntregaCompromiso <= request.FechaEntregaHasta.Value);

        if (request.ItemId.HasValue)
            query = query.Where(x => x.Items.Any(i => i.ItemId == request.ItemId.Value));

        if (request.EstadoEntregaItem.HasValue)
            query = query.Where(x => x.Items.Any(i => i.EstadoEntrega == request.EstadoEntregaItem.Value));

        if (request.SoloAtrasados == true)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            query = query.Where(x => x.Items.Any(i => i.EsAtrasado)
                || (x.FechaEntregaCompromiso.HasValue && x.FechaEntregaCompromiso.Value < hoy));
        }

        if (!string.IsNullOrWhiteSpace(request.CodigoOConcepto))
        {
            var termino = request.CodigoOConcepto.Trim().ToUpperInvariant();
            var itemIds = await db.Items
                .AsNoTracking()
                .Where(x => x.Codigo.ToUpper().Contains(termino) || x.Descripcion.ToUpper().Contains(termino))
                .Select(x => x.Id)
                .ToListAsync(ct);

            query = query.Where(x => x.Items.Any(i => itemIds.Contains(i.ItemId)
                || i.Descripcion.ToUpper().Contains(termino)
                || (i.ObservacionRenglon != null && i.ObservacionRenglon.ToUpper().Contains(termino))));
        }

        if (request.EstadoPedido.HasValue)
        {
            query = query.Where(x => x.EstadoPedido == request.EstadoPedido.Value
                || (!x.EstadoPedido.HasValue && request.EstadoPedido == Domain.Enums.EstadoPedido.Pendiente));
        }

        var total = await query.CountAsync(ct);
        var pedidos = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        if (pedidos.Count == 0)
            return PagedResult<PedidoConEstadoDto>.Empty(request.Page, request.PageSize);

        var terceroIds = pedidos.Select(x => x.TerceroId).Distinct().ToList();
        var sucursalIds = pedidos.Select(x => x.SucursalId).Distinct().ToList();
        var monedaIds = pedidos.Select(x => x.MonedaId).Distinct().ToList();
        var tipoIds = pedidos.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var itemIdsPedido = pedidos.SelectMany(x => x.Items).Select(x => x.ItemId).Distinct().ToList();

        var terceros = await db.Terceros
            .AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.Legajo, x.NroDocumento, x.CondicionIvaId })
            .ToListAsync(ct);

        var condicionIvaIds = terceros.Select(x => x.CondicionIvaId).Distinct().ToList();
        var condicionesIva = await db.CondicionesIva
            .AsNoTracking()
            .Where(x => condicionIvaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct);

        var tercerosLookup = terceros.ToDictionary(
            x => x.Id,
            x => new
            {
                x.RazonSocial,
                x.Legajo,
                x.NroDocumento,
                CondicionIva = condicionesIva.GetValueOrDefault(x.CondicionIvaId)
            });

        var sucursales = await db.Sucursales
            .AsNoTracking()
            .Where(x => sucursalIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, x => x.RazonSocial, ct);

        var monedas = await db.Monedas
            .AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, x => x.Simbolo, ct);

        var tipos = pedidoTypes
            .Where(x => tipoIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x.Descripcion);

        var items = await db.Items
            .AsNoTracking()
            .Where(x => itemIdsPedido.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var hoyActual = DateOnly.FromDateTime(DateTime.Today);
        var dtos = pedidos.Select(pedido =>
        {
            var estadoPedido = ResolverEstadoPedido(pedido);
            var itemsDto = pedido.Items
                .OrderBy(x => x.Orden)
                .Select(item =>
                {
                    var estadoEntrega = ResolverEstadoEntrega(item);
                    var cantidadPendiente = item.CantidadPendiente > 0
                        ? item.CantidadPendiente
                        : Math.Max(0, item.Cantidad - item.CantidadEntregada);
                    var esAtrasado = item.EsAtrasado
                        || (pedido.FechaEntregaCompromiso.HasValue
                            && pedido.FechaEntregaCompromiso.Value < hoyActual
                            && cantidadPendiente > 0);

                    return new PedidoItemConEstadoDto
                    {
                        Id = item.Id,
                        ItemId = item.ItemId,
                        ItemCodigo = items.GetValueOrDefault(item.ItemId)?.Codigo ?? "—",
                        ItemDescripcion = items.GetValueOrDefault(item.ItemId)?.Descripcion ?? item.Descripcion,
                        Concepto = item.ObservacionRenglon,
                        CantidadPedida = item.Cantidad,
                        CantidadEntregada = item.CantidadEntregada,
                        CantidadPendiente = cantidadPendiente,
                        Diferencia = item.ObtenerDiferencia(),
                        EstadoEntrega = estadoEntrega,
                        EstadoEntregaDescripcion = PedidoWorkflowRules.ObtenerDescripcionEstadoEntrega(estadoEntrega),
                        EsAtrasado = esAtrasado,
                        PrecioUnitario = item.PrecioUnitario,
                        TotalLinea = item.TotalLinea
                    };
                })
                .ToList();

            var renglonesCompletados = itemsDto.Count(x => x.EstadoEntrega is EstadoEntregaItem.EntregaCompleta or EstadoEntregaItem.EntregaSobrepasada);
            var renglonesPendientes = itemsDto.Count - renglonesCompletados;
            var totalCantidad = pedido.Items.Sum(x => x.Cantidad);
            var totalEntregada = pedido.Items.Sum(x => x.CantidadEntregada);
            var porcentajeCumplimiento = totalCantidad > 0
                ? Math.Round((totalEntregada / totalCantidad) * 100m, 2)
                : 0m;

            return new PedidoConEstadoDto
            {
                Id = pedido.Id,
                SucursalId = pedido.SucursalId,
                SucursalRazonSocial = sucursales.GetValueOrDefault(pedido.SucursalId) ?? "—",
                TipoComprobanteId = pedido.TipoComprobanteId,
                TipoComprobanteDescripcion = tipos.GetValueOrDefault(pedido.TipoComprobanteId) ?? "Pedido",
                Prefijo = pedido.Numero.Prefijo,
                Numero = pedido.Numero.Numero,
                NumeroFormateado = pedido.Numero.Formateado,
                Fecha = pedido.Fecha,
                FechaEntregaCompromiso = pedido.FechaEntregaCompromiso,
                TerceroId = pedido.TerceroId,
                ClienteRazonSocial = tercerosLookup.GetValueOrDefault(pedido.TerceroId)?.RazonSocial ?? "—",
                ClienteLegajo = tercerosLookup.GetValueOrDefault(pedido.TerceroId)?.Legajo,
                ClienteCuit = tercerosLookup.GetValueOrDefault(pedido.TerceroId)?.NroDocumento,
                ClienteCondicionIva = tercerosLookup.GetValueOrDefault(pedido.TerceroId)?.CondicionIva,
                MonedaId = pedido.MonedaId,
                MonedaSimbolo = monedas.GetValueOrDefault(pedido.MonedaId) ?? "$",
                Cotizacion = pedido.Cotizacion,
                Total = pedido.Total,
                Saldo = pedido.Saldo,
                EstadoPedido = estadoPedido,
                EstadoPedidoDescripcion = PedidoWorkflowRules.ObtenerDescripcionEstadoPedido(estadoPedido),
                EsAtrasado = itemsDto.Any(x => x.EsAtrasado),
                CantidadRenglones = itemsDto.Count,
                RenglonesCompletados = renglonesCompletados,
                RenglonesPendientes = renglonesPendientes,
                PorcentajeCumplimiento = porcentajeCumplimiento,
                Observacion = pedido.Observacion,
                Items = itemsDto.AsReadOnly()
            };
        }).ToList();

        return new PagedResult<PedidoConEstadoDto>(dtos, request.Page, request.PageSize, total);
    }

    private static EstadoPedido ResolverEstadoPedido(Comprobante pedido)
    {
        if (pedido.Estado == EstadoComprobante.Anulado)
            return Domain.Enums.EstadoPedido.Anulado;

        return pedido.EstadoPedido ?? Domain.Enums.EstadoPedido.Pendiente;
    }

    private static EstadoEntregaItem ResolverEstadoEntrega(ComprobanteItem item)
    {
        if (item.EstadoEntrega.HasValue)
            return item.EstadoEntrega.Value;

        if (item.CantidadEntregada <= 0)
            return EstadoEntregaItem.NoEntregado;

        if (item.CantidadEntregada < item.Cantidad)
            return EstadoEntregaItem.EntregaParcial;

        if (item.CantidadEntregada == item.Cantidad)
            return EstadoEntregaItem.EntregaCompleta;

        return EstadoEntregaItem.EntregaSobrepasada;
    }
}
