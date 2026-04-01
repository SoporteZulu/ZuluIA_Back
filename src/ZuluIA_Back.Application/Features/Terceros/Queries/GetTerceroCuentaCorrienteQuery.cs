using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Retorna la configuración de cuenta corriente del tercero.
/// </summary>
public record GetTerceroCuentaCorrienteQuery(long TerceroId) : IRequest<Result<TerceroCuentaCorrienteDto>>;

public class GetTerceroCuentaCorrienteQueryHandler(
    IApplicationDbContext db)
    : IRequestHandler<GetTerceroCuentaCorrienteQuery, Result<TerceroCuentaCorrienteDto>>
{
    public async Task<Result<TerceroCuentaCorrienteDto>> Handle(GetTerceroCuentaCorrienteQuery request, CancellationToken ct)
    {
        var tercero = await db.Terceros
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);

        if (tercero is null)
            return Result.Failure<TerceroCuentaCorrienteDto>($"No se encontró el tercero con Id {request.TerceroId}.");

        var perfil = await db.TercerosPerfilesComerciales
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TerceroId == request.TerceroId && x.DeletedAt == null, ct);

        return Result.Success(TerceroCuentaCorrienteReadModelLoader.Load(tercero, perfil));
    }
}
