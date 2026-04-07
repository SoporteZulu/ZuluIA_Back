﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Proyectos.Commands;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/tareas")]
public class TareasController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet("estimadas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstimadas(
        [FromQuery] long? proyectoId,
        [FromQuery] long? sucursalId,
        [FromQuery] long? asignadoA,
        [FromQuery] bool? activa,
        CancellationToken ct = default)
    {
        var query = db.TareasEstimadas.AsNoTracking();

        if (proyectoId.HasValue) query = query.Where(x => x.ProyectoId == proyectoId.Value);
        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (asignadoA.HasValue) query = query.Where(x => x.AsignadoA == asignadoA.Value);
        if (activa.HasValue) query = query.Where(x => x.Activa == activa.Value);

        var result = await query
            .OrderByDescending(x => x.FechaDesde)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.ProyectoId,
                x.SucursalId,
                x.AsignadoA,
                x.Descripcion,
                x.FechaDesde,
                x.FechaHasta,
                x.HorasEstimadas,
                x.Observacion,
                x.Activa
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("estimadas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEstimada([FromBody] CreateTareaEstimadaRequest req, CancellationToken ct)
    {
        var command = new CreateTareaEstimadaCommand(
            req.ProyectoId,
            req.SucursalId,
            req.AsignadoA,
            req.Descripcion,
            req.FechaDesde,
            req.FechaHasta,
            req.HorasEstimadas,
            req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetEstimadas), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("estimadas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEstimada(long id, [FromBody] UpdateTareaEstimadaRequest req, CancellationToken ct)
    {
        var command = new UpdateTareaEstimadaCommand(
            id,
            req.AsignadoA,
            req.Descripcion,
            req.FechaDesde,
            req.FechaHasta,
            req.HorasEstimadas,
            req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("estimadas/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarEstimada(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateTareaEstimadaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("estimadas/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarEstimada(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateTareaEstimadaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("reales")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReales(
        [FromQuery] long? proyectoId,
        [FromQuery] long? sucursalId,
        [FromQuery] long? usuarioId,
        [FromQuery] bool? aprobada,
        CancellationToken ct = default)
    {
        var query = db.TareasReales.AsNoTracking();

        if (proyectoId.HasValue) query = query.Where(x => x.ProyectoId == proyectoId.Value);
        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (usuarioId.HasValue) query = query.Where(x => x.UsuarioId == usuarioId.Value);
        if (aprobada.HasValue) query = query.Where(x => x.Aprobada == aprobada.Value);

        var result = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.ProyectoId,
                x.SucursalId,
                x.TareaEstimadaId,
                x.UsuarioId,
                x.Fecha,
                x.Descripcion,
                x.HorasReales,
                x.Aprobada,
                x.Observacion
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("reales")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReal([FromBody] CreateTareaRealRequest req, CancellationToken ct)
    {
        var command = new CreateTareaRealCommand(
            req.ProyectoId,
            req.SucursalId,
            req.TareaEstimadaId,
            req.UsuarioId,
            req.Fecha,
            req.Descripcion,
            req.HorasReales,
            req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetReales), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("reales/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateReal(long id, [FromBody] UpdateTareaRealRequest req, CancellationToken ct)
    {
        var command = new UpdateTareaRealCommand(id, req.Descripcion, req.HorasReales, req.Observacion);
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("reales/{id:long}/aprobar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AprobarReal(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ApproveTareaRealCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpDelete("reales/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReal(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTareaRealCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record CreateTareaEstimadaRequest(
    long ProyectoId,
    long SucursalId,
    long? AsignadoA,
    string Descripcion,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    decimal HorasEstimadas,
    string? Observacion);

public record UpdateTareaEstimadaRequest(
    long? AsignadoA,
    string Descripcion,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    decimal HorasEstimadas,
    string? Observacion);

public record CreateTareaRealRequest(
    long ProyectoId,
    long SucursalId,
    long? TareaEstimadaId,
    long UsuarioId,
    DateOnly Fecha,
    string Descripcion,
    decimal HorasReales,
    string? Observacion);

public record UpdateTareaRealRequest(
    string Descripcion,
    decimal HorasReales,
    string? Observacion);
