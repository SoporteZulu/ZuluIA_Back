using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var terceroExists = await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (!terceroExists)
            return Result.Failure<IReadOnlyList<TerceroDomicilioDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var domicilios = await db.PersonasDomicilios
            .AsNoTracking()
            .Where(x => x.PersonaId == request.TerceroId)
            .OrderByDescending(x => x.EsDefecto)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Calle)
            .ToListAsync(ct);

        var dtos = mapper.Map<List<TerceroDomicilioDto>>(domicilios);
        await TerceroDomicilioReadModelLoader.LoadDescripcionesAsync(db, dtos, ct);
        return Result.Success<IReadOnlyList<TerceroDomicilioDto>>(dtos);
    }
}
