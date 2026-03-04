using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class CategoriasItemsController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna el árbol completo de categorías activas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArbol(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCategoriasItemsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna las categorías de un nivel específico (plana, sin árbol).
    /// </summary>
    [HttpGet("nivel/{nivel:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByNivel(short nivel, CancellationToken ct)
    {
        var items = await db.CategoriasItems
            .AsNoTracking()
            .Where(x => x.Nivel == nivel && x.Activo)
            .OrderBy(x => x.OrdenNivel)
            .ThenBy(x => x.Descripcion)
            .Select(x => new CategoriaItemDto
            {
                Id          = x.Id,
                ParentId    = x.ParentId,
                Codigo      = x.Codigo,
                Descripcion = x.Descripcion,
                Nivel       = x.Nivel,
                OrdenNivel  = x.OrdenNivel,
                Activo      = x.Activo
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    /// <summary>
    /// Crea una nueva categoría de ítem.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoriaItemCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateCategoriaItemRequest request,
        CancellationToken ct)
    {
        var categoria = await db.CategoriasItems
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (categoria is null)
            return NotFound(new { error = $"No se encontró la categoría con ID {id}." });

        categoria.Actualizar(
            request.Codigo,
            request.Descripcion,
            request.OrdenNivel,
            null);

        await db.SaveChangesAsync(ct);
        return Ok(new { mensaje = "Categoría actualizada correctamente." });
    }

    /// <summary>
    /// Desactiva una categoría.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var categoria = await db.CategoriasItems
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (categoria is null)
            return NotFound(new { error = $"No se encontró la categoría con ID {id}." });

        // Verificar que no tenga ítems asociados activos
        var tieneItems = await db.Items
            .AnyAsync(x => x.CategoriaId == id && x.Activo, ct);

        if (tieneItems)
            return Conflict(new
            {
                error = "No se puede desactivar una categoría que tiene ítems activos asociados."
            });

        categoria.Desactivar(null);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Categoría desactivada correctamente." });
    }
}

public record UpdateCategoriaItemRequest(
    string Codigo,
    string Descripcion,
    string? OrdenNivel);