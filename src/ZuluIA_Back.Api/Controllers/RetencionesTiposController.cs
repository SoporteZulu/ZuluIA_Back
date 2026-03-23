using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Retenciones.Commands;
using ZuluIA_Back.Application.Features.Retenciones.Queries;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Administración de tipos de retención y sus escalas de cálculo.
/// Equivale al módulo ABM_Retenciones del sistema VB6.
/// </summary>
public class RetencionesTiposController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todos los tipos de retención configurados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloActivos = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetTiposRetencionQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle de un tipo de retención con sus escalas.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetTipoRetencionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTipoRetencionByIdQuery(id), ct);
        return result is null ? NotFound(new { error = $"No se encontró el tipo de retención con ID {id}." }) : Ok(result);
    }

    /// <summary>
    /// Calcula el importe de retención para una base imponible dada.
    /// </summary>
    [HttpGet("{id:long}/calcular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Calcular(
        long id,
        [FromQuery] decimal baseImponible,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CalcularRetencionQuery(id, baseImponible), ct);
        return result.IsSuccess
            ? Ok(new { tipoRetencionId = id, baseImponible, importeRetencion = result.Value })
            : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Crea un nuevo tipo de retención con sus escalas.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTipoRetencionCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtRoute("GetTipoRetencionById", new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza un tipo de retención existente y reemplaza sus escalas.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTipoRetencionCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Da de baja (soft delete) un tipo de retención.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTipoRetencionCommand(id), ct);
        return result.IsSuccess ? Ok() : NotFound(new { error = result.Error });
    }

    // ── Regímenes de retención ────────────────────────────────────────────────

    /// <summary>
    /// Retorna los regímenes parametrizados de un tipo de retención.
    /// </summary>
    [HttpGet("{id:long}/regimenes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegimenes(long id, CancellationToken ct)
    {
        var lista = await db.RetencionesRegimenes
            .Where(r => r.RetencionId == id)
            .OrderBy(r => r.Codigo)
            .Select(r => new
            {
                r.Id,
                r.Codigo,
                r.Descripcion,
                r.Observacion,
                r.RetencionId,
                r.ControlTipoComprobante,
                r.ControlTipoComprobanteAplica,
                r.NoImponible,
                r.NoImponibleAplica,
                r.Alicuota,
                r.AlicuotaAplica,
                r.AlicuotaEscalaAplica,
                r.RetencionMinimo,
                r.RetencionMinimoAplica,
                r.RetencionMaximo,
                r.RetencionMaximoAplica
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Retorna el detalle completo de un régimen.
    /// </summary>
    [HttpGet("{id:long}/regimenes/{regId:long}", Name = "GetRetencionRegimenById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegimenById(long id, long regId, CancellationToken ct)
    {
        var reg = await db.RetencionesRegimenes
            .FirstOrDefaultAsync(r => r.Id == regId && r.RetencionId == id, ct);
        return reg is null ? NotFound(new { error = $"Régimen {regId} no encontrado." }) : Ok(reg);
    }

    /// <summary>
    /// Crea un régimen para un tipo de retención.
    /// </summary>
    [HttpPost("{id:long}/regimenes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRegimen(
        long id,
        [FromBody] CreateRetencionRegimenRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateRetencionRegimenCommand(id, req.Codigo, req.Descripcion, req.Observacion),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetRetencionRegimenById", new { id, regId = result.Value }, new { Id = result.Value });
    }

    /// <summary>
    /// Actualiza los parámetros de cálculo de un régimen.
    /// </summary>
    [HttpPut("{id:long}/regimenes/{regId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRegimen(
        long id, long regId,
        [FromBody] UpdateRetencionRegimenRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateRetencionRegimenCommand(
                id,
                regId,
                req.ControlTipoComprobante,
                req.ControlTipoComprobanteAplica,
                req.BaseImponibleComposicion,
                req.NoImponible,
                req.NoImponibleAplica,
                req.BaseImponiblePorcentaje,
                req.BaseImponiblePorcentajeAplica,
                req.BaseImponibleMinimo,
                req.BaseImponibleMinimoAplica,
                req.BaseImponibleMaximo,
                req.BaseImponibleMaximoAplica,
                req.RetencionComposicion,
                req.RetencionMinimo,
                req.RetencionMinimoAplica,
                req.RetencionMaximo,
                req.RetencionMaximoAplica,
                req.Alicuota,
                req.AlicuotaAplica,
                req.AlicuotaEscalaAplica,
                req.AlicuotaConvenio,
                req.AlicuotaConvenioAplica,
                req.Observacion),
            ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = regId });
    }

    /// <summary>
    /// Elimina un régimen de retención.
    /// </summary>
    [HttpDelete("{id:long}/regimenes/{regId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRegimen(long id, long regId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteRetencionRegimenCommand(id, regId), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record CreateRetencionRegimenRequest(string Codigo, string Descripcion, string? Observacion = null);

public record UpdateRetencionRegimenRequest(
    bool ControlTipoComprobante,
    bool ControlTipoComprobanteAplica,
    string? BaseImponibleComposicion,
    decimal? NoImponible,
    bool NoImponibleAplica,
    decimal? BaseImponiblePorcentaje,
    bool BaseImponiblePorcentajeAplica,
    decimal? BaseImponibleMinimo,
    bool BaseImponibleMinimoAplica,
    decimal? BaseImponibleMaximo,
    bool BaseImponibleMaximoAplica,
    string? RetencionComposicion,
    decimal? RetencionMinimo,
    bool RetencionMinimoAplica,
    decimal? RetencionMaximo,
    bool RetencionMaximoAplica,
    decimal? Alicuota,
    bool AlicuotaAplica,
    bool AlicuotaEscalaAplica,
    decimal? AlicuotaConvenio,
    bool AlicuotaConvenioAplica,
    string? Observacion = null);
