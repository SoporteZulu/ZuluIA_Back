using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Franquicias.Commands;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/franquicias-regiones")]
public class FranquiciasRegionesController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? regionId = null,
        [FromQuery] long? grupoEconomicoId = null,
        CancellationToken ct = default)
    {
        var query =
            from fr in db.FranquiciasXRegiones.AsNoTracking()
            join s in db.Sucursales.AsNoTracking() on fr.SucursalId equals s.Id
            join r in db.Regiones.AsNoTracking() on fr.RegionId equals r.Id
            join g in db.GrupoEconomicos.AsNoTracking() on fr.GrupoEconomicoId equals g.Id into gj
            from g in gj.DefaultIfEmpty()
            select new
            {
                fr.Id,
                fr.SucursalId,
                SucursalDescripcion = s.RazonSocial,
                fr.RegionId,
                RegionDescripcion = r.Descripcion,
                fr.GrupoEconomicoId,
                GrupoEconomicoDescripcion = g != null ? g.Descripcion : null
            };

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (regionId.HasValue)
            query = query.Where(x => x.RegionId == regionId.Value);
        if (grupoEconomicoId.HasValue)
            query = query.Where(x => x.GrupoEconomicoId == grupoEconomicoId.Value);

        var items = await query
            .OrderBy(x => x.SucursalDescripcion)
            .ThenBy(x => x.RegionDescripcion)
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("{id:long}", Name = "GetFranquiciaRegionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var entity = await db.FranquiciasXRegiones.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.RegionId,
                x.GrupoEconomicoId
            })
            .FirstOrDefaultAsync(ct);

        return entity is null
            ? NotFound(new { error = $"Asignacion franquicia-region {id} no encontrada." })
            : Ok(entity);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] FranquiciaRegionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateFranquiciaXRegionCommand(request.SucursalId, request.RegionId, request.GrupoEconomicoId),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetFranquiciaRegionById", new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] FranquiciaRegionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateFranquiciaXRegionCommand(id, request.SucursalId, request.RegionId, request.GrupoEconomicoId),
            ct);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { Id = id });
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteFranquiciaXRegionCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record FranquiciaRegionRequest(long SucursalId, long RegionId, long? GrupoEconomicoId = null);