using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroSucursalesEntregaQueryHandler(
    IApplicationDbContext db,
    IMapper mapper) : IRequestHandler<GetTerceroSucursalesEntregaQuery, Result<IReadOnlyList<TerceroSucursalEntregaDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroSucursalEntregaDto>>> Handle(GetTerceroSucursalesEntregaQuery request, CancellationToken ct)
    {
        var terceroExists = await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (!terceroExists)
            return Result.Failure<IReadOnlyList<TerceroSucursalEntregaDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var sucursales = await db.TercerosSucursalesEntrega
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Descripcion)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TerceroSucursalEntregaDto>>(mapper.Map<IReadOnlyList<TerceroSucursalEntregaDto>>(sucursales));
    }
}
