using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class AsientosController(
    IMediator mediator,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna asientos paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long ejercicioId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        EstadoAsiento? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoAsiento>(estado, true, out var parsed))
            estadoEnum = parsed;

        var result = await Mediator.Send(
            new GetAsientosPagedQuery(
                page, pageSize,
                ejercicioId, sucursalId,
                estadoEnum, desde, hasta),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de un asiento con sus líneas.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetAsientoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAsientoDetalleQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna los asientos asociados a un origen (tabla + id).
    /// Ej: origen_tabla=comprobantes, origen_id=123
    /// </summary>
    [HttpGet("por-origen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByOrigen(
        [FromQuery] string origenTabla,
        [FromQuery] long origenId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(origenTabla))
            return BadRequest(new { error = "El parámetro origenTabla es obligatorio." });

        var asientos = await db.Asientos
            .AsNoTracking()
            .Where(x =>
                x.OrigenTabla == origenTabla &&
                x.OrigenId    == origenId)
            .OrderByDescending(x => x.Fecha)
            .Select(x => new
            {
                x.Id,
                x.EjercicioId,
                x.SucursalId,
                x.Fecha,
                x.Numero,
                x.Descripcion,
                Estado = x.Estado.ToString().ToUpperInvariant()
            })
            .ToListAsync(ct);

        return Ok(asientos);
    }

    /// <summary>
    /// Retorna el libro diario: asientos confirmados en un período.
    /// </summary>
    [HttpGet("libro-diario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLibroDiario(
        [FromQuery] long ejercicioId,
        [FromQuery] long sucursalId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        CancellationToken ct)
    {
        var asientos = await db.Asientos
            .AsNoTracking()
            .Where(x =>
                x.EjercicioId == ejercicioId &&
                x.SucursalId  == sucursalId  &&
                x.Fecha       >= desde        &&
                x.Fecha       <= hasta        &&
                x.Estado      == EstadoAsiento.Confirmado)
            .Include(x => x.Lineas)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Numero)
            .ToListAsync(ct);

        var cuentaIds = asientos
            .SelectMany(a => a.Lineas.Select(l => l.CuentaId))
            .Distinct().ToList();

        var cuentas = await db.PlanCuentas.AsNoTracking()
            .Where(x => cuentaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CodigoCuenta, x.Denominacion })
            .ToDictionaryAsync(x => x.Id, ct);

        var resultado = asientos.Select(a => new
        {
            a.Id,
            a.Fecha,
            a.Numero,
            a.Descripcion,
            TotalDebe = a.TotalDebe,
            TotalHaber = a.TotalHaber,
            Lineas = a.Lineas.OrderBy(l => l.Orden).Select(l => new
            {
                l.Id,
                l.CuentaId,
                CuentaCodigo = cuentas.GetValueOrDefault(l.CuentaId)?.CodigoCuenta  ?? "—",
                CuentaDenominacion = cuentas.GetValueOrDefault(l.CuentaId)?.Denominacion  ?? "—",
                l.Debe,
                l.Haber,
                l.Descripcion,
                l.Orden
            })
        });

        return Ok(new
        {
            ejercicioId,
            sucursalId,
            desde,
            hasta,
            totalAsientos = asientos.Count,
            totalDebe = asientos.Sum(a => a.TotalDebe),
            totalHaber = asientos.Sum(a => a.TotalHaber),
            asientos = resultado
        });
    }

    /// <summary>
    /// Retorna el balance de sumas y saldos para un período.
    /// </summary>
    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance(
        [FromQuery] long ejercicioId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetBalanceSumasYSaldosQuery(
                ejercicioId, sucursalId, desde, hasta),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Registra un nuevo asiento contable manual.
    /// Valida que el ejercicio esté abierto y que el asiento cuadre.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarAsientoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetAsientoById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Anula un asiento contable.
    /// </summary>
    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularAsientoCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna los estados de asiento disponibles.
    /// </summary>
    [HttpGet("estados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEstados() =>
        Ok(Enum.GetValues<EstadoAsiento>()
            .Select(e => new
            {
                valor = e.ToString().ToUpperInvariant(),
                descripcion = e switch
                {
                    EstadoAsiento.Borrador => "Borrador",
                    EstadoAsiento.Confirmado => "Confirmado",
                    EstadoAsiento.Anulado => "Anulado",
                    _ => e.ToString()
                }
            }));
}