using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.RetencionesPorPersona.Queries;

public record GetRetencionesPorPersonaQuery(long TerceroId) : IRequest<IReadOnlyList<RetencionPorPersonaDto>>;

public record RetencionPorPersonaDto(
    long Id,
    long TerceroId,
    long TipoRetencionId,
    string? TipoRetencionNombre,
    string? Descripcion
);

public class GetRetencionesPorPersonaQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetRetencionesPorPersonaQuery, IReadOnlyList<RetencionPorPersonaDto>>
{
    public async Task<IReadOnlyList<RetencionPorPersonaDto>> Handle(
        GetRetencionesPorPersonaQuery request,
        CancellationToken ct)
    {
        return await db.RetencionesPorPersona
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && !x.IsDeleted)
            .Join(db.TiposRetencion,
                r  => r.TipoRetencionId,
                tr => tr.Id,
                (r, tr) => new RetencionPorPersonaDto(
                    r.Id,
                    r.TerceroId,
                    r.TipoRetencionId,
                    tr.Descripcion,
                    r.Descripcion))
            .ToListAsync(ct);
    }
}
