using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.Queries;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Timbrados habilitados por el SET (Paraguay) para emitir comprobantes.
/// Migrado desde VB6: frmTimbrado / TIMBRADO.
/// </summary>
public class TimbradoController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/timbrado?sucursalId=1&puntoFacturacionId=2
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId,
        [FromQuery] long? puntoFacturacionId,
        CancellationToken ct)
    {
        var q = db.Timbrados.AsNoTracking();
        if (sucursalId.HasValue)          q = q.Where(x => x.SucursalId == sucursalId.Value);
        if (puntoFacturacionId.HasValue)  q = q.Where(x => x.PuntoFacturacionId == puntoFacturacionId.Value);

        var result = await q
            .OrderBy(x => x.SucursalId)
            .ThenBy(x => x.PuntoFacturacionId)
            .ThenBy(x => x.FechaInicio)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.PuntoFacturacionId,
                x.TipoComprobanteId,
                x.NroTimbrado,
                x.FechaInicio,
                x.FechaFin,
                x.NroComprobanteDesde,
                x.NroComprobanteHasta,
                x.Activo
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET api/timbrado/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Timbrados.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.PuntoFacturacionId,
                x.TipoComprobanteId,
                x.NroTimbrado,
                x.FechaInicio,
                x.FechaFin,
                x.NroComprobanteDesde,
                x.NroComprobanteHasta,
                x.Activo
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    // GET api/timbrado/vigente?fecha=2024-01-15&tipoComprobanteId=1&puntoFacturacionId=2
    [HttpGet("vigente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVigente(
        [FromQuery] DateOnly fecha,
        [FromQuery] long tipoComprobanteId,
        [FromQuery] long puntoFacturacionId,
        CancellationToken ct)
    {
        var item = await db.Timbrados.AsNoTracking()
            .Where(x => x.Activo
                     && x.TipoComprobanteId  == tipoComprobanteId
                     && x.PuntoFacturacionId == puntoFacturacionId
                     && x.FechaInicio <= fecha
                     && x.FechaFin    >= fecha)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.PuntoFacturacionId,
                x.TipoComprobanteId,
                x.NroTimbrado,
                x.FechaInicio,
                x.FechaFin,
                x.NroComprobanteDesde,
                x.NroComprobanteHasta,
                x.Activo
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    // GET api/timbrado/validar-emision?sucursalId=1&puntoFacturacionId=2&tipoComprobanteId=3&fecha=2026-03-20&numeroComprobante=15
    [HttpGet("validar-emision")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidarEmision(
        [FromQuery] long sucursalId,
        [FromQuery] long? puntoFacturacionId,
        [FromQuery] long tipoComprobanteId,
        [FromQuery] DateOnly fecha,
        [FromQuery] long numeroComprobante,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ValidarTimbradoParaguayQuery(
                sucursalId,
                puntoFacturacionId,
                tipoComprobanteId,
                fecha,
                numeroComprobante),
            ct);

        return Ok(result);
    }

    // POST api/timbrado
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTimbradoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateTimbradoCommand(
                req.SucursalId,
                req.PuntoFacturacionId,
                req.TipoComprobanteId,
                req.NroTimbrado,
                req.FechaInicio,
                req.FechaFin,
                req.NroComprobanteDesde,
                req.NroComprobanteHasta),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    // PUT api/timbrado/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTimbradoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateTimbradoCommand(id, req.FechaInicio, req.FechaFin, req.NroComprobanteDesde, req.NroComprobanteHasta),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound()
                : BadRequest(new { error = result.Error });

        return Ok(new { id });
    }

    // PATCH api/timbrado/{id}/activar
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateTimbradoCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }

    // PATCH api/timbrado/{id}/desactivar
    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateTimbradoCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }
}

public record CreateTimbradoRequest(
    long    SucursalId,
    long    PuntoFacturacionId,
    long    TipoComprobanteId,
    string  NroTimbrado,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int     NroComprobanteDesde,
    int     NroComprobanteHasta);

public record UpdateTimbradoRequest(
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int      NroComprobanteDesde,
    int      NroComprobanteHasta);
