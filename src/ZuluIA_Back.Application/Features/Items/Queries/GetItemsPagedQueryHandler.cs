using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetItemsPagedQueryHandler(
    IItemRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetItemsPagedQuery, PagedResult<ItemListDto>>
{
    public async Task<PagedResult<ItemListDto>> Handle(
        GetItemsPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.CategoriaId,
            request.SoloActivos,
            request.SoloConStock,
            request.SoloProductos,
            request.SoloServicios,
            ct);

        var categoriaIds = result.Items
            .Where(x => x.CategoriaId.HasValue)
            .Select(x => x.CategoriaId!.Value)
            .Distinct()
            .ToList();

        var unidadIds = result.Items
            .Select(x => x.UnidadMedidaId)
            .Distinct()
            .ToList();

        var alicuotaIds = result.Items
            .Select(x => x.AlicuotaIvaId)
            .Distinct()
            .ToList();

        var alicuotaCompraIds = result.Items
            .Where(x => x.AlicuotaIvaCompraId.HasValue)
            .Select(x => x.AlicuotaIvaCompraId!.Value)
            .Distinct()
            .ToList();

        var monedaIds = result.Items
            .Select(x => x.MonedaId)
            .Distinct()
            .ToList();

        var itemIds = result.Items
            .Select(x => x.Id)
            .ToList();

        var depositoIds = result.Items
            .Where(x => x.DepositoDefaultId.HasValue)
            .Select(x => x.DepositoDefaultId!.Value)
            .Distinct()
            .ToList();

        var marcaIds = result.Items
            .Where(x => x.MarcaId.HasValue)
            .Select(x => x.MarcaId!.Value)
            .Distinct()
            .ToList();

        var impuestoInternoIds = result.Items
            .Where(x => x.ImpuestoInternoId.HasValue)
            .Select(x => x.ImpuestoInternoId!.Value)
            .Distinct()
            .ToList();

        var categorias = categoriaIds.Count > 0
            ? await db.CategoriasItems
                .AsNoTracking()
                .Where(x => categoriaIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var marcas = marcaIds.Count > 0
            ? await db.MarcasComerciales
                .AsNoTracking()
                .Where(x => marcaIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var unidades = unidadIds.Count > 0
            ? await db.UnidadesMedida
                .AsNoTracking()
                .Where(x => unidadIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var alicuotasDescripcion = alicuotaIds.Count > 0
            ? await db.AlicuotasIva
                .AsNoTracking()
                .Where(x => alicuotaIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var alicuotasPorcentaje = alicuotaIds.Count > 0
            ? await db.AlicuotasIva
                .AsNoTracking()
                .Where(x => alicuotaIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, decimal>(x.Id, x.Porcentaje))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, decimal>();

        var alicuotasCompraDescripcion = alicuotaCompraIds.Count > 0
            ? await db.AlicuotasIva
                .AsNoTracking()
                .Where(x => alicuotaCompraIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var alicuotasCompraPorcentaje = alicuotaCompraIds.Count > 0
            ? await db.AlicuotasIva
                .AsNoTracking()
                .Where(x => alicuotaCompraIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, decimal>(x.Id, x.Porcentaje))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, decimal>();

        Dictionary<long, string?> monedas = monedaIds.Count > 0
            ? await db.Monedas
                .AsNoTracking()
                .Where(x => monedaIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string?>(x.Id, x.Simbolo))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string?>();

        var depositos = depositoIds.Count > 0
            ? await db.Depositos
                .AsNoTracking()
                .Where(x => depositoIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var impuestosInternos = impuestoInternoIds.Count > 0
            ? await db.Impuestos
                .AsNoTracking()
                .Where(x => impuestoInternoIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var stockPorItem = itemIds.Count > 0
            ? await db.Stock
                .AsNoTracking()
                .Where(x => itemIds.Contains(x.ItemId))
                .GroupBy(x => x.ItemId)
                .Select(x => new { ItemId = x.Key, Cantidad = x.Sum(s => s.Cantidad) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Cantidad, ct)
            : new Dictionary<long, decimal>();

        var componentesPorItem = itemIds.Count > 0
            ? await db.ItemsComponentes
                .AsNoTracking()
                .Where(x => itemIds.Contains(x.ItemPadreId))
                .GroupBy(x => x.ItemPadreId)
                .Select(x => new { ItemId = x.Key, Cantidad = x.Count() })
                .ToDictionaryAsync(x => x.ItemId, x => x.Cantidad, ct)
            : new Dictionary<long, int>();

        var stockOperativoRows = itemIds.Count > 0
            ? await (
                from ci in db.ComprobantesItems.AsNoTracking()
                join c in db.Comprobantes.AsNoTracking() on ci.ComprobanteId equals c.Id
                join tc in db.TiposComprobante.AsNoTracking() on c.TipoComprobanteId equals tc.Id
                where itemIds.Contains(ci.ItemId)
                      && tc.EsVenta
                      && !tc.AfectaStock
                      && c.Estado != EstadoComprobante.Anulado
                      && c.Estado != EstadoComprobante.Convertido
                select new
                {
                    ci.ItemId,
                    c.Estado,
                    CantidadOperativa = ci.Cantidad - ci.CantidadBonificada
                })
                .ToListAsync(ct)
            : [];

        var stockOperativoPorItem = stockOperativoRows
            .GroupBy(x => x.ItemId)
            .ToDictionary(
                x => x.Key,
                x => (
                    StockReservado: x.Where(v => v.Estado == EstadoComprobante.Borrador)
                        .Sum(v => v.CantidadOperativa),
                    StockComprometido: x.Where(v => v.Estado == EstadoComprobante.Emitido ||
                                                    v.Estado == EstadoComprobante.PagadoParcial ||
                                                    v.Estado == EstadoComprobante.Pagado)
                        .Sum(v => v.CantidadOperativa)));

        var stockEnTransitoPorItem = itemIds.Count > 0
            ? await (
                from td in db.TransferenciasDepositoDetalles.AsNoTracking()
                join t in db.TransferenciasDeposito.AsNoTracking()
                    on td.TransferenciaDepositoId equals t.Id
                where itemIds.Contains(td.ItemId)
                      && t.Estado == EstadoTransferenciaDeposito.EnTransito
                      && !t.IsDeleted
                group td by td.ItemId into g
                select new { ItemId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Cantidad, ct)
            : new Dictionary<long, decimal>();

        var dtos = new List<ItemListDto>(result.Items.Count);

        foreach (var i in result.Items)
        {
            var stockFisico = stockPorItem.GetValueOrDefault(i.Id);
            var stockOperativo = stockOperativoPorItem.GetValueOrDefault(i.Id);

            dtos.Add(new ItemListDto
            {
                Id = i.Id,
                Codigo = i.Codigo,
                CodigoAlternativo = i.CodigoAlternativo,
                CodigoBarras = i.CodigoBarras,
                Descripcion = i.Descripcion,
                DescripcionAdicional = i.DescripcionAdicional,
                CategoriaId = i.CategoriaId,
                CategoriaDescripcion = i.CategoriaId.HasValue
                    ? categorias.GetValueOrDefault(i.CategoriaId.Value)
                    : null,
                MarcaId = i.MarcaId,
                MarcaDescripcion = i.MarcaId.HasValue
                    ? marcas.GetValueOrDefault(i.MarcaId.Value)
                    : null,
                UnidadMedidaId = i.UnidadMedidaId,
                UnidadMedidaDescripcion = unidades.GetValueOrDefault(i.UnidadMedidaId),
                AlicuotaIvaId = i.AlicuotaIvaId,
                AlicuotaIvaDescripcion = alicuotasDescripcion.GetValueOrDefault(i.AlicuotaIvaId),
                AlicuotaIvaPorcentaje = alicuotasPorcentaje.GetValueOrDefault(i.AlicuotaIvaId),
                AlicuotaIvaCompraId = i.AlicuotaIvaCompraId,
                AlicuotaIvaCompraDescripcion = i.AlicuotaIvaCompraId.HasValue
                    ? alicuotasCompraDescripcion.GetValueOrDefault(i.AlicuotaIvaCompraId.Value)
                    : null,
                AlicuotaIvaCompraPorcentaje = i.AlicuotaIvaCompraId.HasValue
                    ? alicuotasCompraPorcentaje.GetValueOrDefault(i.AlicuotaIvaCompraId.Value)
                    : null,
                ImpuestoInternoId = i.ImpuestoInternoId,
                ImpuestoInternoDescripcion = i.ImpuestoInternoId.HasValue
                    ? impuestosInternos.GetValueOrDefault(i.ImpuestoInternoId.Value)
                    : null,
                MonedaId = i.MonedaId,
                MonedaSimbol = monedas.GetValueOrDefault(i.MonedaId),
                EsProducto = i.EsProducto,
                EsServicio = i.EsServicio,
                EsFinanciero = i.EsFinanciero,
                ManejaStock = i.ManejaStock,
                PrecioCosto = i.PrecioCosto,
                PrecioVenta = i.PrecioVenta,
                Stock = stockFisico,
                StockDisponible = stockFisico - stockOperativo.StockComprometido - stockOperativo.StockReservado,
                StockComprometido = stockOperativo.StockComprometido,
                StockReservado = stockOperativo.StockReservado,
                StockEnTransito = stockEnTransitoPorItem.GetValueOrDefault(i.Id),
                StockMinimo = i.StockMinimo,
                StockMaximo = i.StockMaximo,
                PuntoReposicion = i.PuntoReposicion,
                StockSeguridad = i.StockSeguridad,
                Peso = i.Peso,
                Volumen = i.Volumen,
                EsTrazable = i.EsTrazable,
                PermiteFraccionamiento = i.PermiteFraccionamiento,
                DiasVencimientoLimite = i.DiasVencimientoLimite,
                DepositoDefaultId = i.DepositoDefaultId,
                DepositoDefaultDescripcion = i.DepositoDefaultId.HasValue
                    ? depositos.GetValueOrDefault(i.DepositoDefaultId.Value)
                    : null,
                CodigoAfip = i.CodigoAfip,
                SucursalId = i.SucursalId,
                Activo = i.Activo,
                AplicaVentas = i.AplicaVentas,
                AplicaCompras = i.AplicaCompras,
                PorcentajeGanancia = i.PorcentajeGanancia,
                PorcentajeMaximoDescuento = i.PorcentajeMaximoDescuento,
                EsRpt = i.EsRpt,
                EsSistema = i.EsSistema,
                EsPack = componentesPorItem.ContainsKey(i.Id),
                CantidadComponentes = componentesPorItem.GetValueOrDefault(i.Id),
                PrecioVentaCalculado = i.CalcularPrecioVentaPorGanancia(),
                PuedeEditar = !i.EsSistema,
                CreatedAt = i.CreatedAt
            });
        }

        return new PagedResult<ItemListDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}
