using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Produccion.Commands;
using ZuluIA_Back.Application.Features.Produccion.Queries;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class ProduccionController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet("ordenes-trabajo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdenesTrabajo(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? formulaId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        EstadoOrdenTrabajo? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoOrdenTrabajo>(estado, true, out var parsed))
            estadoEnum = parsed;

        var result = await Mediator.Send(new GetOrdenesTrabajoPagedQuery(page, pageSize, sucursalId, formulaId, estadoEnum, desde, hasta), ct);
        return Ok(result);
    }

    [HttpGet("ordenes-trabajo/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrdenTrabajoById(long id, CancellationToken ct)
    {
        var ot = await db.OrdenesTrabajo.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (ot is null)
            return NotFound(new { error = $"No se encontró la OT con ID {id}." });

        var formula = await db.FormulasProduccion.AsNoTracking()
            .Where(x => x.Id == ot.FormulaId)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.ItemResultadoId })
            .FirstOrDefaultAsync(ct);

        var depositos = await db.Depositos.AsNoTracking()
            .Where(x => x.Id == ot.DepositoOrigenId || x.Id == ot.DepositoDestinoId)
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var consumos = await db.OrdenesTrabajoConsumos.AsNoTracking()
            .Where(x => x.OrdenTrabajoId == id)
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

        var consumoItemIds = consumos.Select(x => x.ItemId).Distinct().ToList();
        var items = await db.Items.AsNoTracking()
            .Where(x => consumoItemIds.Contains(x.Id) || (formula != null && x.Id == formula.ItemResultadoId))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var empaques = await db.OrdenesEmpaque.AsNoTracking()
            .Where(x => x.OrdenTrabajoId == id)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        return Ok(new
        {
            ot.Id,
            ot.SucursalId,
            ot.FormulaId,
            FormulaCodigo = formula?.Codigo ?? "—",
            FormulaDescripcion = formula?.Descripcion ?? "—",
            ot.DepositoOrigenId,
            DepositoOrigenDescripcion = depositos.GetValueOrDefault(ot.DepositoOrigenId)?.Descripcion ?? "—",
            ot.DepositoDestinoId,
            DepositoDestinoDescripcion = depositos.GetValueOrDefault(ot.DepositoDestinoId)?.Descripcion ?? "—",
            ot.Fecha,
            ot.FechaFinPrevista,
            ot.FechaFinReal,
            ot.Cantidad,
            ot.CantidadProducida,
            Estado = ot.Estado.ToString().ToUpperInvariant(),
            ot.Observacion,
            Consumos = consumos.Select(x => new
            {
                x.Id,
                x.ItemId,
                ItemCodigo = items.GetValueOrDefault(x.ItemId)?.Codigo ?? "—",
                ItemDescripcion = items.GetValueOrDefault(x.ItemId)?.Descripcion ?? "—",
                x.DepositoId,
                x.CantidadPlanificada,
                x.CantidadConsumida,
                x.MovimientoStockId,
                x.Observacion,
                x.CreatedAt
            }),
            Empaques = empaques.Select(x => new
            {
                x.Id,
                x.ItemId,
                ItemCodigo = items.GetValueOrDefault(x.ItemId)?.Codigo ?? "—",
                ItemDescripcion = items.GetValueOrDefault(x.ItemId)?.Descripcion ?? "—",
                x.DepositoId,
                x.Fecha,
                x.Cantidad,
                x.Lote,
                Estado = x.Estado.ToString().ToUpperInvariant(),
                x.Observacion,
                x.CreatedAt
            })
        });
    }

    [HttpPost("ordenes-trabajo")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearOrdenTrabajo([FromBody] CrearOrdenTrabajoCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetOrdenTrabajoById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("ordenes-trabajo/{id:long}/iniciar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> IniciarOrdenTrabajo(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new IniciarOrdenTrabajoCommand(id), ct));

    [HttpPost("ordenes-trabajo/{id:long}/finalizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> FinalizarOrdenTrabajo(long id, [FromBody] FinalizarOrdenTrabajoRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new FinalizarOrdenTrabajoCommand(id, request.FechaFinReal, request.CantidadProducida, request.Consumos), ct));

    [HttpPost("ajustes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarAjuste([FromBody] RegistrarAjusteProduccionCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("ordenes-empaque")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdenesEmpaque([FromQuery] long? ordenTrabajoId = null, [FromQuery] string? estado = null, CancellationToken ct = default)
    {
        EstadoOrdenEmpaque? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoOrdenEmpaque>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.OrdenesEmpaque.AsNoTracking();
        if (ordenTrabajoId.HasValue)
            query = query.Where(x => x.OrdenTrabajoId == ordenTrabajoId.Value);
        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);

        var items = await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        return Ok(items.Select(x => new
        {
            x.Id,
            x.OrdenTrabajoId,
            x.ItemId,
            x.DepositoId,
            x.Fecha,
            x.Cantidad,
            x.Lote,
            Estado = x.Estado.ToString().ToUpperInvariant(),
            x.Observacion,
            x.CreatedAt
        }));
    }

    [HttpPost("ordenes-empaque")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearOrdenEmpaque([FromBody] CrearOrdenEmpaqueCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }
}

public record FinalizarOrdenTrabajoRequest(DateOnly FechaFinReal, decimal? CantidadProducida, IReadOnlyList<ConsumoOrdenTrabajoInput>? Consumos);
