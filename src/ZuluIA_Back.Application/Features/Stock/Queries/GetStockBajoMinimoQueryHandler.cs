using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.DTOs;

namespace ZuluIA_Back.Application.Features.Stock.Queries;

public class GetStockBajoMinimoQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetStockBajoMinimoQuery, IReadOnlyList<StockBajoMinimoDto>>
{
    public async Task<IReadOnlyList<StockBajoMinimoDto>> Handle(
        GetStockBajoMinimoQuery request,
        CancellationToken ct)
    {
        var query =
            from s in db.Stock.AsNoTracking()
            join i in db.Items.AsNoTracking()
                on s.ItemId equals i.Id
            join d in db.Depositos.AsNoTracking()
                on s.DepositoId equals d.Id
            where i.ManejaStock &&
                  i.Activo      &&
                  d.Activo      &&
                  s.Cantidad < i.StockMinimo
            select new StockBajoMinimoDto
            {
                ItemId              = i.Id,
                ItemCodigo          = i.Codigo,
                ItemDescripcion     = i.Descripcion,
                DepositoId          = d.Id,
                DepositoDescripcion = d.Descripcion,
                CantidadActual      = s.Cantidad,
                StockMinimo         = i.StockMinimo,
                Diferencia          = i.StockMinimo - s.Cantidad
            };

        if (request.DepositoId.HasValue)
            query = query.Where(x => x.DepositoId == request.DepositoId.Value);

        if (request.SucursalId.HasValue)
        {
            var depositoIds = db.Depositos
                .AsNoTracking()
                .Where(d => d.SucursalId == request.SucursalId.Value && d.Activo)
                .Select(d => d.Id);

            query = query.Where(x => depositoIds.Contains(x.DepositoId));
        }

        return await query
            .OrderByDescending(x => x.Diferencia)
            .ThenBy(x => x.ItemCodigo)
            .ToListAsync(ct);
    }
}