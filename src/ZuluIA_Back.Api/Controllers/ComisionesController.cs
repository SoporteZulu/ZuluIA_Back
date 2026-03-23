using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Comisiones.Commands;
using ZuluIA_Back.Application.Features.Comisiones.Queries;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/comisiones")]
public class ComisionesController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] long sucursalId,
        [FromQuery] long? vendedorId,
        [FromQuery] int? periodo,
        [FromQuery] string? estado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(
            new GetComisionesVendedorQuery(sucursalId, vendedorId, periodo, estado, page, pageSize), ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetComisionDetalleQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarComisionVendedorCommand command,
        CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("{id:long}/aprobar")]
    public async Task<IActionResult> Aprobar(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new AprobarComisionCommand(id, userId), ct));

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new AnularComisionCommand(id, userId), ct));
}
