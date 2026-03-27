using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroPerfilComercialQueryHandler(
    IApplicationDbContext db,
    IMapper mapper) : IRequestHandler<GetTerceroPerfilComercialQuery, Result<TerceroPerfilComercialDto>>
{
    public async Task<Result<TerceroPerfilComercialDto>> Handle(GetTerceroPerfilComercialQuery request, CancellationToken ct)
    {
        var terceroExists = await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (!terceroExists)
            return Result.Failure<TerceroPerfilComercialDto>($"No se encontró el tercero con Id {request.TerceroId}.");

        var perfil = await db.TercerosPerfilesComerciales
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TerceroId == request.TerceroId && x.DeletedAt == null, ct);

        if (perfil is null)
            return Result.Success(new TerceroPerfilComercialDto
            {
                TerceroId = request.TerceroId,
                RiesgoCrediticio = RiesgoCrediticioComercial.Normal.ToString().ToUpperInvariant()
            });

        var dto = mapper.Map<TerceroPerfilComercialDto>(perfil);

        if (perfil.ZonaComercialId.HasValue)
        {
            dto.ZonaComercialDescripcion = await db.ZonasComerciales
                .AsNoTracking()
                .Where(x => x.Id == perfil.ZonaComercialId.Value)
                .Select(x => x.Descripcion)
                .FirstOrDefaultAsync(ct);
        }

        return Result.Success(dto);
    }
}
