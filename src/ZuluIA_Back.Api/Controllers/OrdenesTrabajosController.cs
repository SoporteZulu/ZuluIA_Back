using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Produccion.Commands;
using ZuluIA_Back.Application.Features.Produccion.Queries;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class OrdenesTrabajosController(
    IMediator mediator,
    IOrdenTrabajoRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna órdenes de trabajo paginadas con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? formulaId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        EstadoOrdenTrabajo? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoOrdenTrabajo>(estado, true, out var parsed))
            estadoEnum = parsed;

        var result = await Mediator.Send(
            new GetOrdenesTrabajoPagedQuery(
                page, pageSize,
                sucursalId, formulaId,
                estadoEnum, desde, hasta),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle de una orden de trabajo.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetOrdenTrabajoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var ot = await repo.GetByIdAsync(id, ct);
        return OkOrNotFound(ot is null ? null : new
        {
            ot.Id,
            ot.SucursalId,
            ot.FormulaId,
            ot.DepositoOrigenId,
            ot.DepositoDestinoId,
            ot.Fecha,
            ot.FechaFinPrevista,
            ot.FechaFinReal,
            ot.Cantidad,
            Estado = ot.Estado.ToString().ToUpperInvariant(),
            ot.Observacion,
            ot.CreatedAt
        });
    }

    /// <summary>
    /// Crea una nueva orden de trabajo en estado Pendiente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CrearOrdenTrabajoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetOrdenTrabajoById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Inicia una orden de trabajo (Pendiente → EnProceso).
    /// </summary>
    [HttpPost("{id:long}/iniciar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Iniciar(long id, CancellationToken ct)
    {
        var ot = await repo.GetByIdAsync(id, ct);
        if (ot is null)
            return NotFound(new { error = $"No se encontró la OT con ID {id}." });

        ot.Iniciar(null);
        repo.Update(ot);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Orden de trabajo iniciada correctamente." });
    }

    /// <summary>
    /// Finaliza una orden de trabajo: consume ingredientes e ingresa
    /// el producto terminado al depósito destino.
    /// </summary>
    [HttpPost("{id:long}/finalizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalizar(
        long id,
        [FromBody] FinalizarOtRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new FinalizarOrdenTrabajoCommand(id, request.FechaFinReal), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Cancela una orden de trabajo.
    /// </summary>
    [HttpPost("{id:long}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(long id, CancellationToken ct)
    {
        var ot = await repo.GetByIdAsync(id, ct);
        if (ot is null)
            return NotFound(new { error = $"No se encontró la OT con ID {id}." });

        ot.Cancelar(null);
        repo.Update(ot);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Orden de trabajo cancelada correctamente." });
    }

    /// <summary>
    /// Retorna los estados de OT disponibles.
    /// </summary>
    [HttpGet("estados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEstados() =>
        Ok(Enum.GetValues<EstadoOrdenTrabajo>()
            .Select(e => new
            {
                valor = e.ToString().ToUpperInvariant(),
                descripcion = e switch
                {
                    EstadoOrdenTrabajo.Pendiente => "Pendiente",
                    EstadoOrdenTrabajo.EnProceso => "En Proceso",
                    EstadoOrdenTrabajo.Finalizada => "Finalizada",
                    EstadoOrdenTrabajo.Cancelada => "Cancelada",
                    _ => e.ToString()
                }
            }));
}

public record FinalizarOtRequest(DateOnly FechaFinReal);