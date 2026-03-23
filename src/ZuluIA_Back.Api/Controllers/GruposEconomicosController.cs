using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Franquicias.Commands;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/grupos-economicos")]
public class GruposEconomicosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivos = true, CancellationToken ct = default)
    {
        var query = db.GrupoEconomicos.AsNoTracking();
        if (soloActivos.HasValue)
            query = query.Where(x => x.Activo == soloActivos.Value);

        var items = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("{id:long}", Name = "GetGrupoEconomicoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var entity = await db.GrupoEconomicos.FindAsync([id], ct);
        return entity is null
            ? NotFound(new { error = $"Grupo economico {id} no encontrado." })
            : Ok(entity);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateGrupoEconomicoRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateGrupoEconomicoCommand(request.Codigo, request.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetGrupoEconomicoById", new { id = result.Value }, new { Id = result.Value, request.Codigo });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateGrupoEconomicoRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateGrupoEconomicoCommand(id, request.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteGrupoEconomicoCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateGrupoEconomicoCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record CreateGrupoEconomicoRequest(string Codigo, string Descripcion);
public record UpdateGrupoEconomicoRequest(string Descripcion);