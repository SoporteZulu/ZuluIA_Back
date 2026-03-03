using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;

namespace ZuluIA_Back.Application.Features.Usuarios.Queries;

public class GetPermisosUsuarioQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPermisosUsuarioQuery, IReadOnlyList<PermisoDto>>
{
    public async Task<IReadOnlyList<PermisoDto>> Handle(
        GetPermisosUsuarioQuery request,
        CancellationToken ct)
    {
        var permisos = await (
            from s in db.Seguridad.AsNoTracking()
            join su in db.SeguridadUsuario.AsNoTracking()
                on new { SeguridadId = s.Id, UsuarioId = request.UsuarioId }
                equals new { su.SeguridadId, su.UsuarioId }
                into suJoin
            from su in suJoin.DefaultIfEmpty()
            orderby s.Identificador
            select new PermisoDto
            {
                SeguridadId               = s.Id,
                Identificador             = s.Identificador,
                Descripcion               = s.Descripcion,
                AplicaSeguridadPorUsuario = s.AplicaSeguridadPorUsuario,
                Valor = s.AplicaSeguridadPorUsuario
                    ? (su != null && su.Valor)
                    : true
            })
            .ToListAsync(ct);

        return permisos.AsReadOnly();
    }
}