using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class SetPermisoUsuarioCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<SetPermisoUsuarioCommand, Result>
{
    public async Task<Result> Handle(SetPermisoUsuarioCommand request, CancellationToken ct)
    {
        var existing = await db.SeguridadUsuario
            .FirstOrDefaultAsync(x =>
                x.SeguridadId == request.SeguridadId &&
                x.UsuarioId   == request.UsuarioId,
                ct);

        if (existing is not null)
            existing.SetValor(request.Valor);
        else
            await db.SeguridadUsuario.AddAsync(
                SeguridadUsuario.Crear(request.SeguridadId, request.UsuarioId, request.Valor),
                ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}