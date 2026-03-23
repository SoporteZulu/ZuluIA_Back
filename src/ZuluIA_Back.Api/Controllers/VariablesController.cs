using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Configuracion.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Administracion del sistema de Variables y Aspectos dinamicos.
/// Equivale a FRA_ASPECTOS + FRA_VARIABLES del modulo VB6 (clsAspectos + clsVariables).
/// </summary>
public class VariablesController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // -- Aspectos --------------------------------------------------------------

    [HttpGet("aspectos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAspectos(
        [FromQuery] long? padreId = null,
        CancellationToken ct = default)
    {
        var q = db.Aspectos.AsNoTracking();
        if (padreId.HasValue)
            q = q.Where(a => a.AspectoPadreId == padreId.Value);

        var lista = await q
            .OrderBy(a => a.Orden).ThenBy(a => a.Codigo)
            .Select(a => new
            {
                a.Id,
                a.Codigo,
                a.Descripcion,
                a.AspectoPadreId,
                a.Nivel,
                a.Orden,
                a.CodigoEstructura,
                a.Observacion
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpGet("aspectos/{id:long}", Name = "GetAspectoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAspectoById(long id, CancellationToken ct)
    {
        var aspecto = await db.Aspectos.FindAsync([id], ct);
        return aspecto is null ? NotFound(new { error = $"Aspecto {id} no encontrado." }) : Ok(aspecto);
    }

    [HttpPost("aspectos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAspecto(
        [FromBody] CreateAspectoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateAspectoCommand(
                req.Codigo,
                req.Descripcion,
                req.AspectoPadreId,
                req.Orden,
                req.Nivel,
                req.CodigoEstructura,
                req.Observacion),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetAspectoById", new { id = result.Value }, new { Id = result.Value, req.Codigo });
    }

    [HttpPut("aspectos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAspecto(
        long id,
        [FromBody] UpdateAspectoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateAspectoCommand(
                id,
                req.Descripcion,
                req.AspectoPadreId,
                req.Orden,
                req.Nivel,
                req.CodigoEstructura,
                req.Observacion),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpDelete("aspectos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAspecto(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteAspectoCommand(id), ct);
        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible eliminar el aspecto.";
            if (error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error });
            if (error.Contains("variables asociadas", StringComparison.OrdinalIgnoreCase)
                || error.Contains("sub-aspectos", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { error });
            return BadRequest(new { error });
        }

        return Ok();
    }

    // -- Variables -------------------------------------------------------------

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVariables(
        [FromQuery] long? aspectoId = null,
        [FromQuery] long? tipoComprobanteId = null,
        CancellationToken ct = default)
    {
        var q = db.Variables.AsNoTracking();
        if (aspectoId.HasValue)
            q = q.Where(v => v.AspectoId == aspectoId.Value);
        if (tipoComprobanteId.HasValue)
            q = q.Where(v => v.TipoComprobanteId == tipoComprobanteId.Value);

        var lista = await q
            .OrderBy(v => v.Orden).ThenBy(v => v.Codigo)
            .Select(v => new
            {
                v.Id,
                v.Codigo,
                v.Descripcion,
                v.AspectoId,
                v.TipoVariableId,
                v.TipoComprobanteId,
                v.Nivel,
                v.Orden,
                v.CodigoEstructura,
                v.Condicionante,
                v.Editable,
                v.Observacion
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpGet("{id:long}", Name = "GetVariableById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariableById(long id, CancellationToken ct)
    {
        var variable = await db.Variables.FindAsync([id], ct);
        return variable is null ? NotFound(new { error = $"Variable {id} no encontrada." }) : Ok(variable);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVariable(
        [FromBody] CreateVariableRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateVariableCommand(
                req.Codigo,
                req.Descripcion,
                req.TipoVariableId,
                req.TipoComprobanteId,
                req.AspectoId,
                req.Nivel,
                req.Orden,
                req.CodigoEstructura,
                req.Observacion,
                req.Condicionante,
                req.Editable),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetVariableById", new { id = result.Value }, new { Id = result.Value, req.Codigo });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariable(
        long id,
        [FromBody] UpdateVariableRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateVariableCommand(
                id,
                req.Descripcion,
                req.TipoVariableId,
                req.TipoComprobanteId,
                req.AspectoId,
                req.Nivel,
                req.Orden,
                req.CodigoEstructura,
                req.Observacion,
                req.Condicionante,
                req.Editable),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVariable(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteVariableCommand(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok();
    }

    // -- Opciones de Variable --------------------------------------------------

    [HttpGet("opciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpciones(CancellationToken ct)
    {
        var lista = await db.OpcionesVariable
            .OrderBy(o => o.Codigo)
            .Select(o => new { o.Id, o.Codigo, o.Descripcion, o.Observaciones })
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpGet("opciones/{id:long}", Name = "GetOpcionVariableById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOpcionById(long id, CancellationToken ct)
    {
        var opcion = await db.OpcionesVariable.FindAsync([id], ct);
        return opcion is null
            ? NotFound(new { error = $"Opcion de variable {id} no encontrada." })
            : Ok(opcion);
    }

    [HttpPost("opciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOpcion(
        [FromBody] CreateOpcionVariableRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateOpcionVariableCommand(req.Codigo, req.Descripcion, req.Observaciones), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetOpcionVariableById", new { id = result.Value }, new { Id = result.Value, req.Codigo });
    }

    [HttpPut("opciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOpcion(
        long id,
        [FromBody] UpdateOpcionVariableRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateOpcionVariableCommand(id, req.Codigo, req.Descripcion, req.Observaciones), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpDelete("opciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOpcion(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteOpcionVariableCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    // -- Detalle de Variables --------------------------------------------------

    [HttpGet("{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetalle(long id, CancellationToken ct)
    {
        var lista = await db.VariablesDetalle
            .AsNoTracking()
            .Where(v => v.VariableId == id)
            .Select(v => new
            {
                v.Id,
                v.VariableId,
                v.OpcionVariableId,
                v.AplicaPuntajePenalizacion,
                v.VisualizarOpcion,
                v.PorcentajeIncidencia,
                v.ValorObjetivo
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    [HttpPost("{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDetalle(long id, [FromBody] VariableDetalleRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddVariableDetalleCommand(
                id,
                req.OpcionVariableId,
                req.AplicaPuntajePenalizacion,
                req.VisualizarOpcion,
                req.PorcentajeIncidencia,
                req.ValorObjetivo),
            ct);

        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible agregar el detalle.";
            return error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return CreatedAtAction(nameof(GetDetalle), new { id }, new { Id = result.Value });
    }

    [HttpPut("{id:long}/detalle/{detalleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDetalle(long id, long detalleId, [FromBody] VariableDetalleRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateVariableDetalleCommand(
                id,
                detalleId,
                req.OpcionVariableId,
                req.AplicaPuntajePenalizacion,
                req.VisualizarOpcion,
                req.PorcentajeIncidencia,
                req.ValorObjetivo),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = detalleId });
    }

    [HttpDelete("{id:long}/detalle/{detalleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDetalle(long id, long detalleId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteVariableDetalleCommand(id, detalleId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record CreateAspectoRequest(
    string Codigo,
    string Descripcion,
    long? AspectoPadreId = null,
    int Orden = 0,
    int Nivel = 0,
    string? CodigoEstructura = null,
    string? Observacion = null);

public record UpdateAspectoRequest(
    string Descripcion,
    long? AspectoPadreId = null,
    int Orden = 0,
    int Nivel = 0,
    string? CodigoEstructura = null,
    string? Observacion = null);

public record CreateVariableRequest(
    string Codigo,
    string Descripcion,
    long? TipoVariableId = null,
    long? TipoComprobanteId = null,
    long? AspectoId = null,
    int Nivel = 0,
    int Orden = 0,
    string? CodigoEstructura = null,
    string? Observacion = null,
    string? Condicionante = null,
    bool Editable = true);

public record UpdateVariableRequest(
    string Descripcion,
    long? TipoVariableId = null,
    long? TipoComprobanteId = null,
    long? AspectoId = null,
    int Nivel = 0,
    int Orden = 0,
    string? CodigoEstructura = null,
    string? Observacion = null,
    string? Condicionante = null,
    bool Editable = true);

public record CreateOpcionVariableRequest(
    string Codigo,
    string Descripcion,
    string? Observaciones = null);

public record UpdateOpcionVariableRequest(
    string Codigo,
    string Descripcion,
    string? Observaciones = null);

public record VariableDetalleRequest(
    long? OpcionVariableId = null,
    bool AplicaPuntajePenalizacion = false,
    bool VisualizarOpcion = true,
    decimal? PorcentajeIncidencia = null,
    decimal? ValorObjetivo = null);
