using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.Items.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class ItemsController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna el listado paginado de ítems con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] long? categoriaId = null,
        [FromQuery] bool? soloActivos = true,
        [FromQuery] bool? soloConStock = null,
        [FromQuery] bool? soloProductos = null,
        [FromQuery] bool? soloServicios = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetItemsPagedQuery(
                page, pageSize, search,
                categoriaId, soloActivos, soloConStock,
                soloProductos, soloServicios),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de un ítem por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetItemById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetItemByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Busca un ítem por código exacto.
    /// </summary>
    [HttpGet("por-codigo/{codigo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCodigo(string codigo, CancellationToken ct)
    {
        var item = await db.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Codigo == codigo.Trim().ToUpperInvariant(), ct);

        return OkOrNotFound(item is null ? null
            : await Mediator.Send(new GetItemByIdQuery(item.Id), ct));
    }

    /// <summary>
    /// Busca un ítem por código de barras.
    /// </summary>
    [HttpGet("por-codigo-barras/{codigoBarras}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCodigoBarras(
        string codigoBarras,
        CancellationToken ct)
    {
        var item = await db.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CodigoBarras == codigoBarras.Trim(), ct);

        return OkOrNotFound(item is null ? null
            : await Mediator.Send(new GetItemByIdQuery(item.Id), ct));
    }

    /// <summary>
    /// Retorna el precio de un ítem, opcionalmente resuelto
    /// desde una lista de precios vigente.
    /// </summary>
    [HttpGet("{id:long}/precio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrecio(
        long id,
        [FromQuery] long? listaPreciosId = null,
        [FromQuery] long? monedaId = null,
        [FromQuery] DateOnly? fecha = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetItemPrecioQuery(
                id,
                listaPreciosId,
                monedaId,
                fecha ?? DateOnly.FromDateTime(DateTime.Today)),
            ct);

        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna el stock actual del ítem por depósito.
    /// </summary>
    [HttpGet("{id:long}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStock(long id, CancellationToken ct)
    {
        var stock = await db.Stock
            .AsNoTracking()
            .Where(x => x.ItemId == id)
            .Join(db.Depositos.AsNoTracking(),
                s => s.DepositoId,
                d => d.Id,
                (s, d) => new
                {
                    s.Id,
                    s.ItemId,
                    s.DepositoId,
                    DepositoDescripcion = d.Descripcion,
                    d.EsDefault,
                    s.Cantidad,
                    s.UpdatedAt
                })
            .OrderByDescending(x => x.EsDefault)
            .ThenBy(x => x.DepositoDescripcion)
            .ToListAsync(ct);

        var totalStock = stock.Sum(x => x.Cantidad);

        return Ok(new
        {
            itemId = id,
            totalStock,
            depositos = stock
        });
    }

    /// <summary>
    /// Crea un nuevo ítem.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateItemCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetItemById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza los datos generales de un ítem.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateItemCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new
            {
                error = "El ID de la URL no coincide con el del body."
            });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Actualiza únicamente los precios de un ítem.
    /// </summary>
    [HttpPatch("{id:long}/precios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrecios(
        long id,
        [FromBody] UpdatePreciosRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateItemPreciosCommand(id, request.PrecioCosto, request.PrecioVenta),
            ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva (soft delete) un ítem.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        // Verificar que no tenga stock activo
        var tieneStock = await db.Stock
            .AnyAsync(x => x.ItemId == id && x.Cantidad > 0, ct);

        if (tieneStock)
            return Conflict(new
            {
                error = "No se puede desactivar un ítem que tiene stock disponible."
            });

        var result = await Mediator.Send(new DeleteItemCommand(id), ct);
        return FromResult(result);
    }
}

public record UpdatePreciosRequest(decimal PrecioCosto, decimal PrecioVenta);
