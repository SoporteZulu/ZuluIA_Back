using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Referencia.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>Gestión de zonas geográficas/comerciales (VB6: ZONAS).</summary>
public class ZonasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/zonas
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? soloActivas = true,
        CancellationToken ct = default)
    {
        var query = db.Zonas.AsNoTracking();
        if (soloActivas.HasValue)
            query = query.Where(x => x.Activo == soloActivas.Value);

        var result = await query
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET api/zonas/{id}
    [HttpGet("{id:long}", Name = "GetZonaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var zona = await db.Zonas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return zona is null
            ? NotFound(new { error = $"Zona {id} no encontrada." })
            : Ok(new { zona.Id, zona.Descripcion, zona.Activo });
    }

    // POST api/zonas
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ZonaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateZonaCommand(req.Descripcion), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var zona = await db.Zonas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == result.Value, ct);
        return CreatedAtRoute(
            "GetZonaById",
            new { id = result.Value },
            zona is null
                ? new { Id = result.Value }
                : new { zona.Id, zona.Descripcion, zona.Activo });
    }

    // PUT api/zonas/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] ZonaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateZonaCommand(id, req.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var zona = await db.Zonas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return Ok(zona is null ? new { Id = id } : new { zona.Id, zona.Descripcion, zona.Activo });
    }

    // PATCH api/zonas/{id}/activar
    [HttpPatch("{id:long}/activar")]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateZonaCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }

    // PATCH api/zonas/{id}/desactivar
    [HttpPatch("{id:long}/desactivar")]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateZonaCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }
}

public record ZonaRequest(string Descripcion);
