using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class SetTerceroUsuarioClientePermisosCommandHandler(
    ITerceroRepository terceroRepo,
    IApplicationDbContext db,
    IMediator mediator)
    : IRequestHandler<SetTerceroUsuarioClientePermisosCommand, Result<TerceroUsuarioClienteDto>>
{
    public async Task<Result<TerceroUsuarioClienteDto>> Handle(SetTerceroUsuarioClientePermisosCommand request, CancellationToken ct)
    {
        var tercero = await terceroRepo.GetByIdAsync(request.TerceroId, ct);
        if (tercero is null)
            return Result.Failure<TerceroUsuarioClienteDto>($"No se encontró el tercero con Id {request.TerceroId}.");

        if (!tercero.UsuarioId.HasValue)
            return Result.Failure<TerceroUsuarioClienteDto>("El tercero no tiene un usuario vinculado.");

        var seguridadIds = request.Permisos.Select(x => x.SeguridadId).Distinct().ToList();
        if (seguridadIds.Count > 0)
        {
            var permisosValidos = await db.Seguridad
                .AsNoTracking()
                .Where(x => seguridadIds.Contains(x.Id) && x.AplicaSeguridadPorUsuario)
                .Select(x => x.Id)
                .ToListAsync(ct);

            var idsInvalidos = seguridadIds.Except(permisosValidos).ToList();
            if (idsInvalidos.Count > 0)
                return Result.Failure<TerceroUsuarioClienteDto>("Uno o más permisos básicos indicados no existen o no aplican por usuario.");
        }

        foreach (var permiso in request.Permisos)
        {
            var result = await mediator.Send(
                new SetPermisoUsuarioCommand(tercero.UsuarioId.Value, permiso.SeguridadId, permiso.Valor),
                ct);

            if (result.IsFailure)
                return Result.Failure<TerceroUsuarioClienteDto>(result.Error ?? "No se pudo actualizar los permisos del usuario vinculado.");
        }

        var dto = await TerceroUsuarioClienteReadModelLoader.LoadAsync(db, tercero.UsuarioId, ct);
        return Result.Success(dto!);
    }
}
