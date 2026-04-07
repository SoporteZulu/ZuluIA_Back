using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Cotizaciones.Commands;
using ZuluIA_Back.Application.Features.Cotizaciones.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class CotizacionesController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Retorna la cotización vigente de una moneda para una fecha dada.
    /// Si no se especifica fecha, usa la fecha de hoy.
    /// </summary>
    [HttpGet("{monedaId:long}/vigente")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVigente(
        long monedaId,
        [FromQuery] DateOnly? fecha,
        CancellationToken ct)
    {
        var fechaConsulta = fecha ?? DateOnly.FromDateTime(DateTime.Today);
        var result = await Mediator.Send(
            new GetCotizacionVigenteQuery(monedaId, fechaConsulta), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna el historial de cotizaciones de una moneda.
    /// Opcionalmente filtra por rango de fechas.
    /// </summary>
    [HttpGet("{monedaId:long}/historico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorico(
        long monedaId,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetHistoricoCotizacionesQuery(monedaId, desde, hasta), ct);
        return Ok(result);
    }

    /// <summary>
    /// Registra o actualiza la cotización de una moneda para una fecha.
    /// Si ya existe cotización para esa fecha, la actualiza (upsert).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarCotizacionCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value, mensaje = "Cotización registrada correctamente." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Importa o actualiza cotizaciones en lote para una moneda.
    /// Reutiliza la misma lógica de upsert por fecha que el alta individual.
    /// </summary>
    [HttpPost("importar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Importar(
        [FromBody] ImportarCotizacionesRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ImportarCotizacionesCommand(
                request.MonedaId,
                request.Items.Select(x => new ImportarCotizacionItemInput(x.Fecha, x.Cotizacion)).ToList()),
            ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
}

public record ImportarCotizacionItemRequest(DateOnly Fecha, decimal Cotizacion);

public record ImportarCotizacionesRequest(
    long MonedaId,
    List<ImportarCotizacionItemRequest> Items);