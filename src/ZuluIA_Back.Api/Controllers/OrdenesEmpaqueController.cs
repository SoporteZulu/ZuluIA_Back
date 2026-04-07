using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Logistica.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Órdenes de Empaque para exportaciones y envíos especiales.
/// Migrado desde VB6: frmOrdenEmpaque.
/// </summary>
public class OrdenesEmpaqueController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/ordenes-empaque
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? terceroId,
        [FromQuery] string? estado,
        [FromQuery] bool incluirAnuladas = false,
        CancellationToken ct = default)
    {
        var query = db.OrdenesEmpaquesLogistica.AsNoTracking();
        if (!incluirAnuladas) query = query.Where(x => !x.Anulada);
        if (terceroId.HasValue) query = query.Where(x => x.TerceroId == terceroId.Value);
        if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(x => x.Estado == estado.ToUpperInvariant());

        var result = await query
            .OrderByDescending(x => x.Fecha)
            .Select(x => new
            {
                x.Id, x.TerceroId, x.VendedorId, x.Fecha,
                x.FechaEmbarque, x.Estado, x.Anulada, x.Total, x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET api/ordenes-empaque/{id}
    [HttpGet("{id:long}", Name = "GetOrdenEmpaqueById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var orden = await db.OrdenesEmpaquesLogistica.AsNoTracking()
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (orden is null) return NotFound(new { error = $"Orden de empaque {id} no encontrada." });

        return Ok(new
        {
            orden.Id, orden.TerceroId, orden.SucursalTerceroId, orden.VendedorId,
            orden.DepositoId, orden.TransportistaId, orden.AgenteId,
            orden.TipoComprobanteId, orden.PuntoFacturacionId,
            orden.MonedaId, orden.Cotizacion, orden.Prefijo, orden.NroComprobante,
            orden.Fecha, orden.FechaEmbarque, orden.FechaVencimiento,
            orden.OrigenObservacion, orden.DestinoObservacion,
            orden.Total, orden.Estado, orden.Anulada, orden.Observacion,
            orden.CreatedAt, orden.UpdatedAt,
            Detalles = orden.Detalles.Select(d => new
            {
                d.Id, d.ItemId, d.Descripcion, d.Cantidad,
                d.PrecioUnitario, d.PorcentajeIva, d.Total, d.Observacion
            })
        });
    }

    // POST api/ordenes-empaque
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CrearOrdenEmpaqueRequest req, CancellationToken ct)
    {
        var command = new CreateOrdenEmpaqueCommand(
            req.TerceroId,
            req.SucursalTerceroId,
            req.VendedorId,
            req.DepositoId,
            req.TransportistaId,
            req.AgenteId,
            req.TipoComprobanteId,
            req.PuntoFacturacionId,
            req.MonedaId,
            req.Cotizacion,
            req.Fecha,
            req.FechaEmbarque,
            req.FechaVencimiento,
            req.OrigenObservacion,
            req.DestinoObservacion,
            req.Total,
            req.Observacion,
            req.Detalles
                .Select(d => new CreateOrdenEmpaqueDetalleInput(
                    d.ItemId,
                    d.Descripcion,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.PorcentajeIva,
                    d.Observacion))
                .ToList());

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetOrdenEmpaqueById", new { id = result.Value }, new { Id = result.Value });
    }

    // POST api/ordenes-empaque/{id}/confirmar
    [HttpPost("{id:long}/confirmar")]
    public async Task<IActionResult> Confirmar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ConfirmOrdenEmpaqueCommand(id), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound()
                : BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value.Id, estado = result.Value.Estado });
    }

    // POST api/ordenes-empaque/{id}/anular
    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CancelOrdenEmpaqueCommand(id), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound()
                : BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value.Id, estado = result.Value.Estado });
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record CrearOrdenEmpaqueRequest(
    long TerceroId,
    long? SucursalTerceroId,
    long? VendedorId,
    long? DepositoId,
    long? TransportistaId,
    long? AgenteId,
    long? TipoComprobanteId,
    long? PuntoFacturacionId,
    int? MonedaId,
    decimal Cotizacion,
    DateOnly Fecha,
    DateOnly? FechaEmbarque,
    DateOnly? FechaVencimiento,
    string? OrigenObservacion,
    string? DestinoObservacion,
    decimal Total,
    string? Observacion,
    IReadOnlyList<OrdenEmpaqueDetalleRequest> Detalles);

public record OrdenEmpaqueDetalleRequest(
    long? ItemId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PorcentajeIva,
    string? Observacion);
