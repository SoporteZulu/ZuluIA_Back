using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Finanzas.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class CobrosController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Retorna cobros paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
            new GetCobrosPagedQuery(
                page, pageSize,
                sucursalId, terceroId,
                desde, hasta),
            ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de un cobro con sus medios.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetCobroById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCobroDetalleQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna comprobantes pendientes de cobro de un cliente
    /// con cálculo de mora y saldo actualizado.
    /// </summary>
    [HttpGet("clientes/{terceroId:long}/pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComprobantesClientePendientes(
        long terceroId,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? monedaId = null,
        [FromQuery] bool soloVencidos = false,
        [FromQuery] DateOnly? fechaHasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetComprobantesClientePendientesCobroQuery(
                terceroId,
                sucursalId,
                monedaId,
                soloVencidos,
                fechaHasta),
            ct);
        return Ok(result);
    }

    /// <summary>
    /// Registra un cobro con múltiples medios de pago
    /// e imputa opcionalmente comprobantes pendientes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarCobroCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCobroById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Anula un cobro registrado.
    /// </summary>
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
