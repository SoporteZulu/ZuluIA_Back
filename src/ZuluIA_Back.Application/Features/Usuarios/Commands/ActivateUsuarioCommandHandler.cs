using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class ActivateUsuarioCommandHandler(
    IUsuarioRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivateUsuarioCommand, Result>
{
    public async Task<Result> Handle(ActivateUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await repo.GetByIdAsync(request.Id, ct);
        if (usuario is null)
            return Result.Failure($"No se encontró el usuario con ID {request.Id}.");

        usuario.Activar(currentUser.UserId);
        repo.Update(usuario);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}