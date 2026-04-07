using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroTransportesQueryHandler(
    IApplicationDbContext db,
    IMapper mapper) : IRequestHandler<GetTerceroTransportesQuery, Result<IReadOnlyList<TerceroTransporteDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroTransporteDto>>> Handle(GetTerceroTransportesQuery request, CancellationToken ct)
    {
        var terceroExists = await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (!terceroExists)
            return Result.Failure<IReadOnlyList<TerceroTransporteDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var transportes = await db.TercerosTransportes
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .ToListAsync(ct);

        var items = mapper.Map<List<TerceroTransporteDto>>(transportes);

        var transportistaIds = transportes
            .Where(x => x.TransportistaId.HasValue)
            .Select(x => x.TransportistaId!.Value)
            .Distinct()
            .ToList();

        var nombresTransportistas = transportistaIds.Count == 0
            ? new Dictionary<long, string>()
            : await db.Transportistas
                .AsNoTracking()
                .Where(x => transportistaIds.Contains(x.Id))
                .Join(db.Terceros.AsNoTracking(), t => t.TerceroId, ter => ter.Id, (t, ter) => new { t.Id, ter.RazonSocial })
                .ToDictionaryAsync(x => x.Id, x => x.RazonSocial, ct);

        for (var i = 0; i < items.Count; i++)
        {
            var source = transportes[i];
            if (source.TransportistaId.HasValue && nombresTransportistas.TryGetValue(source.TransportistaId.Value, out var nombre))
                items[i].TransportistaNombre = nombre;
        }

        return Result.Success<IReadOnlyList<TerceroTransporteDto>>(items);
    }
}
