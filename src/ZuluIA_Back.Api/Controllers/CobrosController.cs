using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Cobros.Commands;
using ZuluIA_Back.Application.Features.Cobros.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class CobrosController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet("{id:long}", Name = "GetCobroById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCobroByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCobroCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCobroById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularCobroCommand(id), ct);
        return FromResult(result);
    }
}