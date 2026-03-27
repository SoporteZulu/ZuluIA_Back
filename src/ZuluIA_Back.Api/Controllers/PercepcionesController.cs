using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Impuestos.Commands;
using ZuluIA_Back.Domain.Entities.Impuestos;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Percepciones e impuestos adicionales (IIBB, Ganancias, IVA especial, etc.)
/// Migrado desde VB6: clsImpuesto / IMP_IMPUESTO + IMP_IMPUESTOXPERSONA + IMP_IMPUESTOXITEM.
/// </summary>
public class PercepcionesController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/percepciones?tipo=percepcion
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? tipo,
        [FromQuery] bool? activo,
        CancellationToken ct)
    {
        var q = db.Impuestos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(tipo)) q = q.Where(x => x.Tipo == tipo.ToLowerInvariant());
        if (activo.HasValue)                  q = q.Where(x => x.Activo == activo.Value);

        var result = await q
            .OrderBy(x => x.Tipo)
            .ThenBy(x => x.Codigo)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.Tipo,
                x.Alicuota,
                x.MinimoBaseCalculo,
                x.Observacion,
                x.Activo
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET api/percepciones/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Impuestos.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.Tipo,
                x.Alicuota,
                x.MinimoBaseCalculo,
                x.Observacion,
                x.Activo
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    // POST api/percepciones
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateImpuestoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateImpuestoCommand(
                req.Codigo,
                req.Descripcion,
                req.Alicuota,
                req.MinimoBaseCalculo,
                req.Tipo,
                req.Observacion),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    // PUT api/percepciones/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateImpuestoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateImpuestoCommand(id, req.Descripcion, req.Alicuota, req.MinimoBaseCalculo, req.Tipo, req.Observacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound()
                : BadRequest(new { error = result.Error });

        return Ok(new { id });
    }

    // PATCH api/percepciones/{id}/activar
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateImpuestoCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }

    // PATCH api/percepciones/{id}/desactivar
    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateImpuestoCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok();
    }

    // GET api/percepciones/tercero/{terceroId}
    [HttpGet("tercero/{terceroId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTercero(long terceroId, CancellationToken ct)
    {
        var result = await db.ImpuestosPorPersona.AsNoTracking()
            .Where(x => x.TerceroId == terceroId)
            .Join(db.Impuestos,
                  xp => xp.ImpuestoId,
                  imp => imp.Id,
                  (xp, imp) => new
                  {
                      xp.Id,
                      xp.TerceroId,
                      xp.ImpuestoId,
                      imp.Codigo,
                      imp.Descripcion,
                      imp.Tipo,
                      imp.Alicuota,
                      imp.MinimoBaseCalculo,
                      Observacion = xp.Observacion ?? imp.Observacion
                  })
            .ToListAsync(ct);

        return Ok(result);
    }

    // POST api/percepciones/{impuestoId}/asignar-tercero
    [HttpPost("{impuestoId:long}/asignar-tercero")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarTercero(
        long impuestoId,
        [FromBody] AsignarImpuestoTerceroRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AssignImpuestoTerceroCommand(impuestoId, req.TerceroId, req.Descripcion, req.Observacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("ya esta asignado", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : NotFound();

        return CreatedAtAction(nameof(GetByTercero), new { terceroId = req.TerceroId }, new { id = result.Value });
    }

    // DELETE api/percepciones/{impuestoId}/tercero/{terceroId}
    [HttpDelete("{impuestoId:long}/tercero/{terceroId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesasignarTercero(long impuestoId, long terceroId, CancellationToken ct)
    {
        var result = await Mediator.Send(new UnassignImpuestoTerceroCommand(impuestoId, terceroId), ct);
        if (result.IsFailure)
            return NotFound();

        return NoContent();
    }

    // GET api/percepciones/item/{itemId}
    [HttpGet("item/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByItem(long itemId, CancellationToken ct)
    {
        var result = await db.ImpuestosPorItem.AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .Join(db.Impuestos,
                  xi  => xi.ImpuestoId,
                  imp => imp.Id,
                  (xi, imp) => new
                  {
                      xi.Id,
                      xi.ItemId,
                      xi.ImpuestoId,
                      imp.Codigo,
                      imp.Descripcion,
                      imp.Tipo,
                      imp.Alicuota,
                      imp.MinimoBaseCalculo,
                      Observacion = xi.Observacion ?? imp.Observacion
                  })
            .ToListAsync(ct);

        return Ok(result);
    }

    // POST api/percepciones/{impuestoId}/asignar-item
    [HttpPost("{impuestoId:long}/asignar-item")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarItem(
        long impuestoId,
        [FromBody] AsignarImpuestoItemRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AssignImpuestoItemCommand(impuestoId, req.ItemId, req.Descripcion, req.Observacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("ya esta asignado", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : NotFound();

        return CreatedAtAction(nameof(GetByItem), new { itemId = req.ItemId }, new { id = result.Value });
    }

    // DELETE api/percepciones/{impuestoId}/item/{itemId}
    [HttpDelete("{impuestoId:long}/item/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesasignarItem(long impuestoId, long itemId, CancellationToken ct)
    {
        var result = await Mediator.Send(new UnassignImpuestoItemCommand(impuestoId, itemId), ct);
        if (result.IsFailure)
            return NotFound();

        return NoContent();
    }

    // POST api/percepciones/calcular
    /// <summary>
    /// Calcula el total de percepciones aplicables a un tercero para un importe dado.
    /// Solo aplica el impuesto si importeBase &gt;= MinimoBaseCalculo.
    /// </summary>
    [HttpPost("calcular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Calcular(
        [FromBody] CalcularPercepcionRequest req,
        CancellationToken ct)
    {
        var impuestosDelTercero = await db.ImpuestosPorPersona.AsNoTracking()
            .Where(x => x.TerceroId == req.TerceroId)
            .Join(db.Impuestos.Where(i => i.Activo),
                  xp  => xp.ImpuestoId,
                  imp => imp.Id,
                  (xp, imp) => new { imp.Codigo, imp.Descripcion, imp.Alicuota, imp.MinimoBaseCalculo })
            .ToListAsync(ct);

        var detalle = impuestosDelTercero
            .Where(i => req.ImporteBase >= i.MinimoBaseCalculo)
            .Select(i => new
            {
                i.Codigo,
                i.Descripcion,
                i.Alicuota,
                Importe = Math.Round(req.ImporteBase * i.Alicuota / 100m, 2)
            })
            .ToList();

        return Ok(new
        {
            req.TerceroId,
            req.ImporteBase,
            percepciones      = detalle,
            totalPercepciones = detalle.Sum(d => d.Importe)
        });
    }

    // ── ImpuestoPorSucursal ───────────────────────────────────────────────────
    // VB6: clsImpuestoXSucursal / IMP_IMPUESTOXSUCURSAL

    [HttpGet("{impuestoId:long}/sucursales")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSucursales(long impuestoId, CancellationToken ct)
    {
        var exists = await db.Impuestos.AnyAsync(x => x.Id == impuestoId, ct);
        if (!exists) return NotFound();

        var list = await db.ImpuestosPorSucursal
            .Where(x => x.ImpuestoId == impuestoId)
            .Select(x => new { x.Id, x.ImpuestoId, x.SucursalId, x.Descripcion, x.Observacion })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpPost("{impuestoId:long}/sucursales")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarSucursal(
        long impuestoId, [FromBody] AsignarImpuestoSucursalRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AssignImpuestoSucursalCommand(impuestoId, req.SucursalId, req.Descripcion, req.Observacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("ya esta asignada", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : NotFound();

        return CreatedAtAction(nameof(GetSucursales), new { impuestoId }, new { id = result.Value });
    }

    [HttpPut("{impuestoId:long}/sucursales/{asigId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSucursal(
        long impuestoId, long asigId, [FromBody] UpdateImpuestoSucursalRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateImpuestoSucursalCommand(impuestoId, asigId, req.Descripcion, req.Observacion),
            ct);

        if (result.IsFailure)
            return NotFound();

        return Ok(new { result.Value.Id, result.Value.Descripcion, result.Value.Observacion });
    }

    [HttpDelete("{impuestoId:long}/sucursales/{asigId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarSucursal(long impuestoId, long asigId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteImpuestoSucursalCommand(impuestoId, asigId), ct);
        if (result.IsFailure)
            return NotFound();

        return NoContent();
    }

    // ── ImpuestoPorTipoComprobante ────────────────────────────────────────────
    // VB6: frmRentasBSAS / IMP_IMPUESTOXTIPOCOMPROBANTE

    [HttpGet("{impuestoId:long}/tipos-comprobante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTiposComprobante(long impuestoId, CancellationToken ct)
    {
        if (!await db.Impuestos.AnyAsync(x => x.Id == impuestoId, ct)) return NotFound();

        var list = await db.ImpuestosPorTipoComprobante
            .Where(x => x.ImpuestoId == impuestoId)
            .Select(x => new { x.Id, x.ImpuestoId, x.TipoComprobanteId, x.Orden })
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpPost("{impuestoId:long}/tipos-comprobante")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarTipoComprobante(
        long impuestoId, [FromBody] AsignarImpuestoTipoComprobanteRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AssignImpuestoTipoComprobanteCommand(impuestoId, req.TipoComprobanteId, req.Orden),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("ya esta asignado", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : NotFound();

        return CreatedAtAction(nameof(GetTiposComprobante), new { impuestoId }, new { id = result.Value });
    }

    [HttpDelete("{impuestoId:long}/tipos-comprobante/{asigId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarTipoComprobante(long impuestoId, long asigId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteImpuestoTipoComprobanteCommand(impuestoId, asigId), ct);
        if (result.IsFailure)
            return NotFound();

        return NoContent();
    }
}

public record CreateImpuestoRequest(
    string  Codigo,
    string  Descripcion,
    decimal Alicuota,
    decimal MinimoBaseCalculo = 0m,
    string? Tipo = "percepcion",
    string? Observacion = null);

public record UpdateImpuestoRequest(
    string  Descripcion,
    decimal Alicuota,
    decimal MinimoBaseCalculo,
    string  Tipo,
    string? Observacion);

public record AsignarImpuestoTerceroRequest(
    long    TerceroId,
    string? Descripcion = null,
    string? Observacion = null);

public record AsignarImpuestoItemRequest(
    long    ItemId,
    string? Descripcion = null,
    string? Observacion = null);

public record CalcularPercepcionRequest(
    long    TerceroId,
    decimal ImporteBase);

public record AsignarImpuestoSucursalRequest(
    long    SucursalId,
    string? Descripcion = null,
    string? Observacion = null);

public record UpdateImpuestoSucursalRequest(
    string? Descripcion = null,
    string? Observacion = null);

public record AsignarImpuestoTipoComprobanteRequest(
    long TipoComprobanteId,
    int  Orden = 0);
