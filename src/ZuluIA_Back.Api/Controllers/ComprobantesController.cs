using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class ComprobantesController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] long? tipoId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetComprobantesPagedQuery(page, pageSize, sucursalId, terceroId, tipoId, estado, desde, hasta), ct);
        return Ok(result);
    }

    [HttpGet("{id:long}", Name = "GetComprobanteById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetComprobanteByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateComprobanteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetComprobanteById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("{id:long}/emitir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Emitir(
        long id,
        [FromBody] EmitirComprobanteRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new EmitirComprobanteCommand(id, request.Cae, request.FechaVtoCae), ct);
        return FromResult(result);
    }

    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularComprobanteCommand(id), ct);
        return FromResult(result);
    }
}

public record EmitirComprobanteRequest(string? Cae, DateOnly? FechaVtoCae);