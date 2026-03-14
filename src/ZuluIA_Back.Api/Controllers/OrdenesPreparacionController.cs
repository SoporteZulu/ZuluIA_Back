using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Queries;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de órdenes de preparación / picking para despacho de mercaderías.
/// Equivale a frmOrdenDePreparacion / clsOrdenDePreparacion del sistema VB6.
/// </summary>
public class OrdenesPreparacionController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Retorna las órdenes de preparación paginadas con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] EstadoOrdenPreparacion? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetOrdenesPreparacionPagedQuery(page, pageSize, sucursalId, terceroId, estado, desde, hasta), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de una orden de preparación.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetOrdenPreparacionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrdenPreparacionByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Crea una nueva orden de preparación en estado Pendiente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrdenPreparacionCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtRoute("GetOrdenPreparacionById", new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Confirma una orden de preparación (la marca como Completada).
    /// </summary>
    [HttpPost("{id:long}/confirmar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirmar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ConfirmarOrdenPreparacionCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Anula una orden de preparación.
    /// </summary>
    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularOrdenPreparacionCommand(id), ct);
        return FromResult(result);
    }
}
