using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroVentanasCobranzaQueryHandler(
    IApplicationDbContext db,
    IMapper mapper) : IRequestHandler<GetTerceroVentanasCobranzaQuery, Result<IReadOnlyList<TerceroVentanaCobranzaDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroVentanaCobranzaDto>>> Handle(GetTerceroVentanasCobranzaQuery request, CancellationToken ct)
    {
        var terceroExists = await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (!terceroExists)
            return Result.Failure<IReadOnlyList<TerceroVentanaCobranzaDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var ventanas = await db.TercerosVentanasCobranza
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Dia)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TerceroVentanaCobranzaDto>>(mapper.Map<IReadOnlyList<TerceroVentanaCobranzaDto>>(ventanas));
    }
}
