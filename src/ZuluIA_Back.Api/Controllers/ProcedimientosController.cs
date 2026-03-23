using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Módulo de procedimientos almacenados/automatización.
/// Implementado sobre búsquedas parametrizadas (filtros JSON reutilizables).
/// </summary>
[Route("api/procedimientos")]
public class ProcedimientosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? usuarioId = null,
        [FromQuery] bool incluirGlobales = true,
        CancellationToken ct = default)
    {
        var query = db.Busquedas.AsNoTracking()
            .Where(x => x.Modulo == "procedimientos");

        if (usuarioId.HasValue)
        {
            query = incluirGlobales
                ? query.Where(x => x.EsGlobal || x.UsuarioId == usuarioId.Value)
                : query.Where(x => x.UsuarioId == usuarioId.Value);
        }
        else
        {
            query = query.Where(x => x.EsGlobal);
        }

        var result = await query
            .OrderBy(x => x.Nombre)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                DefinicionJson = x.FiltrosJson,
                x.UsuarioId,
                x.EsGlobal,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Busquedas.AsNoTracking()
            .Where(x => x.Id == id && x.Modulo == "procedimientos")
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                DefinicionJson = x.FiltrosJson,
                x.UsuarioId,
                x.EsGlobal,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProcedimientoCreateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateProcedimientoCommand(
                req.Nombre,
                req.DefinicionJson,
                req.UsuarioId,
                req.EsGlobal),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] ProcedimientoUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateProcedimientoCommand(
                id,
                req.Nombre,
                req.DefinicionJson,
                req.EsGlobal),
            ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteProcedimientoCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record ProcedimientoCreateRequest(
    string Nombre,
    string DefinicionJson,
    long? UsuarioId,
    bool EsGlobal);

public record ProcedimientoUpdateRequest(
    string Nombre,
    string DefinicionJson,
    bool EsGlobal);
