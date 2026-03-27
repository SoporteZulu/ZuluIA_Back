using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class OrdenesCompraController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna las órdenes de compra con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? proveedorId = null,
        [FromQuery] string? estado = null,
        [FromQuery] bool? habilitada = null,
        CancellationToken ct = default)
    {
        EstadoOrdenCompra? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoOrdenCompra>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.OrdenesCompraMeta.AsNoTracking();

        if (proveedorId.HasValue)
            query = query.Where(x => x.ProveedorId == proveedorId.Value);

        if (estadoEnum.HasValue)
            query = query.Where(x => x.EstadoOc == estadoEnum.Value);

        if (habilitada.HasValue)
            query = query.Where(x => x.Habilitada == habilitada.Value);

        var ordenes = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ComprobanteId,
                x.ProveedorId,
                x.FechaEntregaReq,
                x.CondicionesEntrega,
                x.CantidadTotal,
                x.CantidadRecibida,
                SaldoPendiente = x.CantidadTotal - x.CantidadRecibida,
                x.FechaUltimaRecepcion,
                EstadoOc = x.EstadoOc.ToString().ToUpperInvariant(),
                x.Habilitada,
                x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ordenes);
    }

    /// <summary>
    /// Retorna el detalle de una orden de compra con su comprobante.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var orden = await db.OrdenesCompraMeta
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ComprobanteId,
                x.ProveedorId,
                x.FechaEntregaReq,
                x.CondicionesEntrega,
                x.CantidadTotal,
                x.CantidadRecibida,
                SaldoPendiente = x.CantidadTotal - x.CantidadRecibida,
                x.FechaUltimaRecepcion,
                EstadoOc = x.EstadoOc.ToString().ToUpperInvariant(),
                x.Habilitada,
                x.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(orden);
    }

    /// <summary>
    /// Marca una orden de compra como recibida.
    /// </summary>
    [HttpPost("{id:long}/recibir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Recibir(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new RecibirOrdenCompraCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Orden de compra marcada como recibida." });
    }

    /// <summary>
    /// Cancela una orden de compra.
    /// </summary>
    [HttpPost("{id:long}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CancelarOrdenCompraCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Orden de compra cancelada." });
    }
}