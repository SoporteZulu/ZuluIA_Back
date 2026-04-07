using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.Queries;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/exportaciones-fiscales")]
public class ExportacionesFiscalesController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet("citi-ventas")]
    public async Task<IActionResult> CitiVentas([FromQuery] long sucursalId, [FromQuery] int anio, [FromQuery] int mes, CancellationToken ct)
    {
        var result = await Mediator.Send(new ExportarCitiVentasQuery(sucursalId, anio, mes), ct);
        return File(Encoding.Latin1.GetBytes(result.Contenido), "text/plain", result.NombreArchivo);
    }

    [HttpGet("citi-compras")]
    public async Task<IActionResult> CitiCompras([FromQuery] long sucursalId, [FromQuery] int anio, [FromQuery] int mes, CancellationToken ct)
    {
        var result = await Mediator.Send(new ExportarCitiComprasQuery(sucursalId, anio, mes), ct);
        return File(Encoding.Latin1.GetBytes(result.Contenido), "text/plain", result.NombreArchivo);
    }

    [HttpGet("iibb-percepciones")]
    public async Task<IActionResult> IibbPercepciones([FromQuery] long sucursalId, [FromQuery] int anio, [FromQuery] int mes, CancellationToken ct)
    {
        var result = await Mediator.Send(new ExportarIibbPercepcionesQuery(sucursalId, anio, mes), ct);
        return File(Encoding.Latin1.GetBytes(result.Contenido), "text/plain", result.NombreArchivo);
    }

    [HttpGet("retenciones-ganancias")]
    public async Task<IActionResult> RetencionesGanancias([FromQuery] long sucursalId, [FromQuery] int anio, [FromQuery] int mes, CancellationToken ct)
    {
        var result = await Mediator.Send(new ExportarRetencionesGananciasQuery(sucursalId, anio, mes), ct);
        return File(Encoding.Latin1.GetBytes(result.Contenido), "text/plain", result.NombreArchivo);
    }

    [HttpGet("retenciones-iva")]
    public async Task<IActionResult> RetencionesIva([FromQuery] long sucursalId, [FromQuery] int anio, [FromQuery] int mes, CancellationToken ct)
    {
        var result = await Mediator.Send(new ExportarRetencionesIvaQuery(sucursalId, anio, mes), ct);
        return File(Encoding.Latin1.GetBytes(result.Contenido), "text/plain", result.NombreArchivo);
    }
}
