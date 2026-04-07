using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public record GetTerceroUsuarioClienteQuery(long TerceroId)
    : IRequest<Result<TerceroUsuarioClienteDto?>>;

public class GetTerceroUsuarioClienteQueryHandler(
    ITerceroRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetTerceroUsuarioClienteQuery, Result<TerceroUsuarioClienteDto?>>
{
    public async Task<Result<TerceroUsuarioClienteDto?>> Handle(GetTerceroUsuarioClienteQuery request, CancellationToken ct)
    {
        var tercero = await repo.GetByIdAsync(request.TerceroId, ct);
        if (tercero is null)
            return Result.Failure<TerceroUsuarioClienteDto?>($"No se encontró el tercero con Id {request.TerceroId}.");

        var dto = await TerceroUsuarioClienteReadModelLoader.LoadAsync(db, tercero.UsuarioId, ct);
        return Result.Success(dto);
    }
}
