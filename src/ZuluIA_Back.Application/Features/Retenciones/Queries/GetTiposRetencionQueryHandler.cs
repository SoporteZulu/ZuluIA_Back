using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;

namespace ZuluIA_Back.Application.Features.Retenciones.Queries;

public class GetTiposRetencionQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTiposRetencionQuery, IReadOnlyList<TipoRetencionDto>>
{
    public async Task<IReadOnlyList<TipoRetencionDto>> Handle(
        GetTiposRetencionQuery request,
        CancellationToken ct)
    {
        var query = db.TiposRetencion
            .Include(x => x.Escalas)
            .AsNoTracking()
            .AsQueryable();

        if (request.SoloActivos)
            query = query.Where(x => x.Activo);

        var lista = await query
            .OrderBy(x => x.Descripcion)
            .ToListAsync(ct);

        return lista.Select(t => new TipoRetencionDto(
            t.Id,
            t.Descripcion,
            t.Regimen,
            t.MinimoNoImponible,
            t.AcumulaPago,
            t.TipoComprobanteId,
            t.ItemId,
            t.Activo,
            t.Escalas
                .OrderBy(e => e.ImporteDesde)
                .Select(e => new EscalaRetencionDto(
                    e.Id,
                    e.Descripcion,
                    e.ImporteDesde,
                    e.ImporteHasta,
                    e.Porcentaje))
                .ToList()
                .AsReadOnly()
        )).ToList().AsReadOnly();
    }
}
