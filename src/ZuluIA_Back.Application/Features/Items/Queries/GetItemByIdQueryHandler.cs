using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetItemByIdQueryHandler(
    IItemRepository repo,
    IApplicationDbContext db,
    ItemCommercialStockService itemCommercialStockService)
    : IRequestHandler<GetItemByIdQuery, ItemDto?>
{
    public async Task<ItemDto?> Handle(GetItemByIdQuery request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);
        if (item is null) return null;

        var categoria = item.CategoriaId.HasValue
            ? await db.CategoriasItems
                .AsNoTracking()
                .Where(x => x.Id == item.CategoriaId.Value)
                .Select(x => new { x.Descripcion })
                .FirstOrDefaultAsync(ct)
            : null;

        var marca = item.MarcaId.HasValue
            ? await db.MarcasComerciales
                .AsNoTracking()
                .Where(x => x.Id == item.MarcaId.Value)
                .Select(x => new { x.Descripcion })
                .FirstOrDefaultAsync(ct)
            : null;

        var unidad = await db.UnidadesMedida
            .AsNoTracking()
            .Where(x => x.Id == item.UnidadMedidaId)
            .Select(x => new { x.Descripcion })
            .FirstOrDefaultAsync(ct);

        var alicuota = await db.AlicuotasIva
            .AsNoTracking()
            .Where(x => x.Id == item.AlicuotaIvaId)
            .Select(x => new { x.Porcentaje })
            .FirstOrDefaultAsync(ct);

        var alicuotaCompra = item.AlicuotaIvaCompraId.HasValue
            ? await db.AlicuotasIva
                .AsNoTracking()
                .Where(x => x.Id == item.AlicuotaIvaCompraId.Value)
                .Select(x => new { x.Descripcion, x.Porcentaje })
                .FirstOrDefaultAsync(ct)
            : null;

        var impuestoInterno = item.ImpuestoInternoId.HasValue
            ? await db.Impuestos
                .AsNoTracking()
                .Where(x => x.Id == item.ImpuestoInternoId.Value)
                .Select(x => new { x.Descripcion })
                .FirstOrDefaultAsync(ct)
            : null;

        var depositoDefaultDescripcion = item.DepositoDefaultId.HasValue
            ? await db.Depositos
                .AsNoTracking()
                .Where(x => x.Id == item.DepositoDefaultId.Value)
                .Select(x => x.Descripcion)
                .FirstOrDefaultAsync(ct)
            : null;

        var listasPrecios = await (
            from li in db.ListaPreciosItems.AsNoTracking()
            join l in db.ListasPrecios.AsNoTracking()
                on li.ListaId equals l.Id
            where li.ItemId == request.Id
            orderby l.Descripcion
            select new ItemListaPrecioDto
            {
                ListaId = l.Id,
                ListaDescripcion = l.Descripcion,
                MonedaId = l.MonedaId,
                VigenciaDesde = l.VigenciaDesde,
                VigenciaHasta = l.VigenciaHasta,
                Activa = l.Activa,
                Precio = li.Precio,
                DescuentoPct = li.DescuentoPct,
                PrecioFinal = li.PrecioFinal,
                UpdatedAt = li.UpdatedAt
            })
            .ToListAsync(ct);

        var atributosComerciales = await (
            from ia in db.ItemsAtributosComerciales.AsNoTracking()
            join a in db.AtributosComerciales.AsNoTracking()
                on ia.AtributoComercialId equals a.Id
            where ia.ItemId == request.Id && !ia.IsDeleted
            orderby a.Descripcion
            select new ItemAtributoComercialDto
            {
                Id = ia.Id,
                AtributoComercialId = a.Id,
                AtributoCodigo = a.Codigo,
                AtributoDescripcion = a.Descripcion,
                TipoDato = a.TipoDato.ToString(),
                Valor = ia.Valor
            })
            .ToListAsync(ct);

        var componentesRows = await (
            from ic in db.ItemsComponentes.AsNoTracking()
            join c in db.Items.AsNoTracking() on ic.ComponenteId equals c.Id
            where ic.ItemPadreId == request.Id
            orderby c.Descripcion
            select new
            {
                ic.Id,
                ic.ItemPadreId,
                ic.ComponenteId,
                c.Codigo,
                c.Descripcion,
                UnidadMedidaId = ic.UnidadMedidaId ?? c.UnidadMedidaId,
                ic.Cantidad
            })
            .ToListAsync(ct);

        var unidadesComponentesIds = componentesRows
            .Select(x => x.UnidadMedidaId)
            .Distinct()
            .ToList();

        var unidadesComponentes = unidadesComponentesIds.Count > 0
            ? await db.UnidadesMedida
                .AsNoTracking()
                .Where(x => unidadesComponentesIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : new Dictionary<long, string>();

        var componentes = componentesRows
            .Select(x => new ItemPackComponenteDto
            {
                Id = x.Id,
                ItemPadreId = x.ItemPadreId,
                ComponenteId = x.ComponenteId,
                ComponenteCodigo = x.Codigo,
                ComponenteDescripcion = x.Descripcion,
                Cantidad = x.Cantidad,
                UnidadMedidaId = x.UnidadMedidaId,
                UnidadMedidaDescripcion = unidadesComponentes.GetValueOrDefault(x.UnidadMedidaId)
            })
            .ToList();

        var stockSnapshot = await itemCommercialStockService.GetSnapshotAsync(request.Id, ct);

        var transferenciasEnTransitoIds = await db.TransferenciasDeposito
            .AsNoTracking()
            .Where(x => x.Estado == EstadoTransferenciaDeposito.EnTransito)
            .Select(x => x.Id)
            .ToListAsync(ct);

        var stockEnTransito = transferenciasEnTransitoIds.Count > 0
            ? await db.TransferenciasDepositoDetalles
                .AsNoTracking()
                .Where(x => x.ItemId == request.Id && transferenciasEnTransitoIds.Contains(x.TransferenciaDepositoId))
                .SumAsync(x => x.Cantidad, ct)
            : 0m;

        return new ItemDto
        {
            Id = item.Id,
            Codigo = item.Codigo,
            CodigoAlternativo = item.CodigoAlternativo,
            CodigoBarras = item.CodigoBarras,
            Descripcion = item.Descripcion,
            DescripcionAdicional = item.DescripcionAdicional,
            CategoriaId = item.CategoriaId,
            CategoriaDescripcion = categoria?.Descripcion,
            MarcaId = item.MarcaId,
            MarcaDescripcion = marca?.Descripcion,
            UnidadMedidaId = item.UnidadMedidaId,
            UnidadMedidaDescripcion = unidad?.Descripcion,
            AlicuotaIvaId = item.AlicuotaIvaId,
            AlicuotaIvaPorcentaje = alicuota?.Porcentaje ?? 0,
            MonedaId = item.MonedaId,
            EsProducto = item.EsProducto,
            EsServicio = item.EsServicio,
            EsFinanciero = item.EsFinanciero,
            ManejaStock = item.ManejaStock,
            PrecioCosto = item.PrecioCosto,
            PrecioVenta = item.PrecioVenta,
            Stock = stockSnapshot.Stock,
            StockDisponible = stockSnapshot.StockDisponible,
            StockComprometido = stockSnapshot.StockComprometido,
            StockReservado = stockSnapshot.StockReservado,
            StockEnTransito = stockEnTransito,
            StockMinimo = item.StockMinimo,
            StockMaximo = item.StockMaximo,
            PuntoReposicion = item.PuntoReposicion,
            StockSeguridad = item.StockSeguridad,
            Peso = item.Peso,
            Volumen = item.Volumen,
            EsTrazable = item.EsTrazable,
            PermiteFraccionamiento = item.PermiteFraccionamiento,
            DiasVencimientoLimite = item.DiasVencimientoLimite,
            DepositoDefaultId = item.DepositoDefaultId,
            DepositoDefaultDescripcion = depositoDefaultDescripcion,
            CodigoAfip = item.CodigoAfip,
            SucursalId = item.SucursalId,
            Activo = item.Activo,
            AplicaVentas = item.AplicaVentas,
            AplicaCompras = item.AplicaCompras,
            PorcentajeGanancia = item.PorcentajeGanancia,
            PorcentajeMaximoDescuento = item.PorcentajeMaximoDescuento,
            EsRpt = item.EsRpt,
            EsSistema = item.EsSistema,
            EsPack = componentes.Count > 0,
            PrecioVentaCalculado = item.CalcularPrecioVentaPorGanancia(),
            PuedeEditar = !item.EsSistema,
            ListasPrecios = listasPrecios,
            AtributosComerciales = atributosComerciales,
            Componentes = componentes,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
