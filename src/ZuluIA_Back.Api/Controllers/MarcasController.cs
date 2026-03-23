using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Referencia.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>Gestión de marcas/fabricantes de ítems (VB6: MARCAS).</summary>
public class MarcasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/marcas
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? soloActivas = true,
        CancellationToken ct = default)
    {
        var query = db.Marcas.AsNoTracking();
        if (soloActivas.HasValue)
            query = query.Where(x => x.Activo == soloActivas.Value);

        var result = await query
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET api/marcas/{id}
    [HttpGet("{id:long}", Name = "GetMarcaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var marca = await db.Marcas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return marca is null
            ? NotFound(new { error = $"Marca {id} no encontrada." })
            : Ok(new { marca.Id, marca.Descripcion, marca.Activo });
    }

    // POST api/marcas
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] MarcaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateMarcaCommand(req.Descripcion), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var marca = await db.Marcas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == result.Value, ct);
        return CreatedAtRoute(
            "GetMarcaById",
            new { id = result.Value },
            marca is null
                ? new { Id = result.Value }
                : new { marca.Id, marca.Descripcion, marca.Activo });
    }

    // PUT api/marcas/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] MarcaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateMarcaCommand(id, req.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var marca = await db.Marcas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return Ok(marca is null ? new { Id = id } : new { marca.Id, marca.Descripcion, marca.Activo });
    }

    // PATCH api/marcas/{id}/activar
    [HttpPatch("{id:long}/activar")]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateMarcaCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }

    // PATCH api/marcas/{id}/desactivar
    [HttpPatch("{id:long}/desactivar")]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateMarcaCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }
}

public record MarcaRequest(string Descripcion);
