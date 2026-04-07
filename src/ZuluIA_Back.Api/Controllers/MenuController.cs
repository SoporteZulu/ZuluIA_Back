using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class MenuController(
    IMediator mediator,
    IMenuRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna un ítem de menú por ID.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Menu
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new MenuItemDto
            {
                Id          = x.Id,
                ParentId    = x.ParentId,
                Descripcion = x.Descripcion,
                Formulario  = x.Formulario,
                Icono       = x.Icono,
                Nivel       = x.Nivel,
                Orden       = x.Orden,
                Activo      = x.Activo
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    /// <summary>
    /// Retorna el árbol de menú completo (todos los ítems activos).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArbol(CancellationToken ct)
    {
        var items = await repo.GetArbolCompletoAsync(ct);

        var dtos = items.Select(x => new MenuItemDto
        {
            Id          = x.Id,
            ParentId    = x.ParentId,
            Descripcion = x.Descripcion,
            Formulario  = x.Formulario,
            Icono       = x.Icono,
            Nivel       = x.Nivel,
            Orden       = x.Orden,
            Activo      = x.Activo,
            Hijos       = new List<MenuItemDto>()
        }).ToList();

        // Construir árbol
        var lookup = dtos.ToDictionary(x => x.Id);
        var roots = new List<MenuItemDto>();

        foreach (var dto in dtos)
        {
            if (dto.ParentId.HasValue && lookup.TryGetValue(dto.ParentId.Value, out var parent))
                ((List<MenuItemDto>)parent.Hijos).Add(dto);
            else
                roots.Add(dto);
        }

        return Ok(roots.OrderBy(x => x.Orden));
    }

    /// <summary>
    /// Crea un nuevo ítem de menú.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMenuItemRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateMenuItemCommand(
                request.ParentId,
                request.Descripcion,
                request.Formulario,
                request.Icono,
                request.Nivel,
                request.Orden),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value, mensaje = "Ítem de menú creado correctamente." });
    }

    /// <summary>
    /// Actualiza un ítem de menú existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateMenuItemRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateMenuItemCommand(
                id,
                request.Descripcion,
                request.Formulario,
                request.Icono,
                request.Orden),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Ítem de menú actualizado correctamente." });
    }

    /// <summary>
    /// Desactiva un ítem de menú.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteMenuItemCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Ítem de menú desactivado correctamente." });
    }

    /// <summary>
    /// Reactiva un ítem de menú.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateMenuItemCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Ítem de menú activado correctamente." });
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record CreateMenuItemRequest(
    long? ParentId,
    string Descripcion,
    string? Formulario,
    string? Icono,
    short Nivel,
    short Orden);

public record UpdateMenuItemRequest(
    string Descripcion,
    string? Formulario,
    string? Icono,
    short Orden);