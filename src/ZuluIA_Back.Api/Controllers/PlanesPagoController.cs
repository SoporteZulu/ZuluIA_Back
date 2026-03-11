using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.PlanesPago.Commands;
using ZuluIA_Back.Application.Features.PlanesPago.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class PlanesPagoController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Retorna todos los planes de pago.
    /// Por defecto solo los activos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloActivos = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetPlanesPagoQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo plan de pago.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePlanPagoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza un plan de pago existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdatePlanPagoCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva un plan de pago.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeletePlanPagoCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Calcula el importe de cada cuota dado un total y un plan.
    /// Endpoint utilitario para el frontend.
    /// </summary>
    [HttpGet("{id:long}/calcular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Calcular(
        long id,
        [FromQuery] decimal total,
        CancellationToken ct)
    {
        var plan = await Mediator.Send(
            new GetPlanesPagoQuery(false), ct);

        var found = plan.FirstOrDefault(x => x.Id == id);
        if (found is null)
            return NotFound(new { error = $"No se encontró el plan de pago con ID {id}." });

        if (total <= 0)
            return BadRequest(new { error = "El total debe ser mayor a 0." });

        var totalConInteres = Math.Round(total * (1 + found.InteresPct / 100), 2);
        var cuota = found.CantidadCuotas > 0
            ? Math.Round(totalConInteres / found.CantidadCuotas, 2)
            : totalConInteres;

        return Ok(new
        {
            planId = found.Id,
            descripcion = found.Descripcion,
            totalOriginal = total,
            interesPct = found.InteresPct,
            totalConInteres,
            cantidadCuotas = found.CantidadCuotas,
            importeCuota = cuota
        });
    }
}