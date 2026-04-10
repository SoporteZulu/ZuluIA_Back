using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.DTOs;

namespace ZuluIA_Back.Application.Features.Stock.Queries;

public class GetStockByItemQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetStockByItemQuery, StockResumenDto?>
{
    public async Task<StockResumenDto?> Handle(
        GetStockByItemQuery request,
        CancellationToken ct)
    {
        var item = await db.Items
            .AsNoTracking()
            .Where(x => x.Id == request.ItemId)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.StockMinimo,
                x.StockMaximo
            })
            .FirstOrDefaultAsync(ct);

        if (item is null) return null;

        var stockPorDeposito = await (
            from s in db.Stock.AsNoTracking()
            join d in db.Depositos.AsNoTracking()
                on s.DepositoId equals d.Id
            where s.ItemId == request.ItemId
            select new StockPorDepositoDto
            {
                Id                  = s.Id,
                ItemId              = s.ItemId,
                DepositoId          = s.DepositoId,
                DepositoDescripcion = d.Descripcion,
                EsDefault           = d.EsDefault,
                Cantidad            = s.Cantidad,
                UpdatedAt           = s.UpdatedAt
            })
            .OrderByDescending(x => x.EsDefault)
            .ThenBy(x => x.DepositoDescripcion)
            .ToListAsync(ct);

        var totalStock = stockPorDeposito.Sum(x => x.Cantidad);

        return new StockResumenDto
        {
            ItemId           = item.Id,
            ItemCodigo       = item.Codigo,
            ItemDescripcion  = item.Descripcion,
            StockTotal       = totalStock,
            StockMinimo      = item.StockMinimo,
            StockMaximo      = item.StockMaximo,
            BajoMinimo       = totalStock < item.StockMinimo,
            PorDeposito      = stockPorDeposito.AsReadOnly()
        };
    }
}