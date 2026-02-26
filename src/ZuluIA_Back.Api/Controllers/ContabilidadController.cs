using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class ContabilidadController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet("asientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsientos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long ejercicioId = 0,
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estado = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetAsientosPagedQuery(page, pageSize, ejercicioId, sucursalId, estado), ct);
        return Ok(result);
    }

    [HttpGet("asientos/{id:long}", Name = "GetAsientoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsientoById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAsientoByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    [HttpPost("asientos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsiento(
        [FromBody] CreateAsientoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetAsientoById", new { id = result.IsSuccess ? result.Value : 0 });
    }
}