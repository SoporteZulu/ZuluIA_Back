using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Reportes.Commands;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;

namespace ZuluIA_Back.Api.Controllers;

public class ReportesController(IMediator mediator, ReportesService reportesService) : BaseController(mediator)
{
    [HttpGet("remitos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRemitos([FromQuery] long? sucursalId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, [FromQuery] bool? esVenta, CancellationToken ct)
        => Ok(await reportesService.GetReporteRemitosAsync(sucursalId, desde, hasta, esVenta, ct));

    [HttpGet("informes/contables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInformeContable([FromQuery] long ejercicioId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, [FromQuery] long? sucursalId, CancellationToken ct)
        => Ok(await reportesService.GetInformeContableAsync(ejercicioId, desde, hasta, sucursalId, ct));

    [HttpGet("informes/operativos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInformeOperativo([FromQuery] long sucursalId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, [FromQuery] long? depositoId, CancellationToken ct)
        => Ok(await reportesService.GetInformeOperativoAsync(sucursalId, desde, hasta, depositoId, ct));

    [HttpGet("parametrizados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParametrizado([FromQuery] TipoReporteParametrizado tipo, [FromQuery] long? sucursalId, [FromQuery] long? ejercicioId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, [FromQuery] long? depositoId, CancellationToken ct)
        => Ok(await reportesService.GetReporteParametrizadoAsync(tipo, sucursalId, ejercicioId, desde, hasta, depositoId, ct));

    [HttpGet("dashboards/comercial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardComercial([FromQuery] long sucursalId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken ct)
        => Ok(await reportesService.GetDashboardComercialAsync(sucursalId, desde, hasta, ct));

    [HttpGet("dashboards/operativo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardOperativo([FromQuery] long sucursalId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken ct)
        => Ok(await reportesService.GetDashboardOperativoAsync(sucursalId, desde, hasta, ct));

    [HttpPost("exportar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Exportar([FromBody] ExportarReporteCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return File(result.Contenido, result.ContentType, result.NombreArchivo);
    }
}
