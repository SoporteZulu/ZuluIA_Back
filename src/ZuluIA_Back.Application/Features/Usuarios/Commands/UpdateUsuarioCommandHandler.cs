using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class UpdateUsuarioCommandHandler(
    IUsuarioRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateUsuarioCommand, Result>
{
    public async Task<Result> Handle(UpdateUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await repo.GetByIdConSucursalesAsync(request.Id, ct);
        if (usuario is null)
            return Result.Failure($"No se encontró el usuario con ID {request.Id}.");

        usuario.Actualizar(request.NombreCompleto, request.Email, currentUser.UserId);

        // Sincronizar sucursales
        var actuales = usuario.Sucursales.Select(x => x.SucursalId).ToHashSet();
        var nuevas = request.SucursalIds.ToHashSet();
        var agregar = nuevas.Except(actuales);
        var quitar = actuales.Except(nuevas);

        foreach (var id in agregar) usuario.AsignarSucursal(id);
        foreach (var id in quitar) usuario.RemoverSucursal(id);

        repo.Update(usuario);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}