using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Facturacion.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class LibroIvaController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Genera el Libro IVA Ventas para una sucursal y período.
    /// </summary>
    [HttpGet("ventas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetVentas(
        [FromQuery] long sucursalId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        CancellationToken ct)
    {
        if (hasta < desde)
            return BadRequest(new
            {
                error = "La fecha hasta no puede ser anterior a la fecha desde."
            });

        var result = await Mediator.Send(
            new GetLibroIvaQuery(sucursalId, desde, hasta, TipoLibroIva.Ventas), ct);

        return Ok(result);
    }

    /// <summary>
    /// Genera el Libro IVA Compras para una sucursal y período.
    /// </summary>
    [HttpGet("compras")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCompras(
        [FromQuery] long sucursalId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        CancellationToken ct)
    {
        if (hasta < desde)
            return BadRequest(new
            {
                error = "La fecha hasta no puede ser anterior a la fecha desde."
            });

        var result = await Mediator.Send(
            new GetLibroIvaQuery(sucursalId, desde, hasta, TipoLibroIva.Compras), ct);

        return Ok(result);
    }
}