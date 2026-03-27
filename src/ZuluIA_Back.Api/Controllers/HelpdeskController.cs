using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Documentos.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Módulo de gestión de tickets de soporte (Helpdesk).
/// Implementado sobre Mesa de Entrada para registrar y seguir solicitudes.
/// </summary>
[Route("api/helpdesk")]
public class HelpdeskController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTickets(
        [FromQuery] long? sucursalId,
        [FromQuery] long? asignadoA,
        [FromQuery] string? estadoFlow,
        [FromQuery] bool incluirArchivados = false,
        CancellationToken ct = default)
    {
        var query = db.MesasEntrada.AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (asignadoA.HasValue)
            query = query.Where(x => x.AsignadoA == asignadoA.Value);

        if (!string.IsNullOrWhiteSpace(estadoFlow) &&
            Enum.TryParse<EstadoMesaEntrada>(estadoFlow, true, out var flow))
        {
            query = query.Where(x => x.EstadoFlow == flow);
        }

        if (!incluirArchivados)
            query = query.Where(x => x.EstadoFlow != EstadoMesaEntrada.Archivado && x.EstadoFlow != EstadoMesaEntrada.Anulado);

        var result = await query
            .OrderByDescending(x => x.FechaIngreso)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                TicketId = x.Id,
                x.SucursalId,
                x.TipoId,
                x.TerceroId,
                x.NroDocumento,
                Titulo = x.Asunto,
                x.Observacion,
                x.AsignadoA,
                x.EstadoId,
                EstadoFlow = x.EstadoFlow.ToString(),
                x.FechaIngreso,
                x.FechaVencimiento
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var ticket = await db.MesasEntrada.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                TicketId = x.Id,
                x.SucursalId,
                x.TipoId,
                x.TerceroId,
                x.NroDocumento,
                Titulo = x.Asunto,
                x.Observacion,
                x.AsignadoA,
                x.EstadoId,
                EstadoFlow = x.EstadoFlow.ToString(),
                x.FechaIngreso,
                x.FechaVencimiento,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(ticket);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTicket([FromBody] HelpdeskCreateTicketRequest req, CancellationToken ct)
    {
        var command = new CreateHelpdeskTicketCommand(
            req.SucursalId,
            req.TipoId,
            req.TerceroId,
            req.NroDocumento,
            req.Titulo,
            req.FechaIngreso,
            req.FechaVencimiento,
            req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { ticketId = result.Value });
    }

    [HttpPatch("{id:long}/asignar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Asignar(long id, [FromBody] HelpdeskAsignarRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AssignHelpdeskTicketCommand(id, req.UsuarioId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { ticketId = result.Value.TicketId, result.Value.AsignadoA });
    }

    [HttpPatch("{id:long}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarEstado(long id, [FromBody] HelpdeskEstadoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ChangeHelpdeskTicketStateCommand(id, req.EstadoId, req.EstadoFlow, req.Observacion),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("EstadoFlow inválido", StringComparison.OrdinalIgnoreCase) == true
                ? BadRequest(new { error = result.Error })
                : NotFound(new { error = result.Error });

        return Ok(new
        {
            ticketId = result.Value.TicketId,
            result.Value.EstadoFlow,
            EstadoId = result.Value.EstadoId
        });
    }

    [HttpPatch("{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cerrar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CloseHelpdeskTicketCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record HelpdeskCreateTicketRequest(
    long SucursalId,
    long TipoId,
    long? TerceroId,
    string NroDocumento,
    string Titulo,
    DateOnly FechaIngreso,
    DateOnly? FechaVencimiento,
    string? Observacion);

public record HelpdeskAsignarRequest(long UsuarioId);

public record HelpdeskEstadoRequest(long EstadoId, string EstadoFlow, string? Observacion);
