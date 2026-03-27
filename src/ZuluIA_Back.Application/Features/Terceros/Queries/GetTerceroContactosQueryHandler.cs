using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroContactosQueryHandler(
    IApplicationDbContext db,
    IMapper mapper) : IRequestHandler<GetTerceroContactosQuery, Result<IReadOnlyList<TerceroContactoDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroContactoDto>>> Handle(GetTerceroContactosQuery request, CancellationToken ct)
    {
        var terceroExists = await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (!terceroExists)
            return Result.Failure<IReadOnlyList<TerceroContactoDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var contactos = await db.TercerosContactos
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TerceroContactoDto>>(mapper.Map<IReadOnlyList<TerceroContactoDto>>(contactos));
    }
}
