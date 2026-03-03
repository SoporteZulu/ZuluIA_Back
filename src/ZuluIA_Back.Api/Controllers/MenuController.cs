using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
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
            Activo      = x.Activo
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
        var item = MenuItem.Crear(
            request.ParentId,
            request.Descripcion,
            request.Formulario,
            request.Icono,
            request.Nivel,
            request.Orden);

        await repo.AddAsync(item, ct);
        await db.SaveChangesAsync(ct);

        return Ok(new { id = item.Id, mensaje = "Ítem de menú creado correctamente." });
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
        var item = await db.Menu
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (item is null)
            return NotFound(new { error = $"No se encontró el ítem de menú con ID {id}." });

        item.Actualizar(
            request.Descripcion,
            request.Formulario,
            request.Icono,
            request.Orden);

        await db.SaveChangesAsync(ct);
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
        var item = await db.Menu
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (item is null)
            return NotFound(new { error = $"No se encontró el ítem de menú con ID {id}." });

        item.Desactivar();
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Ítem de menú desactivado correctamente." });
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