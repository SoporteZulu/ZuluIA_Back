using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Recibos.Commands;
using ZuluIA_Back.Application.Features.Recibos.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class RecibosController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetRecibosPagedQuery(page, pageSize, sucursalId, terceroId, desde, hasta), ct);
        return Ok(result);
    }

    [HttpGet("{id:long}", Name = "GetReciboById")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetReciboDetalleQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Emitir([FromBody] EmitirReciboCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetReciboById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AnularReciboCommand(id), ct));
}
