using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de Atributos de Ítems (características personalizables).
/// Migrado desde VB6: frmAtributos / ATRIBUTOS + ATRIBUTOSITEMS.
/// </summary>
public class AtributosController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // ── Atributos (definiciones) ──────────────────────────────────────────────

    // GET api/atributos
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloActivos = true,
        CancellationToken ct = default)
    {
        var q = db.Atributos.AsNoTracking();
        if (soloActivos) q = q.Where(x => x.Activo);
        var result = await q
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.Tipo, x.Requerido, x.Activo })
            .ToListAsync(ct);
        return Ok(result);
    }

    // GET api/atributos/{id}
    [HttpGet("{id:long}", Name = "GetAtributoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var a = await db.Atributos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return NotFound(new { error = $"Atributo {id} no encontrado." });
        return Ok(new { a.Id, a.Descripcion, a.Tipo, a.Requerido, a.Activo });
    }

    // POST api/atributos
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AtributoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateAtributoCommand(req.Descripcion, req.Tipo, req.Requerido), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetAtributoById", new { id = result.Value }, new { Id = result.Value });
    }

    // PUT api/atributos/{id}
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] AtributoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateAtributoCommand(id, req.Descripcion, req.Tipo, req.Requerido), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound()
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    // PATCH api/atributos/{id}/activar
    [HttpPatch("{id:long}/activar")]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateAtributoCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound()
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id, activo = true });
    }

    // PATCH api/atributos/{id}/desactivar
    [HttpPatch("{id:long}/desactivar")]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateAtributoCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound()
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id, activo = false });
    }

    // ── Valores de atributos por ítem ─────────────────────────────────────────

    // GET api/atributos/item/{itemId}
    [HttpGet("item/{itemId:long}")]
    public async Task<IActionResult> GetByItem(long itemId, CancellationToken ct)
    {
        var result = await db.AtributosItems
            .AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .Join(db.Atributos, ai => ai.AtributoId, a => a.Id,
                (ai, a) => new { ai.Id, ai.ItemId, ai.AtributoId, a.Descripcion, a.Tipo, ai.Valor })
            .ToListAsync(ct);
        return Ok(result);
    }

    // POST api/atributos/item/{itemId}
    [HttpPost("item/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetAtributoItem(long itemId, [FromBody] AtributoItemRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new SetAtributoItemCommand(itemId, req.AtributoId, req.Valor), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        if (result.Value.Actualizado)
            return Ok(new { result.Value.Id, mensaje = "Valor actualizado." });

        return CreatedAtAction(nameof(GetByItem), new { itemId }, new { result.Value.Id });
    }

    // DELETE api/atributos/item/{itemId}/{atributoId}
    [HttpDelete("item/{itemId:long}/{atributoId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAtributoItem(long itemId, long atributoId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteAtributoItemCommand(itemId, atributoId), ct);
        if (result.IsFailure)
            return NotFound();

        return NoContent();
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record AtributoRequest(string Descripcion, string Tipo = "texto", bool Requerido = false);
public record AtributoItemRequest(long AtributoId, string Valor);
