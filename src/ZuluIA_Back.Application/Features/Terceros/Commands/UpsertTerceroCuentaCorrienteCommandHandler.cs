using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpsertTerceroCuentaCorrienteCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow) : IRequestHandler<UpsertTerceroCuentaCorrienteCommand, Result<TerceroCuentaCorrienteDto>>
{
    public async Task<Result<TerceroCuentaCorrienteDto>> Handle(UpsertTerceroCuentaCorrienteCommand request, CancellationToken ct)
    {
        var tercero = await db.Terceros
            .FirstOrDefaultAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);

        if (tercero is null)
            return Result.Failure<TerceroCuentaCorrienteDto>($"No se encontró el tercero con Id {request.TerceroId}.");

        var perfil = await db.TercerosPerfilesComerciales
            .FirstOrDefaultAsync(x => x.TerceroId == request.TerceroId && x.DeletedAt == null, ct);

        if (perfil is null)
        {
            perfil = TerceroPerfilComercial.Crear(request.TerceroId, currentUser.UserId);
            db.TercerosPerfilesComerciales.Add(perfil);
        }

        try
        {
            tercero.ActualizarCuentaCorriente(
                request.LimiteCreditoTotal,
                request.VigenciaLimiteCreditoTotalDesde,
                request.VigenciaLimiteCreditoTotalHasta,
                currentUser.UserId);

            perfil.ActualizarCuentaCorriente(
                request.LimiteSaldo,
                request.VigenciaLimiteSaldoDesde,
                request.VigenciaLimiteSaldoHasta,
                currentUser.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<TerceroCuentaCorrienteDto>(ex.Message);
        }

        await uow.SaveChangesAsync(ct);

        return Result.Success(TerceroCuentaCorrienteReadModelLoader.Load(tercero, perfil));
    }
}
