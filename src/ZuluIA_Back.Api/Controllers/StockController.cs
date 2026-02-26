using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class StockController(
    IMediator mediator,
    IStockRepository stockRepo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet("item/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByItem(long itemId, CancellationToken ct)
    {
        var stock = await stockRepo.GetByItemAsync(itemId, ct);
        return Ok(stock.Select(s => new
        {
            s.Id,
            s.ItemId,
            s.DepositoId,
            s.Cantidad,
            s.UpdatedAt
        }));
    }

    [HttpGet("item/{itemId:long}/deposito/{depositoId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSaldo(long itemId, long depositoId, CancellationToken ct)
    {
        var saldo = await stockRepo.GetSaldoAsync(itemId, depositoId, ct);
        return Ok(new { itemId, depositoId, saldo });
    }

    [HttpGet("movimientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovimientos(
        [FromQuery] long? itemId = null,
        [FromQuery] long? depositoId = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = db.MovimientosStock.AsNoTracking();

        if (itemId.HasValue)
            query = query.Where(m => m.ItemId == itemId.Value);

        if (depositoId.HasValue)
            query = query.Where(m => m.DepositoId == depositoId.Value);

        if (desde.HasValue)
            query = query.Where(m => DateOnly.FromDateTime(m.Fecha.Date) >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(m => DateOnly.FromDateTime(m.Fecha.Date) <= hasta.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.ItemId,
                m.DepositoId,
                m.Fecha,
                TipoMovimiento = m.TipoMovimiento.ToString(),
                m.Cantidad,
                m.SaldoResultante,
                m.Observacion
            })
            .ToListAsync(ct);

        return Ok(new
        {
            items,
            page,
            pageSize,
            totalCount = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }
}