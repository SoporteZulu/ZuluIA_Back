using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Rankings.Queries;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/rankings")]
public class RankingsController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet("clientes")]
    public async Task<IActionResult> Clientes(
        [FromQuery] long sucursalId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] int top = 10,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetRankingClientesQuery(sucursalId, desde, hasta, top), ct));

    [HttpGet("items")]
    public async Task<IActionResult> Items(
        [FromQuery] long sucursalId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] int top = 10,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetRankingItemsQuery(sucursalId, desde, hasta, top), ct));

    [HttpGet("analisis-mensual")]
    public async Task<IActionResult> AnalisisMensual(
        [FromQuery] long sucursalId,
        [FromQuery] int anio,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetAnalisisMensualQuery(sucursalId, anio), ct));
}
