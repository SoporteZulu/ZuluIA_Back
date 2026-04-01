using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Enums;

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

    [HttpPost("notas-debito")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearNotaDebito([FromBody] CreateNotaDebitoVentaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RegistrarNotaDebitoVentaCommand(
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
                request.MotivoDebitoId,
                request.MotivoDebitoObservacion,
                request.Items,
                request.ListaPreciosId,
                request.VendedorId,
                request.CanalVentaId,
                request.CondicionPagoId,
                request.PlazoDias,
                request.Emitir),
            ct);

        return CreatedFromResult(result, "GetVentaById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpGet("notas-debito")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotasDebito(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] long? motivoDebitoId = null,
        [FromQuery] long? comprobanteOrigenId = null,
        CancellationToken ct = default)
    {
        EstadoComprobante? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado)
            && Enum.TryParse<EstadoComprobante>(estado, true, out var estadoParseado))
        {
            estadoEnum = estadoParseado;
        }

        var result = await Mediator.Send(
            new ZuluIA_Back.Application.Features.Ventas.Queries.GetNotasDebitoPagedQuery(
                page,
                pageSize,
                sucursalId,
                terceroId,
                null,
                estadoEnum,
                desde,
                hasta,
                comprobanteOrigenId,
                motivoDebitoId),
            ct);

        return Ok(result);
    }

    [HttpGet("notas-debito/motivos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMotivosDebito([FromQuery] bool soloActivos = true, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new ZuluIA_Back.Application.Features.Ventas.Queries.GetMotivosDebitoQuery(soloActivos), ct);
        return Ok(result);
    }

    [HttpGet("documentos/{id:long}/notas-debito")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotasDebitoByOrigen(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ZuluIA_Back.Application.Features.Ventas.Queries.GetNotasDebitoByOrigenQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("notas-debito/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetNotaDebitoById(long id, CancellationToken ct) => GetById(id, ct);

    [HttpPost("notas-debito/{id:long}/emitir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmitirNotaDebito(long id, [FromBody] EmitirNotaDebitoVentaRequest? request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new EmitirDocumentoVentaCommand(
                id,
                OperacionStockVenta.Ninguna,
                request?.AfectaCuentaCorriente == false ? OperacionCuentaCorrienteVenta.Ninguna : OperacionCuentaCorrienteVenta.Debito),
            ct);

        return FromResult(result);
    }

    [HttpPost("remitos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearRemito([FromBody] CreateDocumentoVentaRequest request, CancellationToken ct) =>
        await CrearBorrador(request, ct);

    [HttpPut("remitos/{id:long}/cot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertRemitoCot(long id, [FromBody] UpsertRemitoCotRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpsertRemitoCotCommand(id, request.Numero, request.FechaVigencia, request.Descripcion),
            ct);

        return FromResult(result);
    }

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
                request.FechaEntregaCompromiso,
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

    [HttpGet("pedidos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPedidos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] DateOnly? fechaDesde = null,
        [FromQuery] DateOnly? fechaHasta = null,
        [FromQuery] DateOnly? fechaEntregaDesde = null,
        [FromQuery] DateOnly? fechaEntregaHasta = null,
        [FromQuery] string? estadoPedido = null,
        [FromQuery] string? estadoEntrega = null,
        [FromQuery] bool? soloAtrasados = null,
        [FromQuery] long? itemId = null,
        [FromQuery] string? codigoOConcepto = null,
        CancellationToken ct = default)
    {
        EstadoPedido? parsedEstadoPedido = null;
        if (!string.IsNullOrWhiteSpace(estadoPedido)
            && Enum.TryParse<EstadoPedido>(estadoPedido, true, out var estadoPedidoValue))
        {
            parsedEstadoPedido = estadoPedidoValue;
        }

        EstadoEntregaItem? parsedEstadoEntrega = null;
        if (!string.IsNullOrWhiteSpace(estadoEntrega)
            && Enum.TryParse<EstadoEntregaItem>(estadoEntrega, true, out var estadoEntregaValue))
        {
            parsedEstadoEntrega = estadoEntregaValue;
        }

        var result = await Mediator.Send(
            new ZuluIA_Back.Application.Features.Ventas.Queries.GetPedidosConEstadoQuery(
                page,
                pageSize,
                sucursalId,
                terceroId,
                fechaDesde,
                fechaHasta,
                fechaEntregaDesde,
                fechaEntregaHasta,
                parsedEstadoPedido,
                parsedEstadoEntrega,
                soloAtrasados,
                itemId,
                codigoOConcepto),
            ct);

        return Ok(result);
    }

    [HttpGet("pedidos/{id:long}/vinculaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPedidoVinculaciones(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ZuluIA_Back.Application.Features.Ventas.Queries.GetPedidoVinculacionesQuery(id), ct);
        return OkOrNotFound(result);
    }

    [HttpPost("pedidos/{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CerrarPedido(long id, [FromBody] CerrarPedidoRequest? request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CerrarPedidoCommand(id, request?.MotivoCierre), ct);
        return FromResult(result);
    }

    [HttpPost("pedidos/cerrar-masivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CerrarPedidosMasivo([FromBody] CerrarPedidosMasivoRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CerrarPedidosMasivoCommand(
                request.SucursalId,
                request.TerceroId,
                request.FechaDesde,
                request.FechaHasta,
                request.FechaEntregaDesde,
                request.FechaEntregaHasta,
                request.SoloPendientes,
                request.MotivoCierre),
            ct);

        return result.IsSuccess
            ? Ok(new { cantidad = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("pedidos/{id:long}/actualizar-cumplimiento")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarCumplimientoPedido(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActualizarCumplimientoPedidoCommand(id), ct);
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
                request.AcreditaCuentaCorriente,
                request.MotivoDevolucion,
                request.ObservacionDevolucion,
                request.AutorizadorDevolucionId),
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
                request.FechaEntregaCompromiso,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                request.ComprobanteOrigenId,
                request.Items,
                request.ListaPreciosId,
                request.VendedorId,
                request.CanalVentaId,
                request.CondicionPagoId,
                request.PlazoDias),
            ct);

        return CreatedFromResult(result, "GetVentaById", new { id = result.IsSuccess ? result.Value : 0 });
    }
}
