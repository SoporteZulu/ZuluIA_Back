using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class SetTerceroUsuarioClienteParametrosBasicosCommandHandler(
    ITerceroRepository terceroRepo,
    IApplicationDbContext db,
    IMediator mediator)
    : IRequestHandler<SetTerceroUsuarioClienteParametrosBasicosCommand, Result<TerceroUsuarioClienteDto>>
{
    public async Task<Result<TerceroUsuarioClienteDto>> Handle(SetTerceroUsuarioClienteParametrosBasicosCommand request, CancellationToken ct)
    {
        var tercero = await terceroRepo.GetByIdAsync(request.TerceroId, ct);
        if (tercero is null)
            return Result.Failure<TerceroUsuarioClienteDto>($"No se encontró el tercero con Id {request.TerceroId}.");

        if (!tercero.UsuarioId.HasValue)
            return Result.Failure<TerceroUsuarioClienteDto>("El tercero no tiene un usuario vinculado.");

        if (request.DefaultSucursalId.HasValue)
        {
            var sucursalExiste = await db.Sucursales
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.DefaultSucursalId.Value, ct);

            if (!sucursalExiste)
                return Result.Failure<TerceroUsuarioClienteDto>("La sucursal por defecto indicada no existe.");
        }

        var defaultSucursalResult = await mediator.Send(
            new SetParametroUsuarioCommand(
                tercero.UsuarioId.Value,
                "DEFAULT_SUCURSAL_ID",
                request.DefaultSucursalId?.ToString()),
            ct);

        if (defaultSucursalResult.IsFailure)
            return Result.Failure<TerceroUsuarioClienteDto>(defaultSucursalResult.Error ?? "No se pudo actualizar la sucursal por defecto del usuario vinculado.");

        var layoutResult = await mediator.Send(
            new SetParametroUsuarioCommand(
                tercero.UsuarioId.Value,
                "DEFAULT_LAYOUT_PROFILE",
                string.IsNullOrWhiteSpace(request.DefaultLayoutProfile) ? null : request.DefaultLayoutProfile.Trim()),
            ct);

        if (layoutResult.IsFailure)
            return Result.Failure<TerceroUsuarioClienteDto>(layoutResult.Error ?? "No se pudo actualizar el layout por defecto del usuario vinculado.");

        var dto = await TerceroUsuarioClienteReadModelLoader.LoadAsync(db, tercero.UsuarioId, ct);
        return Result.Success(dto!);
    }
}
