using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Api.Security;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Application.Features.Usuarios.Queries;

namespace ZuluIA_Back.Api.Controllers;

[RequirePermission("USUARIOS.ADMIN")]
[AuditCriticalOperation("USUARIOS_ADMIN")]
public class UsuariosController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Retorna la lista paginada de usuarios.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? soloActivos = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetUsuariosPagedQuery(page, pageSize, search, soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle de un usuario con sus sucursales asignadas.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetUsuarioById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetUsuarioByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna el menú del usuario construido como árbol jerárquico.
    /// </summary>
    [HttpGet("{id:long}/menu")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMenu(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMenuUsuarioQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna todos los permisos del sistema con su valor para el usuario.
    /// </summary>
    [HttpGet("{id:long}/permisos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermisos(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPermisosUsuarioQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna los parámetros personalizados de un usuario.
    /// </summary>
    [HttpGet("{id:long}/parametros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParametros(
        long id,
        [FromServices] Domain.Interfaces.IParametroUsuarioRepository repo,
        CancellationToken ct)
    {
        var parametros = await repo.GetByUsuarioAsync(id, ct);
        return Ok(parametros.Select(p => new
        {
            p.Id,
            p.UsuarioId,
            p.Clave,
            p.Valor
        }));
    }

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUsuarioCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetUsuarioById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza datos y sucursales de un usuario.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateUsuarioCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva un usuario (soft delete).
    /// </summary>
    [HttpDelete("{id:long}")]
    [RequirePermission("USUARIOS.DELETE")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteUsuarioCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reemplaza los ítems de menú asignados a un usuario.
    /// </summary>
    [HttpPut("{id:long}/menu")]
    [RequirePermission("USUARIOS.MENU")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetMenu(
        long id,
        [FromBody] SetMenuRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SetMenuUsuarioCommand(id, request.MenuIds), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Establece o actualiza el valor de un permiso para el usuario.
    /// </summary>
    [HttpPut("{id:long}/permisos/{seguridadId:long}")]
    [RequirePermission("USUARIOS.PERMISOS")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPermiso(
        long id,
        long seguridadId,
        [FromBody] SetPermisoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SetPermisoUsuarioCommand(id, seguridadId, request.Valor), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea o actualiza un parámetro personalizado del usuario (upsert).
    /// </summary>
    [HttpPut("{id:long}/parametros")]
    [RequirePermission("USUARIOS.PARAMETROS")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetParametro(
        long id,
        [FromBody] SetParametroRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SetParametroUsuarioCommand(id, request.Clave, request.Valor), ct);
        return FromResult(result);
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record SetMenuRequest(IReadOnlyList<long> MenuIds);
public record SetPermisoRequest(bool Valor);
public record SetParametroRequest(string Clave, string? Valor);