using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;

namespace ZuluIA_Back.Application.Features.Retenciones.Queries;

public class GetTipoRetencionByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTipoRetencionByIdQuery, TipoRetencionDto?>
{
    public async Task<TipoRetencionDto?> Handle(
        GetTipoRetencionByIdQuery request,
        CancellationToken ct)
    {
        var t = await db.TiposRetencion
            .Include(x => x.Escalas)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (t is null) return null;

        return new TipoRetencionDto(
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
                    e.Id, e.Descripcion, e.ImporteDesde, e.ImporteHasta, e.Porcentaje))
                .ToList()
                .AsReadOnly()
        );
    }
}
