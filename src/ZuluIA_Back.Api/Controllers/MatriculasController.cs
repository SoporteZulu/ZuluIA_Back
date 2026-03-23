using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class MatriculasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId,
        [FromQuery] long? terceroId,
        [FromQuery] bool? activa,
        CancellationToken ct = default)
    {
        var query = db.Matriculas.AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);

        if (activa.HasValue)
            query = query.Where(x => x.Activa == activa.Value);

        var result = await query
            .OrderByDescending(x => x.FechaAlta)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.TerceroId,
                x.SucursalId,
                x.NroMatricula,
                x.Descripcion,
                x.FechaAlta,
                x.FechaVencimiento,
                x.Activa,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}", Name = "GetMatriculaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Matriculas.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.TerceroId,
                x.SucursalId,
                x.NroMatricula,
                x.Descripcion,
                x.FechaAlta,
                x.FechaVencimiento,
                x.Activa,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CrearMatriculaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateMatriculaCommand(
                req.TerceroId,
                req.SucursalId,
                req.NroMatricula,
                req.Descripcion,
                req.FechaAlta,
                req.FechaVencimiento),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetMatriculaById", new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] ActualizarMatriculaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateMatriculaCommand(id, req.Descripcion, req.FechaVencimiento), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateMatriculaCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateMatriculaCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record CrearMatriculaRequest(
    long TerceroId,
    long SucursalId,
    string NroMatricula,
    string? Descripcion,
    DateOnly FechaAlta,
    DateOnly? FechaVencimiento);

public record ActualizarMatriculaRequest(
    string? Descripcion,
    DateOnly? FechaVencimiento);
