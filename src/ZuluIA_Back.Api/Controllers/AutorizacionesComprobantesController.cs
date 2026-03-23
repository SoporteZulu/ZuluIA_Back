using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/autorizaciones-comprobantes")]
public class AutorizacionesComprobantesController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId,
        [FromQuery] long? comprobanteId,
        [FromQuery] string? estado,
        [FromQuery] string? tipoOperacion,
        CancellationToken ct = default)
    {
        var query = db.AutorizacionesComprobantes.AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (comprobanteId.HasValue)
            query = query.Where(x => x.ComprobanteId == comprobanteId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(x => x.Estado.ToString() == estado);

        if (!string.IsNullOrWhiteSpace(tipoOperacion))
            query = query.Where(x => x.TipoOperacion == tipoOperacion.Trim().ToUpperInvariant());

        var result = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ComprobanteId,
                x.SucursalId,
                x.TipoOperacion,
                Estado = x.Estado.ToString(),
                x.Motivo,
                x.AutorizadoPor,
                x.FechaResolucion,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.AutorizacionesComprobantes.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ComprobanteId,
                x.SucursalId,
                x.TipoOperacion,
                Estado = x.Estado.ToString(),
                x.Motivo,
                x.AutorizadoPor,
                x.FechaResolucion,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CrearAutorizacionComprobanteRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateAutorizacionComprobanteCommand(req.ComprobanteId, req.SucursalId, req.TipoOperacion),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPatch("{id:long}/autorizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Autorizar(long id, [FromBody] ResolverAutorizacionRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AutorizarComprobanteCommand(id, req.ResponsableId, req.Motivo), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/rechazar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rechazar(long id, [FromBody] ResolverAutorizacionRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new RechazarAutorizacionComprobanteCommand(id, req.ResponsableId, req.Motivo), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record CrearAutorizacionComprobanteRequest(
    long ComprobanteId,
    long SucursalId,
    string TipoOperacion);

public record ResolverAutorizacionRequest(long ResponsableId, string? Motivo);