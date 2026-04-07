using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.TasasInteres.Commands;
using ZuluIA_Back.Application.Features.TasasInteres.Queries;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/tasas-interes")]
public class TasasInteresController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivas, CancellationToken ct)
        => Ok(await Mediator.Send(new GetTasasInteresQuery(soloActivas), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTasaInteresCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("{id:long}/desactivar")]
    [HttpPatch("{id:long}/desactivar")]
    public async Task<IActionResult> Desactivar(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new DesactivarTasaInteresCommand(id, userId), ct));

    [HttpPatch("{id:long}/activar")]
    public async Task<IActionResult> Activar(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new ActivarTasaInteresCommand(id, userId), ct));
}
