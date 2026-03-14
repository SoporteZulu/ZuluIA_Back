using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Queries;

public class GetOrdenesPreparacionPagedQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrdenesPreparacionPagedQuery, PagedResult<OrdenPreparacionListDto>>
{
    public async Task<PagedResult<OrdenPreparacionListDto>> Handle(
        GetOrdenesPreparacionPagedQuery request,
        CancellationToken ct)
    {
        var query = db.OrdenesPreparacion
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (request.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == request.SucursalId.Value);

        if (request.TerceroId.HasValue)
            query = query.Where(x => x.TerceroId == request.TerceroId.Value);

        if (request.Estado.HasValue)
            query = query.Where(x => x.Estado == request.Estado.Value);

        if (request.Desde.HasValue)
            query = query.Where(x => x.Fecha >= request.Desde.Value);

        if (request.Hasta.HasValue)
            query = query.Where(x => x.Fecha <= request.Hasta.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new OrdenPreparacionListDto
            {
                Id                   = x.Id,
                SucursalId           = x.SucursalId,
                TerceroId            = x.TerceroId,
                ComprobanteOrigenId  = x.ComprobanteOrigenId,
                Fecha                = x.Fecha,
                Estado               = x.Estado,
                Observacion          = x.Observacion,
                FechaConfirmacion    = x.FechaConfirmacion,
                CantidadRenglones    = x.Detalles.Count
            })
            .ToListAsync(ct);

        return new PagedResult<OrdenPreparacionListDto>(items, request.Page, request.PageSize, totalCount);
    }
}
