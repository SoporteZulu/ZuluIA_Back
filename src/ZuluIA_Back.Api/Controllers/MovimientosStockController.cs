using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.Queries;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class MovimientosStockController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna movimientos de stock paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? itemId = null,
        [FromQuery] long? depositoId = null,
        [FromQuery] string? tipo = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        TipoMovimientoStock? tipoEnum = null;
        if (!string.IsNullOrWhiteSpace(tipo) &&
            Enum.TryParse<TipoMovimientoStock>(tipo, true, out var parsedTipo))
            tipoEnum = parsedTipo;

        var result = await Mediator.Send(
            new GetMovimientosStockPagedQuery(
                page, pageSize,
                itemId, depositoId,
                tipoEnum, desde, hasta),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Retorna los movimientos de stock asociados a un origen específico.
    /// Por ejemplo: origen_tabla=comprobantes, origen_id=123
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

        var movimientos = await db.MovimientosStock
            .AsNoTracking()
            .Where(x =>
                x.OrigenTabla == origenTabla &&
                x.OrigenId    == origenId)
            .OrderByDescending(x => x.Fecha)
            .Select(x => new
            {
                x.Id,
                x.ItemId,
                x.DepositoId,
                x.Fecha,
                TipoMovimiento = x.TipoMovimiento.ToString(),
                x.Cantidad,
                x.SaldoResultante,
                x.OrigenTabla,
                x.OrigenId,
                x.Observacion,
                x.CreatedAt,
                x.CreatedBy
            })
            .ToListAsync(ct);

        return Ok(movimientos);
    }

    /// <summary>
    /// Retorna los tipos de movimiento disponibles.
    /// </summary>
    [HttpGet("tipos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTipos()
    {
        var tipos = Enum.GetValues<TipoMovimientoStock>()
            .Select(t => new
            {
                valor = t.ToString(),
                descripcion = t switch
                {
                    TipoMovimientoStock.CompraRecepcion => "Recepción de Compra",
                    TipoMovimientoStock.DevolucionVenta => "Devolución de Venta",
                    TipoMovimientoStock.AjustePositivo => "Ajuste Positivo",
                    TipoMovimientoStock.TransferenciaEntrada => "Transferencia Entrada",
                    TipoMovimientoStock.ProduccionIngreso => "Ingreso por Producción",
                    TipoMovimientoStock.VentaDespacho => "Despacho por Venta",
                    TipoMovimientoStock.DevolucionCompra => "Devolución a Proveedor",
                    TipoMovimientoStock.AjusteNegativo => "Ajuste Negativo",
                    TipoMovimientoStock.TransferenciaSalida => "Transferencia Salida",
                    TipoMovimientoStock.ProduccionConsumo => "Consumo por Producción",
                    TipoMovimientoStock.StockInicial => "Stock Inicial",
                    _ => t.ToString()
                },
                esIngreso = t is
                    TipoMovimientoStock.CompraRecepcion      or
                    TipoMovimientoStock.DevolucionVenta      or
                    TipoMovimientoStock.AjustePositivo       or
                    TipoMovimientoStock.TransferenciaEntrada or
                    TipoMovimientoStock.ProduccionIngreso    or
                    TipoMovimientoStock.StockInicial
            })
            .ToList();

        return Ok(tipos);
    }

    /// <summary>
    /// Retorna estadísticas de movimientos por ítem en un período.
    /// </summary>
    [HttpGet("estadisticas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadisticas(
        [FromQuery] long? sucursalId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        CancellationToken ct)
    {
        var desdeUtc = desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var hastaUtc = hasta.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var query = db.MovimientosStock
            .AsNoTracking()
            .Where(x => x.Fecha >= desdeUtc && x.Fecha <= hastaUtc);

        if (sucursalId.HasValue)
        {
            var depositoIds = db.Depositos
                .AsNoTracking()
                .Where(d => d.SucursalId == sucursalId.Value)
                .Select(d => d.Id);

            query = query.Where(x => depositoIds.Contains(x.DepositoId));
        }

        var stats = await query
            .GroupBy(x => new { x.ItemId, x.TipoMovimiento })
            .Select(g => new
            {
                g.Key.ItemId,
                TipoMovimiento = g.Key.TipoMovimiento.ToString(),
                TotalMovimientos = g.Count(),
                TotalCantidad = g.Sum(x => x.Cantidad)
            })
            .OrderBy(x => x.ItemId)
            .ToListAsync(ct);

        return Ok(new
        {
            desde,
            hasta,
            sucursalId,
            estadisticas = stats
        });
    }
}