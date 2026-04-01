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
        [FromQuery] string? tipo = null,
        [FromQuery] bool? esALaOrden = null,
        [FromQuery] bool? esCruzado = null,
        [FromQuery] string? titular = null,
        CancellationToken ct = default)
    {
        EstadoCheque? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoCheque>(estado, true, out var parsedEstado))
            estadoEnum = parsedEstado;

        TipoCheque? tipoEnum = null;
        if (!string.IsNullOrWhiteSpace(tipo) &&
            Enum.TryParse<TipoCheque>(tipo, true, out var parsedTipo))
            tipoEnum = parsedTipo;

        var result = await Mediator.Send(
            new GetChequesPagedQuery(
                page,
                pageSize,
                cajaId,
                terceroId,
                estadoEnum,
                tipoEnum,
                esALaOrden,
                esCruzado,
                banco,
                nroCheque,
                titular,
                desde,
                hasta),
            ct);

        return Ok(result);
    }

    [NonAction]
    public Task<IActionResult> GetAll(
        int page,
        int pageSize,
        long? cajaId,
        long? terceroId,
        string? estado,
        string? tipo,
        bool? esALaOrden,
        bool? esCruzado,
        string? banco,
        string? nroCheque,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct)
    {
        return GetAll(page, pageSize, cajaId, terceroId, estado, banco, nroCheque, desde, hasta, tipo, esALaOrden, esCruzado, null, ct);
    }

    /// <summary>
    /// Retorna el detalle completo de un cheque con historial.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var detalle = await Mediator.Send(new GetChequeDetalleQuery(id), ct);
        return detalle is not null ? Ok(detalle) : NotFound();
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
            .Where(x => x.CajaId == cajaId && x.Estado == EstadoCheque.Cartera)
            .OrderBy(x => x.FechaVencimiento)
            .Select(x => new
            {
                x.Id,
                x.NroCheque,
                x.Banco,
                x.Titular,
                x.Importe,
                x.MonedaId,
                x.FechaEmision,
                x.FechaVencimiento,
                x.TerceroId,
                x.EsALaOrden,
                x.EsCruzado,
                Estado = x.Estado.ToString(),
                Tipo = x.Tipo.ToString()
            })
            .ToListAsync(ct);

        return Ok(cheques);
    }

    /// <summary>
    /// Retorna cheques pendientes de depósito.
    /// </summary>
    [HttpGet("pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendientes(
        [FromQuery] long? cajaId,
        [FromQuery] DateOnly? hastaFechaVencimiento,
        [FromQuery] bool soloVencidos = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetChequesPendientesQuery(cajaId, hastaFechaVencimiento, soloVencidos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna cheques depositados no acreditados.
    /// </summary>
    [HttpGet("depositados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepositados(
        [FromQuery] long? cajaId,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetChequesDepositadosQuery(cajaId, desde, hasta), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el historial completo de un cheque.
    /// </summary>
    [HttpGet("{id:long}/historial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistorial(long id, CancellationToken ct)
    {
        var historial = await Mediator.Send(new GetChequeHistorialQuery(id), ct);
        return Ok(historial);
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
            new CambiarEstadoChequeCommand(id, AccionCheque.Depositar, request.FechaDeposito, request.FechaAcreditacion, null),
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
            new CambiarEstadoChequeCommand(id, AccionCheque.Acreditar, request.FechaAcreditacion, null, null),
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
                request.TerceroId,
                request.ConceptoRechazo),
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
    public async Task<IActionResult> Entregar(
        long id,
        [FromBody] EntregarChequeRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CambiarEstadoChequeCommand(id, AccionCheque.Entregar, request.FechaEntrega, null, request.Observacion, request.TerceroId),
            ct);
        return FromResult(result);
    }

    /// <summary>
    /// Endosa un cheque a otro tercero.
    /// </summary>
    [HttpPost("{id:long}/endosar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Endosar(
        long id,
        [FromBody] EndosarChequeRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CambiarEstadoChequeCommand(id, AccionCheque.Endosar, request.Fecha, null, request.Observacion, request.NuevoTerceroId),
            ct);
        return FromResult(result);
    }

    /// <summary>
    /// Anula un cheque propio.
    /// </summary>
    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(
        long id,
        [FromBody] AnularChequeRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CambiarEstadoChequeCommand(id, AccionCheque.Anular, request.Fecha, null, request.Motivo),
            ct);
        return FromResult(result);
    }

    /// <summary>
    /// Actualiza datos de un cheque en cartera.
    /// </summary>
    [HttpPatch("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        long id,
        [FromBody] ActualizarChequeRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ActualizarChequeCommand(
                id,
                request.Titular,
                request.FechaEmision,
                request.FechaVencimiento,
                request.CodigoSucursalBancaria,
                request.CodigoPostal),
            ct);
        return FromResult(result);
    }
}

public record DepositarChequeRequest(DateOnly FechaDeposito, DateOnly? FechaAcreditacion);
public record AcreditarChequeRequest(DateOnly FechaAcreditacion);
public record RechazarChequeRequest(DateOnly? Fecha, string? ConceptoRechazo, string? Observacion, long? TerceroId)
{
    public RechazarChequeRequest(DateOnly? fecha, string? observacion, long? terceroId)
        : this(fecha, null, observacion, terceroId)
    {
    }
}
public record EntregarChequeRequest(DateOnly? FechaEntrega, long? TerceroId, string? Observacion);
public record EndosarChequeRequest(DateOnly? Fecha, long NuevoTerceroId, string? Observacion)
{
    public EndosarChequeRequest(long nuevoTerceroId, string? observacion)
        : this(null, nuevoTerceroId, observacion)
    {
    }
}
public record AnularChequeRequest(DateOnly? Fecha, string Motivo)
{
    public AnularChequeRequest(string motivo)
        : this(null, motivo)
    {
    }
}
public record ActualizarChequeRequest(
    string? Titular,
    DateOnly? FechaEmision,
    DateOnly? FechaVencimiento,
    string? CodigoSucursalBancaria,
    string? CodigoPostal);
