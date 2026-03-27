using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Application.Features.Compras.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class ComprasController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet("documentos/{id:long}", Name = "GetCompraById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentoById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetComprobanteDetalleQuery(id), ct);
        return OkOrNotFound(result);
    }

    [HttpGet("ordenes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdenes(
        [FromQuery] long? proveedorId = null,
        [FromQuery] string? estado = null,
        [FromQuery] bool? habilitada = null,
        CancellationToken ct = default)
    {
        EstadoOrdenCompra? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoOrdenCompra>(estado, true, out var parsed))
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

    [HttpGet("ordenes/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrdenById(long id, CancellationToken ct)
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

    [HttpPost("cotizaciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public Task<IActionResult> CrearCotizacion([FromBody] CreateDocumentoCompraRequest request, CancellationToken ct) =>
        CrearBorrador(request, ct);

    [HttpPost("requisiciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public Task<IActionResult> CrearRequisicion([FromBody] CreateDocumentoCompraRequest request, CancellationToken ct) =>
        CrearBorrador(request, ct);

    [HttpPost("remitos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearRemito([FromBody] CreateRemitoCompraRequest request, CancellationToken ct)
    {
        var createResult = await Mediator.Send(
            new CrearBorradorCompraCommand(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.Fecha,
                request.FechaVencimiento,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                request.ComprobanteOrigenId,
                request.Items),
            ct);

        if (!createResult.IsSuccess)
            return BadRequest(new { error = createResult.Error });

        var emitResult = await Mediator.Send(
            new EmitirDocumentoCompraCommand(
                createResult.Value,
                request.AfectaStock ? OperacionStockCompra.Ingreso : OperacionStockCompra.Ninguna,
                request.EsValorizado ? OperacionCuentaCorrienteCompra.Debito : OperacionCuentaCorrienteCompra.Ninguna),
            ct);

        return CreatedFromResult(emitResult, "GetCompraById", new { id = emitResult.IsSuccess ? emitResult.Value : 0 });
    }

    [HttpPost("documentos/{id:long}/emitir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmitirDocumento(long id, [FromBody] EmitirDocumentoCompraRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new EmitirDocumentoCompraCommand(
                id,
                request.AfectaStock ? OperacionStockCompra.Ingreso : OperacionStockCompra.Ninguna,
                request.AfectaCuentaCorriente ? OperacionCuentaCorrienteCompra.Debito : OperacionCuentaCorrienteCompra.Ninguna),
            ct);

        return FromResult(result);
    }

    [HttpPost("ordenes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenCompraRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CrearOrdenCompraCommand(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.Fecha,
                request.FechaVencimiento,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                request.FechaEntregaReq,
                request.CondicionesEntrega,
                request.Items),
            ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetOrdenById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("ordenes/{id:long}/recibir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RecibirOrden(long id, [FromBody] RecibirOrdenCompraRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RegistrarRecepcionOrdenCompraCommand(
                id,
                request.FechaRecepcion,
                request.CantidadRecibida,
                request.TipoComprobanteRemitoId,
                request.RemitoValorizado,
                request.Observacion),
            ct);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("devoluciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarDevolucion([FromBody] RegistrarDevolucionCompraRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RegistrarDevolucionCompraCommand(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.Fecha,
                request.FechaVencimiento,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                request.ComprobanteOrigenId,
                request.Items,
                request.EgresoStock,
                request.AcreditaCuentaCorriente),
            ct);

        return CreatedFromResult(result, "GetCompraById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    private async Task<IActionResult> CrearBorrador(CreateDocumentoCompraRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CrearBorradorCompraCommand(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.Fecha,
                request.FechaVencimiento,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                request.ComprobanteOrigenId,
                request.Items),
            ct);

        return CreatedFromResult(result, "GetCompraById", new { id = result.IsSuccess ? result.Value : 0 });
    }
}

public record CreateDocumentoCompraRequest(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    long? ComprobanteOrigenId,
    IReadOnlyList<ComprobanteItemInput> Items);

public record CreateRemitoCompraRequest(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    long? ComprobanteOrigenId,
    IReadOnlyList<ComprobanteItemInput> Items,
    bool EsValorizado,
    bool AfectaStock = true);

public record EmitirDocumentoCompraRequest(bool AfectaStock, bool AfectaCuentaCorriente);

public record CrearOrdenCompraRequest(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    DateOnly? FechaEntregaReq,
    string? CondicionesEntrega,
    IReadOnlyList<ComprobanteItemInput> Items);

public record RecibirOrdenCompraRequest(
    DateOnly FechaRecepcion,
    decimal CantidadRecibida,
    long? TipoComprobanteRemitoId,
    bool RemitoValorizado,
    string? Observacion);

public record RegistrarDevolucionCompraRequest(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    long? ComprobanteOrigenId,
    IReadOnlyList<ComprobanteItemInput> Items,
    bool EgresoStock = true,
    bool AcreditaCuentaCorriente = true);
