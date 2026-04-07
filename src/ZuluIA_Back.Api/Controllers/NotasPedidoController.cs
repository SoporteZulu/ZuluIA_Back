using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.NotasPedido.Commands;
using ZuluIA_Back.Application.Features.NotasPedido.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class NotasPedidoController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null, [FromQuery] long? terceroId = null,
        [FromQuery] string? estado = null, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetNotasPedidoPagedQuery(page, pageSize, sucursalId, terceroId, estado), ct));

    [HttpGet("{id:long}", Name = "GetNotaPedidoById")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetNotaPedidoDetalleQuery(id), ct));

    [HttpGet("{id:long}/reimpresion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReimpresion(long id, CancellationToken ct)
    {
        var detalle = await Mediator.Send(new GetNotaPedidoDetalleQuery(id), ct);
        if (detalle is null)
            return NotFound();

        return Ok(new NotaPedidoReimpresionResponse(true, DateTimeOffset.UtcNow, detalle));
    }

    [HttpGet("pendientes/{sucursalId:long}")]
    public async Task<IActionResult> GetPendientes(long sucursalId, CancellationToken ct)
        => Ok(await Mediator.Send(new GetNotasPedidoPendientesQuery(sucursalId), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearNotaPedidoCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetNotaPedidoById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AnularNotaPedidoCommand(id), ct));
}

public record NotaPedidoReimpresionResponse(
    bool EsReimpresion,
    DateTimeOffset GeneradoEn,
    object Documento);
