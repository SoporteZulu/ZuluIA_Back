using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Proyectos.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Proyectos de trabajo / contratos con asignación de comprobantes por cuota.
/// Migrado desde VB6: clsProyectos + ClsComprobantesProyectos / Proyectos + ComprobantesProyectos.
/// </summary>
[Route("api/proyectos")]
public class ProyectosController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/proyectos?sucursalId=1&estado=activo
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId,
        [FromQuery] long? terceroId,
        [FromQuery] string? estado,
        CancellationToken ct)
    {
        var q = db.Proyectos.AsNoTracking();
        if (sucursalId.HasValue)                q = q.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue)                 q = q.Where(x => x.TerceroId  == terceroId.Value);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(x => x.Estado     == estado.ToLowerInvariant());

        var result = await q
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id, x.Codigo, x.Descripcion, x.Estado,
                x.FechaInicio, x.FechaFin, x.SucursalId,
                x.TerceroId, x.TotalCuotas, x.Anulada
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET api/proyectos/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Proyectos.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id, x.Codigo, x.Descripcion, x.Observacion,
                x.Estado, x.FechaInicio, x.FechaFin,
                x.SucursalId, x.TerceroId, x.TotalCuotas,
                x.SoloPadre, x.Anulada
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    // GET api/proyectos/{id}/comprobantes
    [HttpGet("{id:long}/comprobantes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComprobantes(long id, CancellationToken ct)
    {
        var result = await db.ComprobantesProyectos.AsNoTracking()
            .Where(cp => cp.ProyectoId == id && !cp.Deshabilitada)
            .Select(cp => new { cp.Id, cp.ComprobanteId, cp.Porcentaje, cp.Importe, cp.NroCuota, cp.Observacion })
            .ToListAsync(ct);
        return Ok(result);
    }

    // POST api/proyectos
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProyectoRequest req, CancellationToken ct)
    {
        var command = new CreateProyectoCommand(
            req.Codigo, req.Descripcion, req.SucursalId,
            req.TerceroId, req.FechaInicio, req.FechaFin,
            req.TotalCuotas, req.SoloPadre, req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    // PUT api/proyectos/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id,
        [FromBody] UpdateProyectoRequest req, CancellationToken ct)
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
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { id });
    }

    // PATCH api/proyectos/{id}/finalizar | anular | reactivar
    [HttpPatch("{id:long}/finalizar")]
    public async Task<IActionResult> Finalizar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new FinalizarProyectoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularProyectoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<IActionResult> Reactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ReactivarProyectoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    // POST api/proyectos/{id}/comprobantes — asignar comprobante al proyecto
    [HttpPost("{id:long}/comprobantes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarComprobante(long id,
        [FromBody] AsignarComprobanteProyectoRequest req, CancellationToken ct)
    {
        var command = new AsignarComprobanteProyectoCommand(
            id,
            req.ComprobanteId,
            req.Porcentaje,
            req.Importe,
            req.NroCuota,
            req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id }, new { id = result.Value });
    }

    // DELETE api/proyectos/{id}/comprobantes/{linkId}
    [HttpDelete("{id:long}/comprobantes/{linkId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesasignarComprobante(long id, long linkId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DesasignarComprobanteProyectoCommand(id, linkId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}

public record CreateProyectoRequest(
    string    Codigo,
    string    Descripcion,
    long      SucursalId,
    long?     TerceroId    = null,
    DateOnly? FechaInicio  = null,
    DateOnly? FechaFin     = null,
    int       TotalCuotas  = 0,
    bool      SoloPadre    = false,
    string?   Observacion  = null);

public record UpdateProyectoRequest(
    string    Descripcion,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    int       TotalCuotas,
    bool      SoloPadre,
    string?   Observacion);

public record AsignarComprobanteProyectoRequest(
    long    ComprobanteId,
    decimal Porcentaje,
    decimal Importe,
    int     NroCuota    = 0,
    string? Observacion = null);
