using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de presupuestos / cotizaciones comerciales.
/// Mapea a la tabla presupuestos + presupuestos_items.
/// </summary>
public class PresupuestosController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // ── Listado ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Listado paginado de presupuestos con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var q = db.Presupuestos
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (sucursalId.HasValue) q = q.Where(x => x.SucursalId == sucursalId);
        if (terceroId.HasValue)  q = q.Where(x => x.TerceroId  == terceroId);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(x => x.Estado == estado);
        if (desde.HasValue) q = q.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(x => x.Fecha <= hasta.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TerceroId,
                x.Fecha,
                x.FechaVigencia,
                x.MonedaId,
                x.Cotizacion,
                x.Total,
                x.Estado,
                x.Observacion,
                x.ComprobanteId,
                x.CreatedAt,
            })
            .ToListAsync(ct);

        return Ok(new
        {
            items,
            page,
            pageSize,
            totalCount = total,
            totalPages  = (int)Math.Ceiling((double)total / pageSize),
        });
    }

    // ── Detalle ────────────────────────────────────────────────────────────────

    /// <summary>Retorna el detalle de un presupuesto con sus líneas.</summary>
    [HttpGet("{id:long}", Name = "GetPresupuestoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var p = await db.Presupuestos
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TerceroId,
                x.Fecha,
                x.FechaVigencia,
                x.MonedaId,
                x.Cotizacion,
                x.Total,
                x.Estado,
                x.Observacion,
                x.ComprobanteId,
                x.CreatedAt,
                x.UpdatedAt,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ItemId,
                    i.Descripcion,
                    i.Cantidad,
                    i.PrecioUnitario,
                    i.DescuentoPct,
                    i.Subtotal,
                    i.Orden,
                }).OrderBy(i => i.Orden),
            })
            .FirstOrDefaultAsync(ct);

        return p is null ? NotFound(new { error = $"Presupuesto {id} no encontrado." }) : Ok(p);
    }

    /// <summary>Retorna un payload dedicado para reimpresion del presupuesto.</summary>
    [HttpGet("{id:long}/reimpresion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReimpresion(long id, CancellationToken ct)
    {
        var presupuesto = await db.Presupuestos
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TerceroId,
                x.Fecha,
                x.FechaVigencia,
                x.MonedaId,
                x.Cotizacion,
                x.Total,
                x.Estado,
                x.Observacion,
                x.ComprobanteId,
                x.CreatedAt,
                x.UpdatedAt,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ItemId,
                    i.Descripcion,
                    i.Cantidad,
                    i.PrecioUnitario,
                    i.DescuentoPct,
                    i.Subtotal,
                    i.Orden,
                }).OrderBy(i => i.Orden),
            })
            .FirstOrDefaultAsync(ct);

        return presupuesto is null
            ? NotFound(new { error = $"Presupuesto {id} no encontrado." })
            : Ok(new PresupuestoReimpresionResponse(true, DateTimeOffset.UtcNow, presupuesto));
    }

    // ── Crear ──────────────────────────────────────────────────────────────────

    /// <summary>Crea un nuevo presupuesto.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePresupuestoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreatePresupuestoCommand(
                req.SucursalId,
                req.TerceroId,
                req.Fecha,
                req.FechaVigencia,
                req.MonedaId,
                req.Cotizacion,
                req.Observacion,
                GetCurrentUserId(),
                req.Items?.Select(x => new CreatePresupuestoItemInput(
                    x.ItemId,
                    x.Descripcion,
                    x.Cantidad,
                    x.PrecioUnitario,
                    x.DescuentoPct)).ToList()),
            ct);

        return result.IsSuccess
            ? CreatedAtRoute("GetPresupuestoById", new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    // ── Aprobar / Rechazar ─────────────────────────────────────────────────────

    [HttpPost("{id:long}/aprobar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Aprobar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AprobarPresupuestoCommand(id, GetCurrentUserId()), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(new { mensaje = "Presupuesto aprobado." });
    }

    [HttpPost("{id:long}/rechazar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rechazar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new RechazarPresupuestoCommand(id, GetCurrentUserId()), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(new { mensaje = "Presupuesto rechazado." });
    }

    // ── Eliminar ───────────────────────────────────────────────────────────────

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeletePresupuestoCommand(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return NoContent();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private long? GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("userId");
        return claim != null && long.TryParse(claim.Value, out var id) ? id : null;
    }
}

public record CreatePresupuestoItemRequest(
    long ItemId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal DescuentoPct);

public record CreatePresupuestoRequest(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    DateOnly? FechaVigencia,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    List<CreatePresupuestoItemRequest>? Items);

public record PresupuestoReimpresionResponse(
    bool EsReimpresion,
    DateTimeOffset GeneradoEn,
    object Documento);
