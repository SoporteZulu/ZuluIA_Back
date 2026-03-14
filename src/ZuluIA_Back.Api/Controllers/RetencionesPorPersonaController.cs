using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.RetencionesPorPersona.Commands;
using ZuluIA_Back.Application.Features.RetencionesPorPersona.Queries;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de los tipos de retención asignados a un tercero (proveedor).
/// Equivale a RetencionesXPersona del sistema VB6.
/// Determina qué retenciones se practican automáticamente al registrar pagos.
/// </summary>
[Route("api/terceros/{terceroId:long}/retenciones")]
public class RetencionesPorPersonaController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Retorna las retenciones configuradas para un tercero específico.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(long terceroId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetRetencionesPorPersonaQuery(terceroId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Asigna un tipo de retención a un tercero.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Asignar(
        long terceroId,
        [FromBody] AsignarRetencionBodyRequest body,
        CancellationToken ct)
    {
        var command = new AsignarRetencionAPersonaCommand(terceroId, body.TipoRetencionId, body.Descripcion);
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Created($"api/terceros/{terceroId}/retenciones/{result.Value}", new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Quita un tipo de retención de un tercero.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Quitar(long terceroId, long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new QuitarRetencionDePersonaCommand(id), ct);
        return FromResult(result);
    }
}

public record AsignarRetencionBodyRequest(long TipoRetencionId, string? Descripcion);
