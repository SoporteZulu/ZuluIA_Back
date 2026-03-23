using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Planes de tarjeta (legacy VB6: PLANTARJETAS).
/// </summary>
public class PlanesTarjetaController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? tarjetaTipoId,
        [FromQuery] bool? activo,
        CancellationToken ct)
    {
        var query = db.PlanesTarjeta.AsNoTracking();

        if (tarjetaTipoId.HasValue)
            query = query.Where(x => x.TarjetaTipoId == tarjetaTipoId.Value);

        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        var result = await query
            .OrderBy(x => x.TarjetaTipoId)
            .ThenBy(x => x.Codigo)
            .Select(x => new
            {
                x.Id,
                x.TarjetaTipoId,
                x.Codigo,
                x.Descripcion,
                x.CantidadCuotas,
                x.Recargo,
                x.DiasAcreditacion,
                x.Activo
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.PlanesTarjeta.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.TarjetaTipoId,
                x.Codigo,
                x.Descripcion,
                x.CantidadCuotas,
                x.Recargo,
                x.DiasAcreditacion,
                x.Activo
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] PlanTarjetaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreatePlanTarjetaCommand(
                req.TarjetaTipoId,
                req.Codigo,
                req.Descripcion,
                req.CantidadCuotas,
                req.Recargo,
                req.DiasAcreditacion),
            ct);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("no existe la tarjeta", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] PlanTarjetaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdatePlanTarjetaCommand(id, req.Descripcion, req.CantidadCuotas, req.Recargo, req.DiasAcreditacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivatePlanTarjetaCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivatePlanTarjetaCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record PlanTarjetaRequest(
    long TarjetaTipoId,
    string Codigo,
    string Descripcion,
    int CantidadCuotas,
    decimal Recargo,
    int DiasAcreditacion);
