using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Application.Features.Compras.Common;
using ZuluIA_Back.Application.Features.Compras.Queries;
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

    [HttpGet("cotizaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCotizaciones(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? proveedorId = null,
        [FromQuery] string? estado = null,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetCotizacionesCompraPagedQuery(page, pageSize, sucursalId, proveedorId, estado), ct));

    [HttpGet("cotizaciones/{id:long}", Name = "GetCompraCotizacionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCotizacionById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetCotizacionCompraDetalleQuery(id), ct));

    [HttpPost("cotizaciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearCotizacion([FromBody] CrearCotizacionCompraCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCompraCotizacionById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("cotizaciones/{id:long}/aceptar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AceptarCotizacion(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AceptarCotizacionCompraCommand(id), ct));

    [HttpPost("cotizaciones/{id:long}/rechazar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RechazarCotizacion(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new RechazarCotizacionCompraCommand(id), ct));

    [HttpGet("requisiciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequisiciones(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? solicitanteId = null,
        [FromQuery] string? estado = null,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetRequisicionesCompraPagedQuery(page, pageSize, sucursalId, solicitanteId, estado), ct));

    [HttpGet("requisiciones/{id:long}", Name = "GetCompraRequisicionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequisicionById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetRequisicionCompraDetalleQuery(id), ct));

    [HttpGet("solicitudes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSolicitudes(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? depositoId = null,
        [FromQuery] string? severidad = null,
        CancellationToken ct = default)
    {
        var query =
            from stock in db.Stock.AsNoTracking()
            join item in db.Items.AsNoTracking() on stock.ItemId equals item.Id
            join deposito in db.Depositos.AsNoTracking() on stock.DepositoId equals deposito.Id
            join sucursal in db.Sucursales.AsNoTracking() on deposito.SucursalId equals sucursal.Id
            join unidad in db.UnidadesMedida.AsNoTracking() on item.UnidadMedidaId equals unidad.Id into unidadJoin
            from unidad in unidadJoin.DefaultIfEmpty()
            join categoria in db.CategoriasItems.AsNoTracking() on item.CategoriaId equals categoria.Id into categoriaJoin
            from categoria in categoriaJoin.DefaultIfEmpty()
            where item.ManejaStock && item.Activo && deposito.Activo && sucursal.Activa && stock.Cantidad < item.StockMinimo
            select new
            {
                item.Id,
                item.Codigo,
                item.Descripcion,
                item.PrecioCosto,
                item.StockMaximo,
                item.StockMinimo,
                item.ManejaStock,
                DepositoId = deposito.Id,
                DepositoDescripcion = deposito.Descripcion,
                SucursalId = sucursal.Id,
                SucursalDescripcion = sucursal.NombreFantasia ?? sucursal.RazonSocial,
                StockActual = stock.Cantidad,
                UnidadMedidaDescripcion = unidad != null ? (unidad.Disminutivo ?? unidad.Descripcion) : string.Empty,
                CategoriaDescripcion = categoria != null ? categoria.Descripcion : null
            };

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (depositoId.HasValue)
            query = query.Where(x => x.DepositoId == depositoId.Value);

        var rows = await query.ToListAsync(ct);
        var severidadNormalizada = NormalizarSeveridadSolicitud(severidad);

        var result = rows
            .Select(x =>
            {
                var faltante = Math.Max(x.StockMinimo - x.StockActual, 0);
                var sugerido = CalcularCompraSugerida(x.StockActual, x.StockMinimo, x.StockMaximo);
                var severidadCalculada = CalcularSeveridadSolicitud(x.StockActual, x.StockMinimo);

                return new CompraSolicitudResumenDto(
                    $"{x.SucursalId}-{x.DepositoId}-{x.Id}",
                    x.SucursalId,
                    x.SucursalDescripcion,
                    x.DepositoId,
                    x.DepositoDescripcion,
                    x.Id,
                    x.Codigo,
                    x.Descripcion,
                    x.StockActual,
                    x.StockMinimo,
                    faltante,
                    sugerido,
                    sugerido * x.PrecioCosto,
                    severidadCalculada,
                    ObtenerEstadoReposicion(x.StockActual, x.StockMinimo, faltante, severidadCalculada),
                    ObtenerCoberturaObjetivo(x.StockActual, sugerido, x.StockMaximo),
                    x.PrecioCosto,
                    x.StockMaximo,
                    x.CategoriaDescripcion,
                    x.UnidadMedidaDescripcion,
                    x.ManejaStock,
                    null);
            })
            .Where(x => string.IsNullOrWhiteSpace(severidadNormalizada) || string.Equals(x.Severidad, severidadNormalizada, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => PrioridadSeveridad(x.Severidad))
            .ThenByDescending(x => x.Faltante)
            .ThenBy(x => x.Codigo)
            .ToList();

        return Ok(result);
    }

    [HttpPost("requisiciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearRequisicion([FromBody] CrearRequisicionCompraCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCompraRequisicionById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("requisiciones/{id:long}/enviar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EnviarRequisicion(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new EnviarRequisicionCompraCommand(id), ct));

    [HttpPost("requisiciones/{id:long}/aprobar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AprobarRequisicion(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AprobarRequisicionCompraCommand(id), ct));

    [HttpPost("requisiciones/{id:long}/rechazar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RechazarRequisicion(long id, [FromBody] string? motivo, CancellationToken ct)
        => FromResult(await Mediator.Send(new RechazarRequisicionCompraCommand(id, motivo), ct));

    [HttpPost("requisiciones/{id:long}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelarRequisicion(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new CancelarRequisicionCompraCommand(id), ct));

static string CalcularSeveridadSolicitud(decimal stockActual, decimal stockMinimo)
{
    if (stockActual <= 0)
        return "critica";

    if (stockActual <= Math.Max(1m, stockMinimo * 0.5m))
        return "urgente";

    return "normal";
}

static decimal CalcularCompraSugerida(decimal stockActual, decimal stockMinimo, decimal? stockMaximo)
{
    var faltante = Math.Max(stockMinimo - stockActual, 1);
    if (stockMaximo.HasValue && stockMaximo.Value > stockActual)
        return Math.Max(stockMaximo.Value - stockActual, faltante);

    return faltante;
}

static string ObtenerCoberturaObjetivo(decimal stockActual, decimal sugerido, decimal? stockMaximo)
{
    var target = stockActual + sugerido;
    return stockMaximo.HasValue && stockMaximo.Value > 0
        ? string.Create(CultureInfo.InvariantCulture, $"{target} u. sobre objetivo {stockMaximo.Value}")
        : string.Create(CultureInfo.InvariantCulture, $"{target} u. proyectadas contra mínimo operativo");
}

static string ObtenerEstadoReposicion(decimal stockActual, decimal stockMinimo, decimal faltante, string severidad)
{
    if (stockActual <= 0)
        return "Reposición inmediata requerida";

    if (faltante >= stockMinimo)
        return "Déficit completo contra mínimo";

    if (string.Equals(severidad, "urgente", StringComparison.OrdinalIgnoreCase))
        return "Reposición prioritaria en curso";

    return "Reposición preventiva recomendada";
}

static string? NormalizarSeveridadSolicitud(string? severidad)
{
    if (string.IsNullOrWhiteSpace(severidad))
        return null;

    var value = severidad.Trim().ToLowerInvariant();
    return value switch
    {
        "critica" or "crítica" => "critica",
        "urgente" => "urgente",
        "normal" => "normal",
        _ => value
    };
}

static int PrioridadSeveridad(string severidad) => NormalizarSeveridadSolicitud(severidad) switch
{
    "critica" => 0,
    "urgente" => 1,
    _ => 2
};

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

}

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

public record CompraSolicitudResumenDto(
    string Id,
    long SucursalId,
    string SucursalDescripcion,
    long DepositoId,
    string DepositoDescripcion,
    long ItemId,
    string Codigo,
    string Descripcion,
    decimal StockActual,
    decimal StockMinimo,
    decimal Faltante,
    decimal Sugerido,
    decimal CostoEstimado,
    string Severidad,
    string EstadoReposicion,
    string CoberturaObjetivo,
    decimal PrecioCosto,
    decimal? StockMaximo,
    string? CategoriaDescripcion,
    string UnidadMedidaDescripcion,
    bool ManejaStock,
    string? ProveedorPreferido);
