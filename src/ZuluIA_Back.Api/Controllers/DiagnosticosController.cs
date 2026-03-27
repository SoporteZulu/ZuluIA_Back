using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Diagnosticos.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class DiagnosticosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet("regiones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegiones(CancellationToken ct)
    {
        var items = await db.RegionesDiagnosticas.AsNoTracking()
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Activa })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpPost("regiones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRegion([FromBody] CreateRegionDiagnosticaCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpGet("aspectos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAspectos([FromQuery] long? regionId = null, CancellationToken ct = default)
    {
        var query = db.AspectosDiagnostico.AsNoTracking();
        if (regionId.HasValue)
            query = query.Where(x => x.RegionId == regionId.Value);

        var regiones = await db.RegionesDiagnosticas.AsNoTracking()
            .ToDictionaryAsync(x => x.Id, ct);

        var items = await query.OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.RegionId, x.Codigo, x.Descripcion, x.Peso, x.Activo })
            .ToListAsync(ct);

        return Ok(items.Select(x => new
        {
            x.Id,
            x.RegionId,
            RegionCodigo = regiones.GetValueOrDefault(x.RegionId)?.Codigo ?? "—",
            x.Codigo,
            x.Descripcion,
            x.Peso,
            x.Activo
        }));
    }

    [HttpPost("aspectos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAspecto([FromBody] CreateAspectoDiagnosticoCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpGet("variables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVariables([FromQuery] long? aspectoId = null, CancellationToken ct = default)
    {
        var query = db.VariablesDiagnosticas.AsNoTracking();
        if (aspectoId.HasValue)
            query = query.Where(x => x.AspectoId == aspectoId.Value);

        var opciones = await db.VariablesDiagnosticasOpciones.AsNoTracking().ToListAsync(ct);
        var items = await query.OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.AspectoId, x.Codigo, x.Descripcion, x.Tipo, x.Requerida, x.Peso, x.Activa })
            .ToListAsync(ct);

        return Ok(items.Select(x => new
        {
            x.Id,
            x.AspectoId,
            x.Codigo,
            x.Descripcion,
            Tipo = x.Tipo.ToString().ToUpperInvariant(),
            x.Requerida,
            x.Peso,
            x.Activa,
            Opciones = opciones.Where(o => o.VariableId == x.Id).OrderBy(o => o.Orden).Select(o => new
            {
                o.Id,
                o.Codigo,
                o.Descripcion,
                o.ValorNumerico,
                o.Orden
            })
        }));
    }

    [HttpPost("variables")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVariable([FromBody] CreateVariableDiagnosticaCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpGet("plantillas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlantillas(CancellationToken ct)
    {
        var items = await db.PlantillasDiagnosticas.AsNoTracking()
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Activa, x.Observacion })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("plantillas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlantillaById(long id, CancellationToken ct)
    {
        var plantilla = await db.PlantillasDiagnosticas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (plantilla is null)
            return NotFound(new { error = $"No se encontró la plantilla diagnóstica ID {id}." });

        var enlaces = await db.PlantillasDiagnosticasVariables.AsNoTracking()
            .Where(x => x.PlantillaId == id)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var variableIds = enlaces.Select(x => x.VariableId).Distinct().ToList();
        var variables = await db.VariablesDiagnosticas.AsNoTracking()
            .Where(x => variableIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return Ok(new
        {
            plantilla.Id,
            plantilla.Codigo,
            plantilla.Descripcion,
            plantilla.Activa,
            plantilla.Observacion,
            Variables = enlaces.Select(x => new
            {
                x.Id,
                x.VariableId,
                VariableCodigo = variables.GetValueOrDefault(x.VariableId)?.Codigo ?? "—",
                VariableDescripcion = variables.GetValueOrDefault(x.VariableId)?.Descripcion ?? "—",
                x.Orden
            })
        });
    }

    [HttpPost("plantillas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlantilla([FromBody] CreatePlantillaDiagnosticaCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpGet("planillas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanillas(CancellationToken ct)
    {
        var plantillas = await db.PlantillasDiagnosticas.AsNoTracking().ToDictionaryAsync(x => x.Id, ct);
        var items = await db.PlanillasDiagnosticas.AsNoTracking()
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        return Ok(items.Select(x => new
        {
            x.Id,
            x.PlantillaId,
            PlantillaCodigo = plantillas.GetValueOrDefault(x.PlantillaId)?.Codigo ?? "—",
            x.Nombre,
            x.Fecha,
            x.ResultadoTotal,
            Estado = x.Estado.ToString().ToUpperInvariant(),
            x.Observacion,
            x.CreatedAt
        }));
    }

    [HttpGet("planillas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanillaById(long id, CancellationToken ct)
    {
        var planilla = await db.PlanillasDiagnosticas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (planilla is null)
            return NotFound(new { error = $"No se encontró la planilla diagnóstica ID {id}." });

        var respuestas = await db.PlanillasDiagnosticasRespuestas.AsNoTracking()
            .Where(x => x.PlanillaId == id)
            .ToListAsync(ct);

        var variableIds = respuestas.Select(x => x.VariableId).Distinct().ToList();
        var variables = await db.VariablesDiagnosticas.AsNoTracking()
            .Where(x => variableIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return Ok(new
        {
            planilla.Id,
            planilla.PlantillaId,
            planilla.Nombre,
            planilla.Fecha,
            planilla.ResultadoTotal,
            Estado = planilla.Estado.ToString().ToUpperInvariant(),
            planilla.Observacion,
            Respuestas = respuestas.Select(x => new
            {
                x.Id,
                x.VariableId,
                VariableCodigo = variables.GetValueOrDefault(x.VariableId)?.Codigo ?? "—",
                VariableDescripcion = variables.GetValueOrDefault(x.VariableId)?.Descripcion ?? "—",
                x.OpcionId,
                x.ValorTexto,
                x.ValorNumerico,
                x.PuntajeObtenido
            })
        });
    }

    [HttpPost("planillas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlanilla([FromBody] CreatePlanillaDiagnosticaCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("planillas/{id:long}/evaluar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Evaluar(long id, [FromBody] EvaluarPlanillaDiagnosticaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new EvaluarPlanillaDiagnosticaCommand(id, request.Respuestas, request.Observacion), ct);
        return FromResult(result);
    }
}

public record EvaluarPlanillaDiagnosticaRequest(IReadOnlyList<RespuestaDiagnosticaInput> Respuestas, string? Observacion);
