using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.DescuentosComerciales.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Queries;

public class GetDescuentosComercialesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetDescuentosComercialesQuery, IReadOnlyList<DescuentoComercialDto>>
{
    public async Task<IReadOnlyList<DescuentoComercialDto>> Handle(
        GetDescuentosComercialesQuery request,
        CancellationToken ct)
    {
        var query = db.DescuentosComerciales
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (request.TerceroId.HasValue)
            query = query.Where(x => x.TerceroId == request.TerceroId.Value);

        if (request.ItemId.HasValue)
            query = query.Where(x => x.ItemId == request.ItemId.Value);

        if (request.VigenteEn.HasValue)
        {
            var fecha = request.VigenteEn.Value;
            query = query.Where(x =>
                x.FechaDesde <= fecha &&
                (!x.FechaHasta.HasValue || x.FechaHasta.Value >= fecha));
        }

        return await query
            .OrderBy(x => x.TerceroId)
            .ThenBy(x => x.ItemId)
            .Select(x => new DescuentoComercialDto
            {
                Id         = x.Id,
                TerceroId  = x.TerceroId,
                ItemId     = x.ItemId,
                FechaDesde = x.FechaDesde,
                FechaHasta = x.FechaHasta,
                Porcentaje = x.Porcentaje
            })
            .ToListAsync(ct);
    }
}
