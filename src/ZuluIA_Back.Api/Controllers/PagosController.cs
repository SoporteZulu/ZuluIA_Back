using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Pagos.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class PagosController(IMediator mediator) : BaseController(mediator)
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePagoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularPagoCommand(id), ct);
        return FromResult(result);
    }
}