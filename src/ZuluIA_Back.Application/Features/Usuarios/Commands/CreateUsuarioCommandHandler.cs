using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class CreateUsuarioCommandHandler(
    IUsuarioRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IPasswordHasherService passwordHasher)
    : IRequestHandler<CreateUsuarioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateUsuarioCommand request, CancellationToken ct)
    {
        if (await repo.ExisteUserNameAsync(request.UserName, null, ct))
            return Result.Failure<long>(
                $"Ya existe un usuario con el nombre '{request.UserName}'.");

        var usuario = Usuario.Crear(
            request.UserName,
            request.NombreCompleto,
            request.Email,
            request.SupabaseUserId,
            currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(request.Password))
            usuario.EstablecerPasswordHash(passwordHasher.HashPassword(request.Password), currentUser.UserId);

        foreach (var sucursalId in request.SucursalIds)
            usuario.AsignarSucursal(sucursalId);

        await repo.AddAsync(usuario, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(usuario.Id);
    }
}