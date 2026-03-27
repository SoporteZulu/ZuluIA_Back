using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.Commands;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Application.Features.Cheques.Queries;
using ZuluIA_Back.Domain.Enums;

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
        [FromQuery] string? banco = null,
        [FromQuery] string? nroCheque = null,
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
                                    estadoEnum, banco, nroCheque, desde, hasta), ct);
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
                request.Fecha,
                null,
                request.Observacion,
                request.TerceroId),
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
    public async Task<IActionResult> Entregar(long id, [FromBody] EntregarChequeRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CambiarEstadoChequeCommand(id, AccionCheque.Entregar, request.FechaEntrega, null, request.Observacion, request.TerceroId),
            ct);
        return FromResult(result);
    }

    [HttpGet("{id:long}/historial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorial(long id, CancellationToken ct)
    {
        var historial = await db.ChequesHistorial
            .AsNoTracking()
            .Where(x => x.ChequeId == id)
            .OrderByDescending(x => x.FechaOperacion)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        var cajaIds = historial.Select(x => x.CajaId).Distinct().ToList();
        var terceroIds = historial.Where(x => x.TerceroId.HasValue).Select(x => x.TerceroId!.Value).Distinct().ToList();

        var cajas = await db.CajasCuentasBancarias.AsNoTracking()
            .Where(x => cajaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = historial.Select(x => new ChequeHistorialDto
        {
            Id = x.Id,
            ChequeId = x.ChequeId,
            CajaId = x.CajaId,
            CajaDescripcion = cajas.GetValueOrDefault(x.CajaId)?.Descripcion ?? "—",
            TerceroId = x.TerceroId,
            TerceroRazonSocial = x.TerceroId.HasValue ? terceros.GetValueOrDefault(x.TerceroId.Value)?.RazonSocial : null,
            Operacion = x.Operacion.ToString().ToUpperInvariant(),
            EstadoAnterior = x.EstadoAnterior?.ToString().ToUpperInvariant(),
            EstadoNuevo = x.EstadoNuevo.ToString().ToUpperInvariant(),
            FechaOperacion = x.FechaOperacion,
            FechaAcreditacion = x.FechaAcreditacion,
            Observacion = x.Observacion,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy
        });

        return Ok(dtos);
    }

    [HttpGet("auditoria")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditoria(
        [FromQuery] long? cajaId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] string? operacion = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        TipoOperacionCheque? operacionEnum = null;
        if (!string.IsNullOrWhiteSpace(operacion) && Enum.TryParse<TipoOperacionCheque>(operacion, true, out var parsedOperacion))
            operacionEnum = parsedOperacion;

        EstadoCheque? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoCheque>(estado, true, out var parsedEstado))
            estadoEnum = parsedEstado;

        var query = db.ChequesHistorial.AsNoTracking();

        if (cajaId.HasValue)
            query = query.Where(x => x.CajaId == cajaId.Value);
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (operacionEnum.HasValue)
            query = query.Where(x => x.Operacion == operacionEnum.Value);
        if (estadoEnum.HasValue)
            query = query.Where(x => x.EstadoNuevo == estadoEnum.Value);
        if (desde.HasValue)
            query = query.Where(x => x.FechaOperacion >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.FechaOperacion <= hasta.Value);

        var historial = await query.OrderByDescending(x => x.FechaOperacion).ThenByDescending(x => x.Id).ToListAsync(ct);
        var chequeIds = historial.Select(x => x.ChequeId).Distinct().ToList();
        var cheques = await db.Cheques.AsNoTracking()
            .Where(x => chequeIds.Contains(x.Id))
            .Select(x => new { x.Id, x.NroCheque, x.Banco, x.Importe, x.Estado })
            .ToDictionaryAsync(x => x.Id, ct);

        return Ok(historial.Select(x => new
        {
            x.Id,
            x.ChequeId,
            NroCheque = cheques.GetValueOrDefault(x.ChequeId)?.NroCheque ?? "—",
            Banco = cheques.GetValueOrDefault(x.ChequeId)?.Banco ?? "—",
            Importe = cheques.GetValueOrDefault(x.ChequeId)?.Importe ?? 0m,
            EstadoActual = cheques.GetValueOrDefault(x.ChequeId)?.Estado.ToString().ToUpperInvariant() ?? "—",
            Operacion = x.Operacion.ToString().ToUpperInvariant(),
            EstadoAnterior = x.EstadoAnterior?.ToString().ToUpperInvariant(),
            EstadoNuevo = x.EstadoNuevo.ToString().ToUpperInvariant(),
            x.FechaOperacion,
            x.FechaAcreditacion,
            x.CajaId,
            x.TerceroId,
            x.Observacion,
            x.CreatedAt,
            x.CreatedBy
        }));
    }
}

// ── Request bodies ───────────────────────────────────────────────────────────
public record DepositarChequeRequest(DateOnly FechaDeposito, DateOnly? FechaAcreditacion);
public record AcreditarChequeRequest(DateOnly FechaAcreditacion);
public record RechazarChequeRequest(DateOnly? Fecha, string? Observacion, long? TerceroId = null);
public record EntregarChequeRequest(DateOnly? FechaEntrega, long? TerceroId, string? Observacion);
