using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class RemoveTerceroUsuarioClienteCommandHandler(
    ITerceroRepository terceroRepo,
    IUsuarioRepository usuarioRepo,
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow)
    : IRequestHandler<RemoveTerceroUsuarioClienteCommand, Result>
{
    public async Task<Result> Handle(RemoveTerceroUsuarioClienteCommand request, CancellationToken ct)
    {
        var tercero = await terceroRepo.GetByIdAsync(request.TerceroId, ct);
        if (tercero is null)
            return Result.Failure($"No se encontró el tercero con Id {request.TerceroId}.");

        if (!tercero.UsuarioId.HasValue)
            return Result.Success();

        var usuario = await usuarioRepo.GetByIdAsync(tercero.UsuarioId.Value, ct);
        if (usuario is not null)
        {
            var relacionesGrupo = await db.UsuariosXUsuario
                .Where(x => x.UsuarioMiembroId == usuario.Id)
                .ToListAsync(ct);

            if (relacionesGrupo.Count > 0)
                db.UsuariosXUsuario.RemoveRange(relacionesGrupo);

            usuario.Desactivar(currentUser.UserId);
            usuarioRepo.Update(usuario);
        }

        tercero.SetUsuario(null, currentUser.UserId);
        terceroRepo.Update(tercero);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
