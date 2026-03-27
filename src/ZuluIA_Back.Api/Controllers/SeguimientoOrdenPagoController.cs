using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Seguimiento de estado de ordenes de pago (legacy VB6: SEGUIMIENTOORDENPAGO).
/// </summary>
public class SeguimientoOrdenPagoController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? pagoId,
        [FromQuery] long? sucursalId,
        CancellationToken ct)
    {
        var query = db.SeguimientosOrdenPago.AsNoTracking();

        if (pagoId.HasValue)
            query = query.Where(x => x.PagoId == pagoId.Value);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var result = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.PagoId,
                x.SucursalId,
                x.Fecha,
                x.Estado,
                x.Observacion,
                x.UsuarioId
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.SeguimientosOrdenPago.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.PagoId,
                x.SucursalId,
                x.Fecha,
                x.Estado,
                x.Observacion,
                x.UsuarioId
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SeguimientoOrdenPagoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateSeguimientoOrdenPagoCommand(
                req.PagoId,
                req.SucursalId,
                req.Fecha,
                req.Estado,
                req.Observacion,
                req.UsuarioId),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPatch("{id:long}/observacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateObservacion(long id, [FromBody] UpdateSeguimientoObservacionRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateSeguimientoOrdenPagoObservacionCommand(id, req.Observacion, req.UsuarioId),
            ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { result.Value.Id, result.Value.Observacion });
    }
}

public record SeguimientoOrdenPagoRequest(
    long PagoId,
    long SucursalId,
    DateOnly Fecha,
    string Estado,
    string? Observacion,
    long? UsuarioId);

public record UpdateSeguimientoObservacionRequest(string? Observacion, long? UsuarioId);
