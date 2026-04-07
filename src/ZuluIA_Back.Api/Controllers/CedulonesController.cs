using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class CedulonesController(
    IMediator mediator,
    ICedulonRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna cedulones paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? terceroId = null,
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estado = null,
        CancellationToken ct = default)
    {
        EstadoCedulon? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoCedulon>(estado, true, out var parsed))
            estadoEnum = parsed;

        var result = await repo.GetPagedAsync(
            page, pageSize, terceroId, sucursalId, estadoEnum, ct);

        var terceroIds = result.Items.Select(x => x.TerceroId).Distinct().ToList();
        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var dtos = result.Items.Select(c => new CedulonDto
        {
            Id                 = c.Id,
            TerceroId          = c.TerceroId,
            TerceroRazonSocial = terceros.GetValueOrDefault(c.TerceroId)?.RazonSocial ?? "—",
            SucursalId         = c.SucursalId,
            PlanPagoId         = c.PlanPagoId,
            NroCedulon         = c.NroCedulon,
            FechaEmision       = c.FechaEmision,
            FechaVencimiento   = c.FechaVencimiento,
            Importe            = c.Importe,
            ImportePagado      = c.ImportePagado,
            SaldoPendiente     = c.Importe - c.ImportePagado,
            Estado             = c.Estado.ToString().ToUpperInvariant(),
            Vencido            = c.Estado != EstadoCedulon.Pagado &&
                                  c.FechaVencimiento < hoy
        }).ToList();

        return Ok(new
        {
            data = dtos,
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    /// <summary>
    /// Retorna el detalle de un cedulón por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetCedulonById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var cedulon = await repo.GetByIdAsync(id, ct);
        if (cedulon is null)
            return NotFound(new { error = $"No se encontró el cedulón con ID {id}." });

        var tercero = await db.Terceros.AsNoTracking()
            .Where(x => x.Id == cedulon.TerceroId)
            .Select(x => new { x.RazonSocial })
            .FirstOrDefaultAsync(ct);

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return Ok(new CedulonDto
        {
            Id                 = cedulon.Id,
            TerceroId          = cedulon.TerceroId,
            TerceroRazonSocial = tercero?.RazonSocial ?? "—",
            SucursalId         = cedulon.SucursalId,
            PlanPagoId         = cedulon.PlanPagoId,
            NroCedulon         = cedulon.NroCedulon,
            FechaEmision       = cedulon.FechaEmision,
            FechaVencimiento   = cedulon.FechaVencimiento,
            Importe            = cedulon.Importe,
            ImportePagado      = cedulon.ImportePagado,
            SaldoPendiente     = cedulon.Importe - cedulon.ImportePagado,
            Estado             = cedulon.Estado.ToString().ToUpperInvariant(),
            Vencido            = cedulon.Estado != EstadoCedulon.Pagado &&
                                  cedulon.FechaVencimiento < hoy
        });
    }

    /// <summary>
    /// Crea un nuevo cedulón.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCedulonRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateCedulonCommand(
                request.TerceroId,
                request.SucursalId,
                request.PlanPagoId,
                request.NroCedulon,
                request.FechaEmision,
                request.FechaVencimiento,
                request.Importe),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetCedulonById",
            new { id = result.Value },
            new { id = result.Value });
    }

    /// <summary>
    /// Registra un pago parcial o total sobre un cedulón.
    /// </summary>
    [HttpPost("{id:long}/pagar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pagar(
        long id,
        [FromBody] PagarCedulonRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new PagarCedulonCommand(id, request.Importe), ct);
        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new
        {
            mensaje = "Pago registrado correctamente.",
            importePagado = result.Value.ImportePagado,
            saldoPendiente = result.Value.SaldoPendiente,
            estado = result.Value.Estado
        });
    }

    /// <summary>
    /// Marca un cedulón como vencido.
    /// </summary>
    [HttpPost("{id:long}/vencer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Vencer(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new VencerCedulonCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new
        {
            mensaje = "Cedulón marcado como vencido correctamente.",
            estado = result.Value
        });
    }

    /// <summary>
    /// Retorna cedulones vencidos pendientes de pago.
    /// </summary>
    [HttpGet("vencidos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVencidos(
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var query = db.Cedulones
            .AsNoTracking()
            .Where(x => x.FechaVencimiento < hoy &&
                        x.Estado != EstadoCedulon.Pagado);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var vencidos = await query
            .OrderBy(x => x.FechaVencimiento)
            .Select(x => new
            {
                x.Id,
                x.NroCedulon,
                x.TerceroId,
                x.FechaVencimiento,
                x.Importe,
                SaldoPendiente = x.Importe - x.ImportePagado,
                Estado = x.Estado.ToString().ToUpperInvariant(),
                DiasVencido = hoy.DayNumber - x.FechaVencimiento.DayNumber
            })
            .ToListAsync(ct);

        return Ok(new
        {
            totalVencidos = vencidos.Count,
            totalImporte = vencidos.Sum(x => x.SaldoPendiente),
            cedulones = vencidos
        });
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────
public record CreateCedulonRequest(
    long TerceroId,
    long SucursalId,
    long? PlanPagoId,
    string NroCedulon,
    DateOnly FechaEmision,
    DateOnly FechaVencimiento,
    decimal Importe);

public record PagarCedulonRequest(decimal Importe);