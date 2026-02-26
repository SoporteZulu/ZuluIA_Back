using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class TercerosController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? soloClientes = null,
        [FromQuery] bool? soloProveedores = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetTercerosPagedQuery(page, pageSize, search, soloClientes, soloProveedores), ct);
        return Ok(result);
    }

    [HttpGet("{id:long}", Name = "GetTerceroById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTerceroCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetTerceroById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTerceroCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroCommand(id), ct);
        return FromResult(result);
    }
}