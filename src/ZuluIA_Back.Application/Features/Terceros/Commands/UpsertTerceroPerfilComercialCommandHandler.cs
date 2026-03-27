using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpsertTerceroPerfilComercialCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow,
    IMapper mapper) : IRequestHandler<UpsertTerceroPerfilComercialCommand, Result<TerceroPerfilComercialDto>>
{
    public async Task<Result<TerceroPerfilComercialDto>> Handle(UpsertTerceroPerfilComercialCommand request, CancellationToken ct)
    {
        var tercero = await db.Terceros.FirstOrDefaultAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (tercero is null)
            return Result.Failure<TerceroPerfilComercialDto>($"No se encontró el tercero con Id {request.TerceroId}.");

        if (request.ZonaComercialId.HasValue)
        {
            var zonaExists = await db.ZonasComerciales
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.ZonaComercialId.Value && x.DeletedAt == null && x.Activo, ct);

            if (!zonaExists)
                return Result.Failure<TerceroPerfilComercialDto>("La zona comercial indicada no existe o está inactiva.");
        }

        if (!Enum.TryParse<RiesgoCrediticioComercial>(request.RiesgoCrediticio, true, out var riesgo))
            return Result.Failure<TerceroPerfilComercialDto>("El riesgo crediticio indicado no es válido.");

        var perfil = await db.TercerosPerfilesComerciales
            .FirstOrDefaultAsync(x => x.TerceroId == request.TerceroId && x.DeletedAt == null, ct);

        if (perfil is null)
        {
            perfil = TerceroPerfilComercial.Crear(request.TerceroId, currentUser.UserId);
            db.TercerosPerfilesComerciales.Add(perfil);
        }

        try
        {
            perfil.Actualizar(
                request.ZonaComercialId,
                request.Rubro,
                request.Subrubro,
                request.Sector,
                request.CondicionCobranza,
                riesgo,
                request.SaldoMaximoVigente,
                request.VigenciaSaldo,
                request.CondicionVenta,
                request.PlazoCobro,
                request.FacturadorPorDefecto,
                request.MinimoFacturaMipymes,
                request.ObservacionComercial,
                currentUser.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<TerceroPerfilComercialDto>(ex.Message);
        }

        await uow.SaveChangesAsync(ct);

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
