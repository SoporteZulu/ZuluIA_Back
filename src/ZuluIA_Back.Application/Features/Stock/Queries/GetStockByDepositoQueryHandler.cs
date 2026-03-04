using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.DTOs;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Stock.Queries;

public class GetStockByDepositoQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetStockByDepositoQuery, IReadOnlyList<StockItemDto>>
{
    public async Task<IReadOnlyList<StockItemDto>> Handle(
        GetStockByDepositoQuery request,
        CancellationToken ct)
    {
        var result = await (
            from s in db.Stock.AsNoTracking()
            join i in db.Items.AsNoTracking()
                on s.ItemId equals i.Id
            join d in db.Depositos.AsNoTracking()
                on s.DepositoId equals d.Id
            where s.DepositoId == request.DepositoId && i.Activo
            select new StockItemDto
            {
                Id                  = s.Id,
                ItemId              = s.ItemId,
                ItemCodigo          = i.Codigo,
                ItemDescripcion     = i.Descripcion,
                DepositoId          = s.DepositoId,
                DepositoDescripcion = d.Descripcion,
                DepositoEsDefault   = d.EsDefault,
                Cantidad            = s.Cantidad,
                StockMinimo         = i.StockMinimo,
                StockMaximo         = i.StockMaximo,
                BajoMinimo          = s.Cantidad < i.StockMinimo,
                UpdatedAt           = s.UpdatedAt
            })
            .OrderBy(x => x.ItemCodigo)
            .ToListAsync(ct);

        return result.AsReadOnly();
    }
}