using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class SetMenuUsuarioCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<SetMenuUsuarioCommand, Result>
{
    public async Task<Result> Handle(SetMenuUsuarioCommand request, CancellationToken ct)
    {
        // Eliminar asignaciones existentes
        var existentes = await db.MenuUsuario
            .Where(x => x.UsuarioId == request.UsuarioId)
            .ToListAsync(ct);

        db.MenuUsuario.RemoveRange(existentes);

        // Agregar las nuevas
        var nuevos = request.MenuIds
            .Distinct()
            .Select(menuId => MenuUsuario.Crear(menuId, request.UsuarioId))
            .ToList();

        await db.MenuUsuario.AddRangeAsync(nuevos, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}