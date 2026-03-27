using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de Períodos Contables (apertura / cierre).
/// Migrado desde VB6: frmCierrePeriodoContable / PERIODO_CONTABLE.
/// </summary>
public class PeriodosContablesController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/periodos-contables
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? soloAbiertos = null,
        CancellationToken ct = default)
    {
        var q = db.PeriodosContables.AsNoTracking();
        if (soloAbiertos.HasValue) q = q.Where(x => x.Abierto == soloAbiertos.Value);
        var result = await q
            .OrderByDescending(x => x.Periodo)
            .Select(x => new { x.Id, x.Periodo, x.FechaInicio, x.FechaFin, x.Abierto })
            .ToListAsync(ct);
        return Ok(result);
    }

    // GET api/periodos-contables/{id}
    [HttpGet("{id:long}", Name = "GetPeriodoContableById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var p = await db.PeriodosContables.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound(new { error = $"Período contable {id} no encontrado." });
        return Ok(new { p.Id, p.Periodo, p.FechaInicio, p.FechaFin, p.Abierto });
    }

    // POST api/periodos-contables
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] PeriodoContableRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreatePeriodoContableCommand(req.Periodo, req.FechaInicio, req.FechaFin),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetPeriodoContableById", new { id = result.Value }, new { Id = result.Value });
    }

    // POST api/periodos-contables/{id}/cerrar
    [HttpPost("{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cerrar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CerrarPeriodoContableCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { result.Value.Id, result.Value.Periodo, abierto = result.Value.Abierto });
    }

    // POST api/periodos-contables/{id}/abrir
    [HttpPost("{id:long}/abrir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Abrir(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AbrirPeriodoContableCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { result.Value.Id, result.Value.Periodo, abierto = result.Value.Abierto });
    }

    // GET api/periodos-contables/activo
    /// <summary>Devuelve el período contable actualmente abierto, si existe.</summary>
    [HttpGet("activo")]
    public async Task<IActionResult> GetActivo(CancellationToken ct)
    {
        var p = await db.PeriodosContables.AsNoTracking()
            .Where(x => x.Abierto)
            .OrderByDescending(x => x.Periodo)
            .FirstOrDefaultAsync(ct);
        if (p is null) return NotFound(new { error = "No hay período contable abierto." });
        return Ok(new { p.Id, p.Periodo, p.FechaInicio, p.FechaFin });
    }
}

// ── DTO ───────────────────────────────────────────────────────────────────────

public record PeriodoContableRequest(string Periodo, DateOnly FechaInicio, DateOnly FechaFin);
