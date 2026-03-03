using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class PeriodosIvaController(IMediator mediator, IPeriodoIvaRepository repo)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna los períodos IVA de una sucursal.
    /// Opcionalmente filtra por ejercicio.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySucursal(
        [FromQuery] long sucursalId,
        [FromQuery] long? ejercicioId,
        CancellationToken ct)
    {
        var periodos = await repo.GetBySucursalAsync(sucursalId, ejercicioId, ct);
        var dtos = periodos.Select(p => new PeriodoIvaDto
        {
            Id                 = p.Id,
            EjercicioId        = p.EjercicioId,
            SucursalId         = p.SucursalId,
            Periodo            = p.Periodo,
            PeriodoDescripcion = p.PeriodoDescripcion,
            Cerrado            = p.Cerrado,
            CreatedAt          = p.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Verifica si una fecha cae en un período IVA abierto.
    /// </summary>
    [HttpGet("estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstado(
        [FromQuery] long sucursalId,
        [FromQuery] DateOnly fecha,
        CancellationToken ct)
    {
        var abierto = await repo.EstaAbiertoPeriodoAsync(sucursalId, fecha, ct);
        return Ok(new
        {
            sucursalId,
            fecha,
            periodoAbierto = abierto,
            mensaje = abierto
                ? "El período está abierto. Se pueden emitir comprobantes."
                : "El período está cerrado. No se pueden emitir comprobantes."
        });
    }

    /// <summary>
    /// Abre (o reabre) un período IVA para una sucursal.
    /// </summary>
    [HttpPost("abrir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Abrir(
        [FromBody] AbrirPeriodoIvaCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value, mensaje = "Período IVA abierto correctamente." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Cierra un período IVA. Una vez cerrado no se podrán emitir
    /// comprobantes con fecha en ese período.
    /// </summary>
    [HttpPost("cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cerrar(
        [FromBody] CerrarPeriodoIvaCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }
}