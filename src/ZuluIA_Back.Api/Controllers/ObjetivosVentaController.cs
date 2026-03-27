using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.ObjetivosVenta.Commands;
using ZuluIA_Back.Application.Features.ObjetivosVenta.Queries;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/objetivos-venta")]
public class ObjetivosVentaController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] long sucursalId,
        [FromQuery] long? vendedorId,
        [FromQuery] int? periodo,
        [FromQuery] bool? cerrado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(
            new GetObjetivosVentaQuery(sucursalId, vendedorId, periodo, cerrado, page, pageSize), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearObjetivoVentaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Actualizar(long id, [FromBody] ActualizarObjetivoVentaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command with { Id = id }, ct));

    [HttpPost("{id:long}/cerrar")]
    public async Task<IActionResult> CerrarPeriodo(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new CerrarPeriodoObjetivoCommand(id, userId), ct));
}
