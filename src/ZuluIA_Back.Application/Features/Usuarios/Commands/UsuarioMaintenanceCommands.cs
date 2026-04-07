using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record AddUsuarioRelacionadoCommand(long UsuarioId, long? UsuarioMiembroId, long? UsuarioGrupoId)
    : IRequest<Result<long>>;

public record DeleteUsuarioRelacionadoCommand(long UsuarioId, long UsuarioRelacionadoId)
    : IRequest<Result>;

public class AddUsuarioRelacionadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddUsuarioRelacionadoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddUsuarioRelacionadoCommand request, CancellationToken ct)
    {
        var usuarioMiembroId = request.UsuarioMiembroId ?? request.UsuarioId;
        var usuarioGrupoId = request.UsuarioGrupoId ?? request.UsuarioId;

        var existe = await db.UsuariosXUsuario.AnyAsync(
            u => u.UsuarioMiembroId == usuarioMiembroId && u.UsuarioGrupoId == usuarioGrupoId,
            ct);

        if (existe)
            return Result.Failure<long>("La relacion ya existe.");

        UsuarioXUsuario relacion;
        try
        {
            relacion = UsuarioXUsuario.Crear(usuarioMiembroId, usuarioGrupoId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.UsuariosXUsuario.Add(relacion);
        await db.SaveChangesAsync(ct);

        return Result.Success(relacion.Id);
    }
}

public class DeleteUsuarioRelacionadoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteUsuarioRelacionadoCommand, Result>
{
    public async Task<Result> Handle(DeleteUsuarioRelacionadoCommand request, CancellationToken ct)
    {
        var relacion = await db.UsuariosXUsuario.FirstOrDefaultAsync(
            u => u.Id == request.UsuarioRelacionadoId
              && (u.UsuarioMiembroId == request.UsuarioId || u.UsuarioGrupoId == request.UsuarioId),
            ct);

        if (relacion is null)
            return Result.Failure("Relacion no encontrada.");

        db.UsuariosXUsuario.Remove(relacion);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public record AddUsuarioMenuItemCommand(long UsuarioId, long MenuItemId)
    : IRequest<Result<long>>;

public record RemoveUsuarioMenuItemCommand(long UsuarioId, long MenuItemId)
    : IRequest<Result>;

public class AddUsuarioMenuItemCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddUsuarioMenuItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddUsuarioMenuItemCommand request, CancellationToken ct)
    {
        var existe = await db.MenuUsuario.AnyAsync(
            m => m.UsuarioId == request.UsuarioId && m.MenuId == request.MenuItemId,
            ct);

        if (existe)
            return Result.Failure<long>("El item ya esta asignado al usuario.");

        var item = MenuUsuario.Crear(request.MenuItemId, request.UsuarioId);
        db.MenuUsuario.Add(item);
        await db.SaveChangesAsync(ct);

        return Result.Success(item.Id);
    }
}

public class RemoveUsuarioMenuItemCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RemoveUsuarioMenuItemCommand, Result>
{
    public async Task<Result> Handle(RemoveUsuarioMenuItemCommand request, CancellationToken ct)
    {
        var item = await db.MenuUsuario.FirstOrDefaultAsync(
            m => m.UsuarioId == request.UsuarioId && m.MenuId == request.MenuItemId,
            ct);

        if (item is null)
            return Result.Failure("El item no esta asignado al usuario.");

        db.MenuUsuario.Remove(item);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AddUsuarioRelacionadoCommandValidator : AbstractValidator<AddUsuarioRelacionadoCommand>
{
    public AddUsuarioRelacionadoCommandValidator()
    {
        RuleFor(x => x.UsuarioId).GreaterThan(0);
    }
}

public class DeleteUsuarioRelacionadoCommandValidator : AbstractValidator<DeleteUsuarioRelacionadoCommand>
{
    public DeleteUsuarioRelacionadoCommandValidator()
    {
        RuleFor(x => x.UsuarioId).GreaterThan(0);
        RuleFor(x => x.UsuarioRelacionadoId).GreaterThan(0);
    }
}

public class AddUsuarioMenuItemCommandValidator : AbstractValidator<AddUsuarioMenuItemCommand>
{
    public AddUsuarioMenuItemCommandValidator()
    {
        RuleFor(x => x.UsuarioId).GreaterThan(0);
        RuleFor(x => x.MenuItemId).GreaterThan(0);
    }
}

public class RemoveUsuarioMenuItemCommandValidator : AbstractValidator<RemoveUsuarioMenuItemCommand>
{
    public RemoveUsuarioMenuItemCommandValidator()
    {
        RuleFor(x => x.UsuarioId).GreaterThan(0);
        RuleFor(x => x.MenuItemId).GreaterThan(0);
    }
}
