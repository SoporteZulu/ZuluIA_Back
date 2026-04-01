using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public record GetTerceroMediosContactoQuery(long TerceroId) : IRequest<Result<IReadOnlyList<TerceroMedioContactoDto>>>;

public class GetTerceroMediosContactoQueryHandler(
    IApplicationDbContext db,
    ILogger<GetTerceroMediosContactoQueryHandler> logger) : IRequestHandler<GetTerceroMediosContactoQuery, Result<IReadOnlyList<TerceroMedioContactoDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroMedioContactoDto>>> Handle(GetTerceroMediosContactoQuery request, CancellationToken ct)
    {
        var terceroExists = await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (!terceroExists)
            return Result.Failure<IReadOnlyList<TerceroMedioContactoDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var mediosContacto = await TerceroMedioContactoReadModelLoader.LoadAsync(db, request.TerceroId, logger, ct);
        return Result.Success(mediosContacto);
    }
}
