using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

internal static class TerceroUsuarioClienteReadModelLoader
{
    public static async Task<TerceroUsuarioClienteDto?> LoadAsync(
        IApplicationDbContext db,
        long? usuarioId,
        CancellationToken ct)
    {
        if (!usuarioId.HasValue)
            return null;

        var usuario = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == usuarioId.Value, ct);

        if (usuario is null)
            return null;

        var grupo = await db.UsuariosXUsuario
            .AsNoTracking()
            .Where(x => x.UsuarioMiembroId == usuario.Id)
            .Join(
                db.Usuarios.AsNoTracking(),
                rel => rel.UsuarioGrupoId,
                groupUser => groupUser.Id,
                (rel, groupUser) => new { rel.UsuarioGrupoId, groupUser.UserName })
            .FirstOrDefaultAsync(ct);

        var parametros = await db.ParametrosUsuario
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuario.Id)
            .Where(x => x.Clave == "DEFAULT_SUCURSAL_ID" || x.Clave == "DEFAULT_LAYOUT_PROFILE")
            .ToDictionaryAsync(x => x.Clave, x => x.Valor, ct);

        long? defaultSucursalId = null;
        string? defaultSucursalDescripcion = null;
        if (parametros.TryGetValue("DEFAULT_SUCURSAL_ID", out var defaultSucursalValue) &&
            long.TryParse(defaultSucursalValue, out var parsedSucursalId))
        {
            defaultSucursalId = parsedSucursalId;
            defaultSucursalDescripcion = await db.Sucursales
                .AsNoTracking()
                .Where(x => x.Id == parsedSucursalId)
                .Select(x => x.RazonSocial)
                .FirstOrDefaultAsync(ct);
        }

        var permisos = await (
            from s in db.Seguridad.AsNoTracking()
            join su in db.SeguridadUsuario.AsNoTracking()
                on new { SeguridadId = s.Id, UsuarioId = usuario.Id }
                equals new { su.SeguridadId, su.UsuarioId }
                into suJoin
            from su in suJoin.DefaultIfEmpty()
            where s.AplicaSeguridadPorUsuario
            orderby s.Identificador
            select new TerceroUsuarioClientePermisoDto
            {
                SeguridadId = s.Id,
                Identificador = s.Identificador,
                Descripcion = s.Descripcion,
                Valor = su != null && su.Valor
            })
            .ToListAsync(ct);

        return new TerceroUsuarioClienteDto
        {
            UsuarioId = usuario.Id,
            UserName = usuario.UserName,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email,
            Activo = usuario.Activo && usuario.DeletedAt == null,
            TienePasswordConfigurada = !string.IsNullOrWhiteSpace(usuario.PasswordHash),
            UsuarioGrupoId = grupo?.UsuarioGrupoId,
            UsuarioGrupoUserName = grupo?.UserName,
            ParametrosBasicos = new TerceroUsuarioClienteParametrosBasicosDto
            {
                DefaultSucursalId = defaultSucursalId,
                DefaultSucursalDescripcion = defaultSucursalDescripcion,
                DefaultLayoutProfile = parametros.GetValueOrDefault("DEFAULT_LAYOUT_PROFILE")
            },
            PermisosBasicos = permisos.AsReadOnly()
        };
    }
}
