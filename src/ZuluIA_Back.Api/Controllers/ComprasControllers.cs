using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Application.Features.Compras.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class RequisicionesCompraController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null, [FromQuery] long? solicitanteId = null,
        [FromQuery] string? estado = null, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetRequisicionesCompraPagedQuery(page, pageSize, sucursalId, solicitanteId, estado), ct));

    [HttpGet("{id:long}", Name = "GetRequisicionCompraById")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetRequisicionCompraDetalleQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearRequisicionCompraCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetRequisicionCompraById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("{id:long}/enviar")]
    public async Task<IActionResult> Enviar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new EnviarRequisicionCompraCommand(id), ct));

    [HttpPost("{id:long}/aprobar")]
    public async Task<IActionResult> Aprobar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AprobarRequisicionCompraCommand(id), ct));

    [HttpPost("{id:long}/rechazar")]
    public async Task<IActionResult> Rechazar(long id, [FromBody] string? motivo, CancellationToken ct)
        => FromResult(await Mediator.Send(new RechazarRequisicionCompraCommand(id, motivo), ct));

    [HttpPost("{id:long}/cancelar")]
    public async Task<IActionResult> Cancelar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new CancelarRequisicionCompraCommand(id), ct));
}

public class CotizacionesCompraController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null, [FromQuery] long? proveedorId = null,
        [FromQuery] string? estado = null, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetCotizacionesCompraPagedQuery(page, pageSize, sucursalId, proveedorId, estado), ct));

    [HttpGet("{id:long}", Name = "GetCotizacionCompraById")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetCotizacionCompraDetalleQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearCotizacionCompraCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCotizacionCompraById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("{id:long}/aceptar")]
    public async Task<IActionResult> Aceptar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AceptarCotizacionCompraCommand(id), ct));

    [HttpPost("{id:long}/rechazar")]
    public async Task<IActionResult> Rechazar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new RechazarCotizacionCompraCommand(id), ct));
}
