using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class DeleteUsuarioCommandHandler(
    IUsuarioRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteUsuarioCommand, Result>
{
    public async Task<Result> Handle(DeleteUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await repo.GetByIdAsync(request.Id, ct);
        if (usuario is null)
            return Result.Failure($"No se encontró el usuario con ID {request.Id}.");

        usuario.Desactivar(currentUser.UserId);
        repo.Update(usuario);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}