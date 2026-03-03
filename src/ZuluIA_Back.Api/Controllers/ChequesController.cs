using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.Commands;
using ZuluIA_Back.Application.Features.Cheques.Queries;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class ChequesController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna cheques paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? cajaId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        EstadoCheque? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoCheque>(estado, true, out var parsedEstado))
            estadoEnum = parsedEstado;

        var result = await Mediator.Send(
            new GetChequesPagedQuery(page, pageSize, cajaId, terceroId,
                                    estadoEnum, desde, hasta), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna los cheques en cartera de una caja.
    /// </summary>
    [HttpGet("cartera/{cajaId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCartera(long cajaId, CancellationToken ct)
    {
        var cheques = await db.Cheques
            .AsNoTracking()
            .Where(x => x.CajaId == cajaId &&
                        x.Estado == EstadoCheque.Cartera)
            .OrderBy(x => x.FechaVencimiento)
            .Select(x => new
            {
                x.Id,
                x.NroCheque,
                x.Banco,
                x.Importe,
                x.MonedaId,
                x.FechaEmision,
                x.FechaVencimiento,
                x.TerceroId,
                Estado = x.Estado.ToString()
            })
            .ToListAsync(ct);

        return Ok(cheques);
    }

    /// <summary>
    /// Registra un nuevo cheque.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateChequeCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deposita un cheque en cartera.
    /// </summary>
    [HttpPost("{id:long}/depositar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Depositar(
        long id,
        [FromBody] DepositarChequeRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CambiarEstadoChequeCommand(
                id,
                AccionCheque.Depositar,
                request.FechaDeposito,
                request.FechaAcreditacion,
                null),
            ct);
        return FromResult(result);
    }

    /// <summary>
    /// Acredita un cheque depositado.
    /// </summary>
    [HttpPost("{id:long}/acreditar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Acreditar(
        long id,
        [FromBody] AcreditarChequeRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CambiarEstadoChequeCommand(
                id,
                AccionCheque.Acreditar,
                request.FechaAcreditacion,
                null,
                null),
            ct);
        return FromResult(result);
    }

    /// <summary>
    /// Rechaza un cheque.
    /// </summary>
    [HttpPost("{id:long}/rechazar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rechazar(
        long id,
        [FromBody] RechazarChequeRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CambiarEstadoChequeCommand(
                id,
                AccionCheque.Rechazar,
                null,
                null,
                request.Observacion),
            ct);
        return FromResult(result);
    }

    /// <summary>
    /// Entrega un cheque en cartera a un tercero.
    /// </summary>
    [HttpPost("{id:long}/entregar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Entregar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CambiarEstadoChequeCommand(id, AccionCheque.Entregar, null, null, null),
            ct);
        return FromResult(result);
    }
}

// ── Request bodies ───────────────────────────────────────────────────────────
public record DepositarChequeRequest(DateOnly FechaDeposito, DateOnly? FechaAcreditacion);
public record AcreditarChequeRequest(DateOnly FechaAcreditacion);
public record RechazarChequeRequest(string? Observacion);