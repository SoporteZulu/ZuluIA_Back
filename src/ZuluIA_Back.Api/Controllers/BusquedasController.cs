using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;
using ZuluIA_Back.Application.Features.Extras.DTOs;

namespace ZuluIA_Back.Api.Controllers;

public class BusquedasController(
    IMediator mediator,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna las búsquedas guardadas del usuario actual y las globales
    /// para un módulo dado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string modulo,
        [FromQuery] long? usuarioId = null,
        CancellationToken ct = default)
    {
        var query = db.Busquedas.AsNoTracking()
            .Where(x => x.Modulo == modulo.Trim().ToLowerInvariant());

        if (usuarioId.HasValue)
            query = query.Where(x => x.EsGlobal || x.UsuarioId == usuarioId.Value);
        else
            query = query.Where(x => x.EsGlobal);

        var busquedas = await query
            .OrderBy(x => x.Nombre)
            .Select(x => new BusquedaDto
            {
                Id          = x.Id,
                Nombre      = x.Nombre,
                Modulo      = x.Modulo,
                FiltrosJson = x.FiltrosJson,
                UsuarioId   = x.UsuarioId,
                EsGlobal    = x.EsGlobal,
                CreatedAt   = x.CreatedAt,
                UpdatedAt   = x.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(busquedas);
    }

    /// <summary>
    /// Guarda una nueva búsqueda.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBusquedaRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateBusquedaCommand(
                request.Nombre,
                request.Modulo,
                request.FiltrosJson,
                request.UsuarioId,
                request.EsGlobal),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value });
    }

    /// <summary>
    /// Actualiza una búsqueda guardada.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateBusquedaRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateBusquedaCommand(
                id,
                request.Nombre,
                request.FiltrosJson,
                request.EsGlobal),
            ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Búsqueda actualizada correctamente." });
    }

    /// <summary>
    /// Elimina una búsqueda guardada.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteBusquedaCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Búsqueda eliminada correctamente." });
    }
}

public record CreateBusquedaRequest(
    string Nombre,
    string Modulo,
    string FiltrosJson,
    long? UsuarioId,
    bool EsGlobal);

public record UpdateBusquedaRequest(
    string Nombre,
    string FiltrosJson,
    bool EsGlobal);