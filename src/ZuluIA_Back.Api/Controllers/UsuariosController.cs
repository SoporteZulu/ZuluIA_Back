using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Application.Features.Usuarios.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class UsuariosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteUsuarioCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reactiva un usuario desactivado.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateUsuarioCommand(id), ct);
        return FromResult(result);
    }

    // ─── Usuarios relacionados (grupos / miembros) ────────────────────────────

    /// <summary>Retorna los usuarios vinculados a un usuario (como miembro o como grupo). VB6: clsUsuarioXUsuario / SEG_USUARIOXUSUARIO.</summary>
    [HttpGet("{id:long}/usuarios-relacionados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsuariosRelacionados(long id, CancellationToken ct)
    {
        var lista = await db.UsuariosXUsuario
            .AsNoTracking()
            .Where(u => u.UsuarioMiembroId == id || u.UsuarioGrupoId == id)
            .Select(u => new { u.Id, u.UsuarioMiembroId, u.UsuarioGrupoId })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Agrega un usuario relacionado.</summary>
    [HttpPost("{id:long}/usuarios-relacionados")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddUsuarioRelacionado(long id, [FromBody] UsuarioRelacionadoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddUsuarioRelacionadoCommand(id, req.UsuarioMiembroId, req.UsuarioGrupoId), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetUsuariosRelacionados), new { id }, new { Id = result.Value });
    }

    /// <summary>Elimina un usuario relacionado.</summary>
    [HttpDelete("{id:long}/usuarios-relacionados/{uxuId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUsuarioRelacionado(long id, long uxuId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteUsuarioRelacionadoCommand(id, uxuId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = "Relacion no encontrada." });

        return Ok();
    }

    /// <summary>
    /// Reemplaza los ítems de menú asignados a un usuario.
    /// </summary>
    [HttpPut("{id:long}/menu")]
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
    /// Agrega un ítem de menú individual al usuario (favorito / acceso directo).
    /// VB6: clsMenuFavorito / clsMenuGestion — MNU_ITEMXMENU.Guardar().
    /// </summary>
    [HttpPost("{id:long}/menu/{menuItemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMenuItem(long id, long menuItemId, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddUsuarioMenuItemCommand(id, menuItemId), ct);
        if (!result.IsSuccess)
            return Conflict(new { error = "El item ya esta asignado al usuario." });

        return Ok(new { Id = result.Value });
    }

    /// <summary>
    /// Elimina un ítem de menú individual del usuario.
    /// VB6: clsMenuFavorito / clsMenuGestion — MNU_ITEMXMENU.Eliminar().
    /// </summary>
    [HttpDelete("{id:long}/menu/{menuItemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMenuItem(long id, long menuItemId, CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveUsuarioMenuItemCommand(id, menuItemId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = "El item no esta asignado al usuario." });

        return Ok();
    }

    /// <summary>
    /// Establece o actualiza el valor de un permiso para el usuario.
    /// </summary>
    [HttpPut("{id:long}/permisos/{seguridadId:long}")]
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
public record UsuarioRelacionadoRequest(long? UsuarioMiembroId = null, long? UsuarioGrupoId = null);