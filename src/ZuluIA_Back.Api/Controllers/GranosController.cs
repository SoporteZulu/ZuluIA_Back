using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Granos.Commands;
using ZuluIA_Back.Application.Features.Granos.Queries;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/granos")]
public class GranosController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet("liquidaciones")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] long? sucursalId,
        [FromQuery] long? terceroId,
        [FromQuery] string? estado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetLiquidacionesGranoPagedQuery(sucursalId, terceroId, estado, page, pageSize), ct));

    [HttpGet("liquidaciones/{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetLiquidacionGranosDetalleQuery(id), ct));

    [HttpPost("liquidaciones")]
    public async Task<IActionResult> Crear([FromBody] CrearLiquidacionGranosCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("liquidaciones/{id:long}/conceptos")]
    public async Task<IActionResult> AgregarConcepto(long id, [FromBody] AgregarConceptoLiquidacionCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command with { LiquidacionId = id }, ct));

    [HttpPost("liquidaciones/{id:long}/certificaciones")]
    public async Task<IActionResult> AgregarCertificacion(long id, [FromBody] AgregarCertificacionCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command with { LiquidacionId = id }, ct));

    [HttpPost("liquidaciones/{id:long}/emitir")]
    public async Task<IActionResult> Emitir(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new EmitirLiquidacionGranosCommand(id, userId), ct));

    [HttpPost("liquidaciones/{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new AnularLiquidacionGranosCommand(id, userId), ct));
}
