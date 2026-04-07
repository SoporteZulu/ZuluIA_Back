﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Proyectos.Commands;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/obras")]
public class ObrasController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId,
        [FromQuery] long? terceroId,
        [FromQuery] string? estado,
        CancellationToken ct = default)
    {
        var q = db.Proyectos.AsNoTracking();
        if (sucursalId.HasValue) q = q.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue) q = q.Where(x => x.TerceroId == terceroId.Value);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(x => x.Estado == estado.ToLowerInvariant());

        var result = await q
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.Estado,
                x.FechaInicio,
                x.FechaFin,
                x.SucursalId,
                x.TerceroId,
                x.TotalCuotas,
                x.Anulada
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Proyectos.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.Observacion,
                x.Estado,
                x.FechaInicio,
                x.FechaFin,
                x.SucursalId,
                x.TerceroId,
                x.TotalCuotas,
                x.SoloPadre,
                x.Anulada
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateObraRequest req, CancellationToken ct)
    {
        var command = new CreateProyectoCommand(
            req.Codigo,
            req.Descripcion,
            req.SucursalId,
            req.TerceroId,
            req.FechaInicio,
            req.FechaFin,
            req.TotalCuotas,
            req.SoloPadre,
            req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateObraRequest req, CancellationToken ct)
    {
        var command = new UpdateProyectoCommand(
            id,
            req.Descripcion,
            req.FechaInicio,
            req.FechaFin,
            req.TotalCuotas,
            req.SoloPadre,
            req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = $"Obra {id} no encontrada." })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("{id:long}/finalizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalizar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new FinalizarProyectoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = $"Obra {id} no encontrada." });

        return Ok();
    }

    [HttpPatch("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularProyectoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = $"Obra {id} no encontrada." });

        return Ok();
    }

    [HttpPatch("{id:long}/reactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ReactivarProyectoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = $"Obra {id} no encontrada." });

        return Ok();
    }
}

public record CreateObraRequest(
    string Codigo,
    string Descripcion,
    long SucursalId,
    long? TerceroId,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    int TotalCuotas,
    bool SoloPadre,
    string? Observacion);

public record UpdateObraRequest(
    string Descripcion,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    int TotalCuotas,
    bool SoloPadre,
    string? Observacion);
