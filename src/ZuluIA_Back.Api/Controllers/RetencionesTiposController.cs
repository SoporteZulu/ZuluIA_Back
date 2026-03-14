using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Retenciones.Commands;
using ZuluIA_Back.Application.Features.Retenciones.Queries;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Administración de tipos de retención y sus escalas de cálculo.
/// Equivale al módulo ABM_Retenciones del sistema VB6.
/// </summary>
public class RetencionesTiposController(IMediator mediator) : BaseController(mediator)
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
}
