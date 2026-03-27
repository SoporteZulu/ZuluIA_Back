using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Queries;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/categorias-items")]
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
        var result = await Mediator.Send(
            new UpdateCategoriaItemCommand(id, request.Codigo, request.Descripcion, request.OrdenNivel),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

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
        var result = await Mediator.Send(new DeleteCategoriaItemCommand(id), ct);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });

            if (result.Error?.Contains("ítems activos", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { mensaje = "Categoría desactivada correctamente." });
    }

    /// <summary>
    /// Reactiva una categoría desactivada.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateCategoriaItemCommand(id), ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Categoría activada correctamente." });
    }
}

public record UpdateCategoriaItemRequest(
    string Codigo,
    string Descripcion,
    string? OrdenNivel);