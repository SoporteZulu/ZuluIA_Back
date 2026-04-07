using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Configuracion.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestion de Planillas y Plantillas de Diagnostico.
/// VB6: clsPlanillaDiagnostico (FRA_PLANILLAS) + clsPlantillaDiagnostico (FRA_PLANTILLAS).
/// </summary>
[Route("api/planillas")]
public class PlanillasController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    // -- Plantillas ------------------------------------------------------------

    [HttpGet("plantillas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlantillas(CancellationToken ct)
    {
        var lista = await db.PlantillasDiagnostico
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Descripcion,
                p.FechaDesde,
                p.FechaHasta,
                p.FechaRegistro,
                p.Observaciones
            })
            .OrderBy(p => p.Descripcion)
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpGet("plantillas/{id:long}", Name = "GetPlantillaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlantillaById(long id, CancellationToken ct)
    {
        var p = await db.PlantillasDiagnostico.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound(new { error = $"Plantilla {id} no encontrada." });

        return Ok(new
        {
            p.Id,
            p.Descripcion,
            p.FechaDesde,
            p.FechaHasta,
            p.FechaRegistro,
            p.Observaciones
        });
    }

    [HttpPost("plantillas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlantilla([FromBody] PlantillaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreatePlantillaDiagnosticoCommand(req.Descripcion, req.FechaDesde, req.FechaHasta, req.Observaciones),
            ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPlantillaById), new { id = result.Value }, new { Id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("plantillas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlantilla(long id, [FromBody] PlantillaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdatePlantillaDiagnosticoCommand(id, req.Descripcion, req.FechaDesde, req.FechaHasta, req.Observaciones),
            ct);

        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible actualizar la plantilla.";
            return error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return Ok(new { Id = id });
    }

    [HttpDelete("plantillas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlantilla(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeletePlantillaDiagnosticoCommand(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok();
    }

    // -- Detalle de Plantillas -------------------------------------------------

    [HttpGet("plantillas/{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlantillaDetalle(long id, CancellationToken ct)
    {
        var lista = await db.PlantillasDiagnosticoDetalle
            .AsNoTracking()
            .Where(d => d.PlantillaId == id)
            .Select(d => new
            {
                d.Id,
                d.PlantillaId,
                d.VariableDetalleId,
                d.PorcentajeIncidencia,
                d.ValorObjetivo
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpPost("plantillas/{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPlantillaDetalle(long id, [FromBody] PlantillaDetalleRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddPlantillaDiagnosticoDetalleCommand(id, req.VariableDetalleId, req.PorcentajeIncidencia, req.ValorObjetivo),
            ct);

        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible agregar el detalle de plantilla.";
            return error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return CreatedAtAction(nameof(GetPlantillaDetalle), new { id }, new { Id = result.Value });
    }

    [HttpPut("plantillas/{id:long}/detalle/{detalleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlantillaDetalle(long id, long detalleId, [FromBody] PlantillaDetalleRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdatePlantillaDiagnosticoDetalleCommand(id, detalleId, req.VariableDetalleId, req.PorcentajeIncidencia, req.ValorObjetivo),
            ct);

        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(new { Id = detalleId });
    }

    [HttpDelete("plantillas/{id:long}/detalle/{detalleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlantillaDetalle(long id, long detalleId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeletePlantillaDiagnosticoDetalleCommand(id, detalleId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok();
    }

    // -- Planillas -------------------------------------------------------------

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanillas(
        [FromQuery] long? clienteId = null,
        [FromQuery] long? plantillaId = null,
        CancellationToken ct = default)
    {
        var query = db.PlanillasDiagnostico.AsNoTracking();
        if (clienteId.HasValue) query = query.Where(p => p.ClienteId == clienteId);
        if (plantillaId.HasValue) query = query.Where(p => p.PlantillaId == plantillaId);

        var lista = await query
            .Select(p => new
            {
                p.Id,
                p.ClienteId,
                p.PlantillaId,
                p.TipoPlanillaId,
                p.PlanillaPadreId,
                p.EstadoId,
                p.FechaEvaluacion,
                p.FechaDesde,
                p.FechaHasta,
                p.FechaRegistro,
                p.Observaciones
            })
            .OrderByDescending(p => p.FechaRegistro)
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpGet("{id:long}", Name = "GetPlanillaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanillaById(long id, CancellationToken ct)
    {
        var p = await db.PlanillasDiagnostico.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound(new { error = $"Planilla {id} no encontrada." });

        return Ok(new
        {
            p.Id,
            p.ClienteId,
            p.PlantillaId,
            p.TipoPlanillaId,
            p.PlanillaPadreId,
            p.EstadoId,
            p.FechaEvaluacion,
            p.FechaDesde,
            p.FechaHasta,
            p.FechaRegistro,
            p.Observaciones
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlanilla([FromBody] PlanillaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreatePlanillaDiagnosticoCommand(
                req.ClienteId,
                req.PlantillaId,
                req.TipoPlanillaId,
                req.PlanillaPadreId,
                req.EstadoId,
                req.FechaEvaluacion,
                req.FechaDesde,
                req.FechaHasta,
                req.Observaciones),
            ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPlanillaById), new { id = result.Value }, new { Id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlanilla(long id, [FromBody] PlanillaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdatePlanillaDiagnosticoCommand(
                id,
                req.ClienteId,
                req.PlantillaId,
                req.TipoPlanillaId,
                req.PlanillaPadreId,
                req.EstadoId,
                req.FechaEvaluacion,
                req.FechaDesde,
                req.FechaHasta,
                req.Observaciones),
            ct);

        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(new { Id = id });
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlanilla(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeletePlanillaDiagnosticoCommand(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok();
    }

    // -- Detalle de Planillas --------------------------------------------------

    [HttpGet("{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanillaDetalle(long id, CancellationToken ct)
    {
        var lista = await db.PlanillasDiagnosticoDetalle
            .AsNoTracking()
            .Where(d => d.PlanillaId == id)
            .Select(d => new
            {
                d.Id,
                d.PlanillaId,
                d.VariableDetalleId,
                d.OpcionVariableId,
                d.PuntajeVariable,
                d.Valor,
                d.PorcentajeIncidencia,
                d.ValorObjetivo
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpPost("{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPlanillaDetalle(long id, [FromBody] PlanillaDetalleRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddPlanillaDiagnosticoDetalleCommand(
                id,
                req.VariableDetalleId,
                req.OpcionVariableId,
                req.PuntajeVariable,
                req.Valor,
                req.PorcentajeIncidencia,
                req.ValorObjetivo),
            ct);

        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible agregar el detalle de planilla.";
            return error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return CreatedAtAction(nameof(GetPlanillaDetalle), new { id }, new { Id = result.Value });
    }

    [HttpPut("{id:long}/detalle/{detalleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlanillaDetalle(long id, long detalleId, [FromBody] PlanillaDetalleRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdatePlanillaDiagnosticoDetalleCommand(
                id,
                detalleId,
                req.OpcionVariableId,
                req.PuntajeVariable,
                req.Valor,
                req.PorcentajeIncidencia,
                req.ValorObjetivo),
            ct);

        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(new { Id = detalleId });
    }

    [HttpDelete("{id:long}/detalle/{detalleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlanillaDetalle(long id, long detalleId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeletePlanillaDiagnosticoDetalleCommand(id, detalleId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record PlantillaRequest(
    string Descripcion,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null,
    string? Observaciones = null);

public record PlantillaDetalleRequest(
    long? VariableDetalleId = null,
    decimal PorcentajeIncidencia = 0,
    decimal? ValorObjetivo = null);

public record PlanillaRequest(
    long? ClienteId = null,
    long? PlantillaId = null,
    long? TipoPlanillaId = null,
    long? PlanillaPadreId = null,
    long? EstadoId = null,
    DateTime? FechaEvaluacion = null,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null,
    string? Observaciones = null);

public record PlanillaDetalleRequest(
    long? VariableDetalleId = null,
    long? OpcionVariableId = null,
    decimal PuntajeVariable = 0,
    decimal Valor = 0,
    decimal PorcentajeIncidencia = 0,
    decimal? ValorObjetivo = null);
