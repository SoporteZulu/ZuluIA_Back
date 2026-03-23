using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ListasPrecios.Commands;
using ZuluIA_Back.Application.Features.ListasPrecios.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class ListasPreciosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    /// <summary>
    /// Retorna todas las listas de precios activas.
    /// Opcionalmente filtra por fecha de vigencia.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? fecha = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetListasPreciosQuery(fecha), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de una lista con todos sus ítems y precios.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetListaPreciosById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetListaPreciosByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna el precio de un ítem específico dentro de una lista.
    /// </summary>
    [HttpGet("{id:long}/precio/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrecioItem(
        long id,
        long itemId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPrecioItemQuery(id, itemId), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Crea una nueva lista de precios.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateListaPreciosCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(
            result,
            "GetListaPreciosById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza la cabecera de una lista de precios existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateListaPreciosCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva (soft delete) una lista de precios.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteListaPreciosCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reactiva una lista de precios desactivada.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateListaPreciosCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Agrega o actualiza el precio de un ítem dentro de la lista.
    /// Si el ítem ya existe en la lista, actualiza su precio.
    /// Si no existe, lo agrega.
    /// </summary>
    [HttpPost("{id:long}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertItem(
        long id,
        [FromBody] UpsertItemEnListaRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpsertItemEnListaCommand(id, request.ItemId, request.Precio, request.DescuentoPct),
            ct);
        return FromResult(result);
    }

    /// <summary>
    /// Elimina un ítem de la lista de precios.
    /// </summary>
    [HttpDelete("{id:long}/items/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(
        long id,
        long itemId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveItemDeListaCommand(id, itemId), ct);
        return FromResult(result);
    }

    // ── Personas asignadas (LISTASPRECIOSPERSONAS) ─────────────────────────────────

    /// <summary>
    /// Retorna las personas asignadas a una lista de precios.
    /// VB6: frmListasPrecios / LISTASPRECIOSPERSONAS.
    /// </summary>
    [HttpGet("{id:long}/personas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPersonas(long id, CancellationToken ct)
    {
        var lista = await db.ListasPreciosPersonas
            .AsNoTracking()
            .Where(x => x.ListaPreciosId == id)
            .Select(x => new { x.Id, x.ListaPreciosId, x.PersonaId })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Asigna una persona a una lista de precios.
    /// VB6: frmListasPrecios / LISTASPRECIOSPERSONAS (INSERT).
    /// </summary>
    [HttpPost("{id:long}/personas/{personaId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddPersona(long id, long personaId, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddPersonaAListaPreciosCommand(id, personaId), ct);
        if (result.IsFailure)
            return result.Error?.Contains("ya está asignada", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = result.Value });
    }

    /// <summary>
    /// Quita una persona de la lista de precios.
    /// VB6: frmListasPrecios / LISTASPRECIOSPERSONAS (DELETE).
    /// </summary>
    [HttpDelete("{id:long}/personas/{personaId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePersona(long id, long personaId, CancellationToken ct)
    {
        var result = await Mediator.Send(new RemovePersonaDeListaPreciosCommand(id, personaId), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

/// <summary>
/// Request body para UpsertItem — separa los params de ruta del body.
/// </summary>
public record UpsertItemEnListaRequest(
    long ItemId,
    decimal Precio,
    decimal DescuentoPct);