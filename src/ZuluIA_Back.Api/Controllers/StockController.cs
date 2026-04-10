using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.Commands;
using ZuluIA_Back.Application.Features.Stock.Queries;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class StockController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna el stock total de un ítem con el detalle por depósito.
    /// </summary>
    [HttpGet("item/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByItem(long itemId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetStockByItemQuery(itemId), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna todos los ítems con su stock para un depósito dado.
    /// </summary>
    [HttpGet("deposito/{depositoId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDeposito(long depositoId, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetStockByDepositoQuery(depositoId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el stock actual de un ítem en un depósito específico.
    /// </summary>
    [HttpGet("item/{itemId:long}/deposito/{depositoId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByItemDeposito(
        long itemId,
        long depositoId,
        CancellationToken ct)
    {
        var stock = await db.Stock
            .AsNoTracking()
            .Where(x => x.ItemId == itemId && x.DepositoId == depositoId)
            .Select(x => new
            {
                x.Id,
                x.ItemId,
                x.DepositoId,
                x.Cantidad,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(stock);
    }

    /// <summary>
    /// Retorna los ítems con stock por debajo del mínimo.
    /// Opcionalmente filtra por sucursal o depósito.
    /// </summary>
    [HttpGet("bajo-minimo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBajoMinimo(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? depositoId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetStockBajoMinimoQuery(sucursalId, depositoId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna un resumen general de stock por sucursal.
    /// </summary>
    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen(
        [FromQuery] long sucursalId,
        CancellationToken ct)
    {
        var depositoIds = await db.Depositos
            .AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Activo)
            .Select(x => x.Id)
            .ToListAsync(ct);

        var totalItems = await db.Stock
            .AsNoTracking()
            .Where(x => depositoIds.Contains(x.DepositoId) && x.Cantidad > 0)
            .Select(x => x.ItemId)
            .Distinct()
            .CountAsync(ct);

        var itemsBajoMinimo = await (
            from s in db.Stock.AsNoTracking()
            join i in db.Items.AsNoTracking() on s.ItemId equals i.Id
            where depositoIds.Contains(s.DepositoId) &&
                  i.ManejaStock &&
                  i.Activo &&
                  s.Cantidad < i.StockMinimo
            select s.ItemId)
            .Distinct()
            .CountAsync(ct);

        var itemsSinStock = await (
            from s in db.Stock.AsNoTracking()
            join i in db.Items.AsNoTracking() on s.ItemId equals i.Id
            where depositoIds.Contains(s.DepositoId) &&
                  i.ManejaStock &&
                  i.Activo &&
                  s.Cantidad <= 0
            select s.ItemId)
            .Distinct()
            .CountAsync(ct);

        return Ok(new
        {
            sucursalId,
            totalItemsConStock = totalItems,
            itemsBajoMinimo,
            itemsSinStock,
            totalDepositos = depositoIds.Count
        });
    }

    /// <summary>
    /// Realiza un ajuste de stock (inventario) para un ítem/depósito.
    /// Ajusta la cantidad a un valor específico y registra el movimiento.
    /// </summary>
    [HttpPost("ajuste")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ajuste(
        [FromBody] AjusteStockCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new
            {
                movimientoId = result.Value,
                mensaje = "Ajuste de stock registrado correctamente."
            })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Transfiere stock de un depósito a otro.
    /// Genera dos movimientos: salida del origen y entrada al destino.
    /// </summary>
    [HttpPost("transferencia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transferencia(
        [FromBody] TransferenciaStockCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Carga el stock inicial masivo.
    /// Útil para la puesta en marcha del sistema.
    /// </summary>
    [HttpPost("stock-inicial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StockInicial(
        [FromBody] StockInicialCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new
            {
                itemsProcesados = result.Value,
                mensaje = $"Stock inicial cargado para {result.Value} ítem(s)."
            })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Retorna los movimientos de stock filtrados y paginados.
    /// </summary>
    [HttpGet("movimientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovimientos(
        [FromQuery] long? itemId = null,
        [FromQuery] long? depositoId = null,
        [FromQuery] string? tipo = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        TipoMovimientoStock? tipoEnum = null;
        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoMovimientoStock>(tipo, true, out var parsedTipo))
            tipoEnum = parsedTipo;

        var result = await Mediator.Send(
            new GetMovimientosStockPagedQuery(page, pageSize, itemId, depositoId, tipoEnum, desde, hasta),
            ct);

        return Ok(result);
    }

    // ── Inventarios / Conteos físicos ─────────────────────────────────────────

    /// <summary>
    /// Retorna los inventarios de conteo registrados.
    /// </summary>
    [HttpGet("inventarios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventarios(
        [FromQuery] bool soloAbiertos = false,
        CancellationToken ct = default)
    {
        var q = db.InventariosConteo.AsNoTracking();
        if (soloAbiertos)
            q = q.Where(i => i.FechaCierre == null);

        var lista = await q
            .OrderByDescending(i => i.FechaApertura)
            .Select(i => new
            {
                i.Id,
                i.NroAuditoria,
                i.UsuarioId,
                i.FechaApertura,
                i.FechaCierre,
                i.FechaAlta,
                Cerrado = i.FechaCierre != null
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Retorna el detalle de un inventario de conteo.
    /// </summary>
    [HttpGet("inventarios/{id:long}", Name = "GetInventarioConteoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInventarioById(long id, CancellationToken ct)
    {
        var inv = await db.InventariosConteo.FindAsync([id], ct);
        return inv is null ? NotFound(new { error = $"Inventario {id} no encontrado." }) : Ok(inv);
    }

    /// <summary>
    /// Abre un nuevo inventario de conteo físico.
    /// </summary>
    [HttpPost("inventarios")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInventario(
        [FromBody] CreateInventarioConteoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateInventarioConteoCommand(req.UsuarioId, req.FechaApertura, req.NroAuditoria),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetInventarioConteoById", new { id = result.Value }, new { Id = result.Value });
    }

    /// <summary>
    /// Cierra un inventario de conteo abierto.
    /// </summary>
    [HttpPatch("inventarios/{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CerrarInventario(
        long id,
        [FromBody] CerrarInventarioRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CerrarInventarioConteoCommand(id, req.FechaCierre), ct);
        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible cerrar el inventario.";
            return error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return Ok();
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record CreateInventarioConteoRequest(long UsuarioId, DateTimeOffset FechaApertura, int NroAuditoria);
public record CerrarInventarioRequest(DateTimeOffset FechaCierre);
