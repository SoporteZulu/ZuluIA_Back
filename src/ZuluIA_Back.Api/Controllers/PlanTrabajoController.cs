using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.PlanTrabajo.Commands;
using ZuluIA_Back.Application.Features.PlanTrabajo.Queries;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/planes-trabajo")]
public class PlanTrabajoController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] long sucursalId,
        [FromQuery] int? periodo,
        [FromQuery] string? estado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetPlanesTrabajoPagedQuery(sucursalId, periodo, estado, page, pageSize), ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetPlanTrabajoDetalleQuery(id), ct));

    [HttpGet("evaluaciones/{id:long}")]
    public async Task<IActionResult> GetEvaluacion(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetEvaluacionDetalleQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPlanTrabajoCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("{id:long}/kpis")]
    public async Task<IActionResult> AgregarKpi(long id, [FromBody] AgregarKpiCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command with { PlanTrabajoId = id }, ct));

    [HttpPost("{id:long}/evaluaciones")]
    public async Task<IActionResult> RegistrarEvaluacion(long id, [FromBody] RegistrarEvaluacionCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command with { PlanTrabajoId = id }, ct));

    [HttpPost("{id:long}/cerrar")]
    public async Task<IActionResult> Cerrar(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new CerrarPlanTrabajoCommand(id, userId), ct));

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, [FromQuery] long? userId, CancellationToken ct)
        => FromResult(await Mediator.Send(new AnularPlanTrabajoCommand(id, userId), ct));
}
