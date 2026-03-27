using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Api.Security;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

[RequirePermission("SEGURIDAD.ADMIN")]
[AuditCriticalOperation("SEGURIDAD_ADMIN")]
public class SeguridadController(
    IMediator mediator,
    ISeguridadRepository repo,
    IApplicationDbContext db,
    EndpointDataSource endpointDataSource)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todos los permisos/funciones del sistema registrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var permisos = await db.Seguridad
            .AsNoTracking()
            .OrderBy(x => x.Identificador)
            .Select(x => new
            {
                x.Id,
                x.Identificador,
                x.Descripcion,
                x.AplicaSeguridadPorUsuario
            })
            .ToListAsync(ct);

        return Ok(permisos);
    }

    /// <summary>
    /// Crea un nuevo permiso del sistema.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSeguridadRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateSeguridadCommand(
                request.Identificador,
                request.Descripcion,
                request.AplicaSeguridadPorUsuario),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value });
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico.
    /// </summary>
    [HttpGet("verificar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Verificar(
        [FromQuery] long usuarioId,
        [FromQuery] string identificador,
        CancellationToken ct)
    {
        var tiene = await repo.TienePermisoAsync(usuarioId, identificador, ct);
        return Ok(new
        {
            usuarioId,
            identificador,
            tienePermiso = tiene
        });
    }

    /// <summary>
    /// Retorna el mapa completo de permisos de un usuario (clave → valor).
    /// </summary>
    [HttpGet("usuario/{usuarioId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermisosUsuario(
        long usuarioId,
        CancellationToken ct)
    {
        var permisos = await repo.GetPermisosUsuarioAsync(usuarioId, ct);
        return Ok(permisos);
    }

    [HttpGet("matriz-endpoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMatrizEndpoints()
    {
        var items = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Select(x => new
            {
                Route = x.RoutePattern.RawText,
                HttpMethods = x.Metadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods).Distinct().OrderBy(m => m),
                Autoriza = x.Metadata.OfType<IAuthorizeData>().Any(),
                Permisos = x.Metadata.OfType<RequirePermissionAttribute>().Select(p => p.Permission).Distinct().OrderBy(p => p)
            })
            .OrderBy(x => x.Route)
            .ToList();

        return Ok(items);
    }

    [HttpGet("usuario/{usuarioId:long}/matriz-endpoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMatrizEndpointsUsuario(long usuarioId, CancellationToken ct)
    {
        var permisosUsuario = await repo.GetPermisosUsuarioAsync(usuarioId, ct);
        var items = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Select(x =>
            {
                var permisos = x.Metadata.OfType<RequirePermissionAttribute>().Select(p => p.Permission).Distinct().OrderBy(p => p).ToList();
                return new
                {
                    Route = x.RoutePattern.RawText,
                    HttpMethods = x.Metadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods).Distinct().OrderBy(m => m),
                    Autoriza = x.Metadata.OfType<IAuthorizeData>().Any(),
                    Permisos = permisos,
                    Acceso = permisos.Count == 0 || permisos.All(p => !permisosUsuario.TryGetValue(p, out var allowed) || allowed)
                };
            })
            .OrderBy(x => x.Route)
            .ToList();

        return Ok(items);
    }

    [HttpGet("perfiles-operativos-riesgo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerfilesOperativosRiesgo([FromQuery] int minPermisos = 10, CancellationToken ct = default)
    {
        var threshold = Math.Max(1, minPermisos);
        var usuarios = await db.Usuarios.AsNoTracking().ToListAsync(ct);
        var usuarioIds = usuarios.Select(x => x.Id).ToList();

        var permisosAsignados = await db.SeguridadUsuario.AsNoTracking()
            .Where(x => usuarioIds.Contains(x.UsuarioId) && x.Valor)
            .GroupBy(x => x.UsuarioId)
            .Select(g => new { UsuarioId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.UsuarioId, x => x.Cantidad, ct);

        var sucursales = await db.UsuariosSucursal.AsNoTracking()
            .Where(x => usuarioIds.Contains(x.UsuarioId))
            .GroupBy(x => x.UsuarioId)
            .Select(g => new { UsuarioId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.UsuarioId, x => x.Cantidad, ct);

        var menus = await db.MenuUsuario.AsNoTracking()
            .Where(x => usuarioIds.Contains(x.UsuarioId))
            .GroupBy(x => x.UsuarioId)
            .Select(g => new { UsuarioId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.UsuarioId, x => x.Cantidad, ct);

        var parametros = await db.ParametrosUsuario.AsNoTracking()
            .Where(x => usuarioIds.Contains(x.UsuarioId))
            .GroupBy(x => x.UsuarioId)
            .Select(g => new { UsuarioId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.UsuarioId, x => x.Cantidad, ct);

        var items = usuarios.Select(x =>
        {
            var permisos = permisosAsignados.GetValueOrDefault(x.Id);
            var cantidadSucursales = sucursales.GetValueOrDefault(x.Id);
            var cantidadMenu = menus.GetValueOrDefault(x.Id);
            var cantidadParametros = parametros.GetValueOrDefault(x.Id);

            var issues = new List<string>();
            if (!x.Activo && permisos > 0)
                issues.Add("Usuario inactivo con permisos personalizados habilitados.");
            if (x.Activo && cantidadSucursales == 0)
                issues.Add("Usuario activo sin sucursales asignadas.");
            if (x.Activo && cantidadMenu == 0)
                issues.Add("Usuario activo sin menú asignado.");
            if (permisos >= threshold)
                issues.Add($"Usuario con {permisos} permisos personalizados habilitados.");

            return new
            {
                x.Id,
                x.UserName,
                x.NombreCompleto,
                x.Email,
                x.Activo,
                CantidadPermisosPersonalizados = permisos,
                CantidadSucursales = cantidadSucursales,
                CantidadMenu = cantidadMenu,
                CantidadParametros = cantidadParametros,
                Riesgo = issues.Count,
                Issues = issues.AsReadOnly()
            };
        })
        .Where(x => x.Riesgo > 0)
        .OrderByDescending(x => x.Riesgo)
        .ThenByDescending(x => x.CantidadPermisosPersonalizados)
        .ThenBy(x => x.UserName)
        .ToList();

        return Ok(new
        {
            threshold,
            CantidadUsuarios = usuarios.Count,
            UsuariosConRiesgo = items.Count,
            Items = items
        });
    }
}

public record CreateSeguridadRequest(
    string Identificador,
    string? Descripcion,
    bool AplicaSeguridadPorUsuario);