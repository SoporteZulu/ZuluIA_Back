using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Catálogo de estados generales legacy para terceros.
/// </summary>
public class EstadosPersonasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? activo, CancellationToken ct)
    {
        var query = db.EstadosPersonas.AsNoTracking();
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        var result = await query
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.EstadosPersonas.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Descripcion, x.Activo })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] EstadoPersonaCatalogoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateEstadoPersonaCommand(req.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] EstadoPersonaCatalogoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateEstadoPersonaCommand(id, req.Descripcion), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { Id = id });
    }

    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateEstadoPersonaCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateEstadoPersonaCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record EstadoPersonaCatalogoRequest(string Descripcion);
