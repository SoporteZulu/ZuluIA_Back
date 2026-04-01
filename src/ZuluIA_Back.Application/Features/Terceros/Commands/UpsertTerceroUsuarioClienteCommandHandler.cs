using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpsertTerceroUsuarioClienteCommandHandler(
    ITerceroRepository terceroRepo,
    IUsuarioRepository usuarioRepo,
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IPasswordHasherService passwordHasher,
    IUnitOfWork uow)
    : IRequestHandler<UpsertTerceroUsuarioClienteCommand, Result<TerceroUsuarioClienteDto>>
{
    public async Task<Result<TerceroUsuarioClienteDto>> Handle(UpsertTerceroUsuarioClienteCommand request, CancellationToken ct)
    {
        var tercero = await terceroRepo.GetByIdAsync(request.TerceroId, ct);
        if (tercero is null)
            return Result.Failure<TerceroUsuarioClienteDto>($"No se encontró el tercero con Id {request.TerceroId}.");

        Usuario? usuario = null;
        if (tercero.UsuarioId.HasValue)
        {
            usuario = await usuarioRepo.GetByIdAsync(tercero.UsuarioId.Value, ct);
            if (usuario is null)
                tercero.SetUsuario(null, currentUser.UserId);
        }

        var normalizedUserName = ResolveUserName(request.UserName, tercero, usuario);
        if (await usuarioRepo.ExisteUserNameAsync(normalizedUserName, usuario?.Id, ct))
            return Result.Failure<TerceroUsuarioClienteDto>($"Ya existe un usuario con el nombre '{normalizedUserName}'.");

        Usuario? usuarioGrupo = null;
        if (request.UsuarioGrupoId.HasValue)
        {
            usuarioGrupo = await usuarioRepo.GetByIdAsync(request.UsuarioGrupoId.Value, ct);
            if (usuarioGrupo is null || !usuarioGrupo.Activo || usuarioGrupo.DeletedAt != null)
                return Result.Failure<TerceroUsuarioClienteDto>("El usuario grupo indicado no existe o está inactivo.");
        }

        if (usuario is null)
        {
            usuario = Usuario.Crear(
                normalizedUserName,
                BuildNombreCompleto(tercero),
                tercero.Email,
                null,
                currentUser.UserId);

            if (tercero.SucursalId.HasValue)
                usuario.AsignarSucursal(tercero.SucursalId.Value);

            await usuarioRepo.AddAsync(usuario, ct);
            await uow.SaveChangesAsync(ct);
            tercero.SetUsuario(usuario.Id, currentUser.UserId);
        }
        else
        {
            usuario.SetUserName(normalizedUserName, currentUser.UserId);
            usuario.Actualizar(BuildNombreCompleto(tercero), tercero.Email, currentUser.UserId);
            usuarioRepo.Update(usuario);
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
            usuario.EstablecerPasswordHash(passwordHasher.HashPassword(request.Password), currentUser.UserId);

        var relacionesGrupo = await db.UsuariosXUsuario
            .Where(x => x.UsuarioMiembroId == usuario.Id)
            .ToListAsync(ct);

        if (relacionesGrupo.Count > 0)
            db.UsuariosXUsuario.RemoveRange(relacionesGrupo);

        if (usuarioGrupo is not null)
            db.UsuariosXUsuario.Add(UsuarioXUsuario.Crear(usuario.Id, usuarioGrupo.Id));

        terceroRepo.Update(tercero);
        usuarioRepo.Update(usuario);
        await uow.SaveChangesAsync(ct);

        var dto = await TerceroUsuarioClienteReadModelLoader.LoadAsync(db, usuario.Id, ct);
        return Result.Success(dto!);
    }

    private static string ResolveUserName(string? requestedUserName, Domain.Entities.Terceros.Tercero tercero, Usuario? usuario)
    {
        if (!string.IsNullOrWhiteSpace(requestedUserName))
            return requestedUserName.Trim().ToLowerInvariant();

        if (usuario is not null)
            return usuario.UserName;

        return $"c{tercero.Legajo.ToLowerInvariant()}-{tercero.Id}";
    }

    private static string BuildNombreCompleto(Domain.Entities.Terceros.Tercero tercero)
    {
        if (tercero.TipoPersoneria == Domain.Enums.TipoPersoneriaTercero.Fisica)
        {
            var nombre = string.Join(' ', new[] { tercero.Nombre, tercero.Apellido }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (!string.IsNullOrWhiteSpace(nombre))
                return nombre;
        }

        return tercero.RazonSocial;
    }
}
