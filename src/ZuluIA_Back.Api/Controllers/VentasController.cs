using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;

namespace ZuluIA_Back.Api.Controllers;

public class VentasController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet("documentos/{id:long}", Name = "GetVentaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetComprobanteDetalleQuery(id), ct);
        return OkOrNotFound(result);
    }

    [HttpGet("documentos/{id:long}/vinculos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVinculos(long id, CancellationToken ct)
    {
        var actual = await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ComprobanteOrigenId,
                x.TipoComprobanteId,
                x.TerceroId,
                Numero = x.Numero.Formateado,
                Estado = x.Estado.ToString().ToUpperInvariant()
            })
            .FirstOrDefaultAsync(ct);

        if (actual is null)
            return NotFound(new { error = $"No se encontró el comprobante ID {id}." });

        var hijos = await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.ComprobanteOrigenId == id)
            .OrderBy(x => x.Fecha)
            .Select(x => new
            {
                x.Id,
                x.TipoComprobanteId,
                x.TerceroId,
                Numero = x.Numero.Formateado,
                Estado = x.Estado.ToString().ToUpperInvariant(),
                x.Fecha,
                x.Total
            })
            .ToListAsync(ct);

        return Ok(new
        {
            actual,
            vinculadosDesde = hijos
        });
    }

    [HttpPost("notas-pedido")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearNotaPedido([FromBody] CreateDocumentoVentaRequest request, CancellationToken ct) =>
        await CrearBorrador(request, ct);

    [HttpPost("pre-facturas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearPreFactura([FromBody] CreateDocumentoVentaRequest request, CancellationToken ct) =>
        await CrearBorrador(request, ct);

    [HttpPost("pre-notas-credito")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearPreNotaCredito([FromBody] CreateDocumentoVentaRequest request, CancellationToken ct) =>
        await CrearBorrador(request, ct);

    [HttpPost("remitos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearRemito([FromBody] CreateDocumentoVentaRequest request, CancellationToken ct) =>
        await CrearBorrador(request, ct);

    [HttpPost("remitos/{id:long}/emitir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmitirRemito(long id, [FromBody] EmitirRemitoRequest? request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new EmitirDocumentoVentaCommand(
                id,
                request?.AfectaStock == false ? OperacionStockVenta.Ninguna : OperacionStockVenta.Egreso,
                request?.EsValorizado == true ? OperacionCuentaCorrienteVenta.Debito : OperacionCuentaCorrienteVenta.Ninguna),
            ct);

        return FromResult(result);
    }

    [HttpPost("remitos/emitir-masivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmitirRemitosMasivo([FromBody] EmitirRemitosMasivoRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new EmitirRemitosVentaMasivosCommand(
                request.ComprobanteIds,
                request.EsValorizado ? OperacionCuentaCorrienteVenta.Debito : OperacionCuentaCorrienteVenta.Ninguna),
            ct);

        return result.IsSuccess
            ? Ok(new { emitidos = result.Value.Count, ids = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("documentos/{id:long}/emitir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmitirDocumento(long id, [FromBody] EmitirDocumentoVentaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new EmitirDocumentoVentaCommand(
                id,
                request.AfectaStock ? OperacionStockVenta.Egreso : OperacionStockVenta.Ninguna,
                request.AfectaCuentaCorriente ? OperacionCuentaCorrienteVenta.Debito : OperacionCuentaCorrienteVenta.Ninguna),
            ct);

        return FromResult(result);
    }

    [HttpPost("documentos/{id:long}/convertir")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> ConvertirDocumento(long id, [FromBody] ConvertirDocumentoVentaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ConvertirDocumentoVentaCommand(
                id,
                request.TipoComprobanteDestinoId,
                request.PuntoFacturacionId,
                request.Fecha,
                request.FechaVencimiento,
                request.Observacion,
                request.AfectaStock ? OperacionStockVenta.Egreso : OperacionStockVenta.Ninguna,
                request.AfectaCuentaCorriente
                    ? (request.EsCreditoCuentaCorriente ? OperacionCuentaCorrienteVenta.Credito : OperacionCuentaCorrienteVenta.Debito)
                    : OperacionCuentaCorrienteVenta.Ninguna),
            ct);

        return CreatedFromResult(result, "GetVentaById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("documentos/{id:long}/vincular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Vincular(long id, [FromBody] VincularDocumentoVentaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new VincularComprobanteVentaCommand(id, request.ComprobanteDestinoId), ct);
        return FromResult(result);
    }

    [HttpPost("devoluciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarDevolucion([FromBody] RegistrarDevolucionVentaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RegistrarDevolucionVentaCommand(
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
                request.ReingresaStock,
                request.AcreditaCuentaCorriente),
            ct);

        return CreatedFromResult(result, "GetVentaById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    private async Task<IActionResult> CrearBorrador(CreateDocumentoVentaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CrearBorradorVentaCommand(
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

        return CreatedFromResult(result, "GetVentaById", new { id = result.IsSuccess ? result.Value : 0 });
    }
}

public record CreateDocumentoVentaRequest(
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

public record EmitirRemitoRequest(bool EsValorizado, bool AfectaStock = true);
public record EmitirRemitosMasivoRequest(IReadOnlyList<long> ComprobanteIds, bool EsValorizado);
public record EmitirDocumentoVentaRequest(bool AfectaStock, bool AfectaCuentaCorriente);

public record ConvertirDocumentoVentaRequest(
    long TipoComprobanteDestinoId,
    long? PuntoFacturacionId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion,
    bool AfectaStock,
    bool AfectaCuentaCorriente,
    bool EsCreditoCuentaCorriente = false);

public record VincularDocumentoVentaRequest(long ComprobanteDestinoId);

public record RegistrarDevolucionVentaRequest(
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
    bool ReingresaStock = true,
    bool AcreditaCuentaCorriente = true);
