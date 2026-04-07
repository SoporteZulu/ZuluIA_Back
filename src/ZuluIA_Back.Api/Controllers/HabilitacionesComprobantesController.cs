using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/habilitaciones-comprobantes")]
public class HabilitacionesComprobantesController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId,
        [FromQuery] long? comprobanteId,
        [FromQuery] string? estado,
        [FromQuery] string? tipoDocumento,
        CancellationToken ct = default)
    {
        var query = db.HabilitacionesComprobantes.AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (comprobanteId.HasValue)
            query = query.Where(x => x.ComprobanteId == comprobanteId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(x => x.Estado.ToString() == estado);

        if (!string.IsNullOrWhiteSpace(tipoDocumento))
            query = query.Where(x => x.TipoDocumento == tipoDocumento.Trim().ToUpperInvariant());

        var result = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ComprobanteId,
                x.SucursalId,
                x.TipoDocumento,
                Estado = x.Estado.ToString(),
                x.MotivoBloqueo,
                x.ObservacionHabilitacion,
                x.HabilitadoPor,
                x.FechaHabilitacion,
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
        var item = await db.HabilitacionesComprobantes.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ComprobanteId,
                x.SucursalId,
                x.TipoDocumento,
                Estado = x.Estado.ToString(),
                x.MotivoBloqueo,
                x.ObservacionHabilitacion,
                x.HabilitadoPor,
                x.FechaHabilitacion,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CrearHabilitacionComprobanteRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateHabilitacionComprobanteCommand(
                req.ComprobanteId,
                req.SucursalId,
                req.TipoDocumento,
                req.MotivoBloqueo),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPatch("{id:long}/habilitar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Habilitar(long id, [FromBody] ResolverHabilitacionRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new HabilitarComprobanteCommand(id, req.ResponsableId, req.Observacion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/rechazar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rechazar(long id, [FromBody] ResolverHabilitacionRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new RechazarHabilitacionComprobanteCommand(id, req.ResponsableId, req.Observacion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record CrearHabilitacionComprobanteRequest(
    long ComprobanteId,
    long SucursalId,
    string TipoDocumento,
    string? MotivoBloqueo);

public record ResolverHabilitacionRequest(long ResponsableId, string? Observacion);