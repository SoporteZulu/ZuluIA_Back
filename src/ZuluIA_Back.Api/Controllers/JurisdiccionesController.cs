using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Referencia.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de jurisdicciones fiscales para Ingresos Brutos (IIBB) provinciales.
/// VB6: JURISDICCIONES
/// </summary>
public class JurisdiccionesController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/jurisdicciones
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? soloActivas = true,
        CancellationToken ct = default)
    {
        var query = db.Jurisdicciones.AsNoTracking();
        if (soloActivas.HasValue)
            query = query.Where(x => x.Activo == soloActivas.Value);

        var result = await query
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET api/jurisdicciones/{id}
    [HttpGet("{id:long}", Name = "GetJurisdiccionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var jur = await db.Jurisdicciones.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return jur is null
            ? NotFound(new { error = $"Jurisdicción {id} no encontrada." })
            : Ok(new { jur.Id, jur.Descripcion, jur.Activo });
    }

    // POST api/jurisdicciones
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] JurisdiccionRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateJurisdiccionCommand(req.Descripcion), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var jur = await db.Jurisdicciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == result.Value, ct);
        return CreatedAtRoute(
            "GetJurisdiccionById",
            new { id = result.Value },
            jur is null
                ? new { Id = result.Value }
                : new { jur.Id, jur.Descripcion, jur.Activo });
    }

    // PUT api/jurisdicciones/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] JurisdiccionRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateJurisdiccionCommand(id, req.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = $"Jurisdicción {id} no encontrada." })
                : BadRequest(new { error = result.Error });

        var jur = await db.Jurisdicciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return Ok(jur is null ? new { Id = id } : new { jur.Id, jur.Descripcion, jur.Activo });
    }

    // PATCH api/jurisdicciones/{id}/activar
    [HttpPatch("{id:long}/activar")]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateJurisdiccionCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }

    // PATCH api/jurisdicciones/{id}/desactivar
    [HttpPatch("{id:long}/desactivar")]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateJurisdiccionCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }
}

public record JurisdiccionRequest(string Descripcion);
