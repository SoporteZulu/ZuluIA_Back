using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class EjerciciosController(
    IMediator mediator,
    IEjercicioRepository repo)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todos los ejercicios contables ordenados por fecha descendente.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetEjerciciosQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el ejercicio vigente para una fecha dada.
    /// </summary>
    [HttpGet("vigente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVigente(
        [FromQuery] DateOnly? fecha = null,
        CancellationToken ct = default)
    {
        var fechaConsulta = fecha ?? DateOnly.FromDateTime(DateTime.Today);
        var ejercicio = await repo.GetVigenteAsync(fechaConsulta, ct);
        return OkOrNotFound(ejercicio is null ? null : new
        {
            ejercicio.Id,
            ejercicio.Descripcion,
            ejercicio.FechaInicio,
            ejercicio.FechaFin,
            ejercicio.MascaraCuentaContable,
            ejercicio.Cerrado
        });
    }

    /// <summary>
    /// Retorna el detalle de un ejercicio con sus sucursales asignadas.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetEjercicioById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var ejercicio = await repo.GetByIdConSucursalesAsync(id, ct);
        return OkOrNotFound(ejercicio is null ? null : new
        {
            ejercicio.Id,
            ejercicio.Descripcion,
            ejercicio.FechaInicio,
            ejercicio.FechaFin,
            ejercicio.MascaraCuentaContable,
            ejercicio.Cerrado,
            ejercicio.CreatedAt,
            Sucursales = ejercicio.Sucursales.Select(s => new
            {
                s.Id,
                s.SucursalId,
                s.UsaContabilidad
            })
        });
    }

    /// <summary>
    /// Crea un nuevo ejercicio contable con sucursales asignadas.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEjercicioCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetEjercicioById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Cierra un ejercicio contable. No se podrán registrar más asientos.
    /// </summary>
    [HttpPost("{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cerrar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CerrarEjercicioCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Ejercicio cerrado correctamente." });
    }

    /// <summary>
    /// Reabre un ejercicio contable cerrado.
    /// </summary>
    [HttpPost("{id:long}/reabrir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reabrir(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ReabrirEjercicioCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Ejercicio reabierto correctamente." });
    }

    /// <summary>
    /// Asigna una sucursal adicional a un ejercicio existente.
    /// </summary>
    [HttpPost("{id:long}/sucursales")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarSucursal(
        long id,
        [FromBody] AsignarSucursalEjercicioRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AsignarSucursalEjercicioCommand(id, request.SucursalId, request.UsaContabilidad),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Sucursal asignada al ejercicio correctamente." });
    }
}

public record AsignarSucursalEjercicioRequest(
    long SucursalId,
    bool UsaContabilidad = true);