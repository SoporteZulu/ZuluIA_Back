using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroDomiciliosQueryHandler(
    IApplicationDbContext db,
    IMapper mapper) : IRequestHandler<GetTerceroDomiciliosQuery, Result<IReadOnlyList<TerceroDomicilioDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroDomicilioDto>>> Handle(GetTerceroDomiciliosQuery request, CancellationToken ct)
    {
        var terceroExists = await db.Terceros.AsNoTrackingSafe().AnySafeAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (!terceroExists)
            return Result.Failure<IReadOnlyList<TerceroDomicilioDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var domicilios = await db.PersonasDomicilios
            .AsNoTrackingSafe()
            .Where(x => x.PersonaId == request.TerceroId)
            .OrderByDescending(x => x.EsDefecto)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Calle)
            .ToListSafeAsync(ct);

        var dtos = mapper.Map<List<TerceroDomicilioDto>>(domicilios);
        await TerceroDomicilioReadModelLoader.LoadDescripcionesAsync(db, dtos, ct);
        return Result.Success<IReadOnlyList<TerceroDomicilioDto>>(dtos);
    }
}
