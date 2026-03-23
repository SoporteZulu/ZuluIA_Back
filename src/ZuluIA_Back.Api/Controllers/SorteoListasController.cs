using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class SorteoListasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId,
        [FromQuery] long? tipoId,
        [FromQuery] bool? activa,
        CancellationToken ct = default)
    {
        var query = db.SorteosLista.AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (tipoId.HasValue)
            query = query.Where(x => x.TipoId == tipoId.Value);

        if (activa.HasValue)
            query = query.Where(x => x.Activa == activa.Value);

        var result = await query
            .OrderByDescending(x => x.FechaInicio)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TipoId,
                x.Descripcion,
                x.FechaInicio,
                x.FechaFin,
                x.Activa,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}", Name = "GetSorteoListaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.SorteosLista.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TipoId,
                x.Descripcion,
                x.FechaInicio,
                x.FechaFin,
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
    public async Task<IActionResult> Create([FromBody] CrearSorteoListaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateSorteoListaCommand(
                req.SucursalId,
                req.TipoId,
                req.Descripcion,
                req.FechaInicio,
                req.FechaFin),
            ct);

        return result.IsSuccess
            ? CreatedAtRoute("GetSorteoListaById", new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] ActualizarSorteoListaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateSorteoListaCommand(id, req.Descripcion, req.FechaInicio, req.FechaFin),
            ct);

        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible actualizar el sorteo.";
            return error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return Ok(new { Id = id });
    }

    [HttpPatch("{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cerrar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CloseSorteoListaCommand(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("{id:long}/participantes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParticipantes(long id, CancellationToken ct)
    {
        var result = await db.SorteosListaXCliente.AsNoTracking()
            .Where(x => x.SorteoListaId == id)
            .OrderBy(x => x.NroTicket)
            .Select(x => new
            {
                x.Id,
                x.SorteoListaId,
                x.TerceroId,
                x.NroTicket,
                x.Ganador
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("{id:long}/participantes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddParticipante(long id, [FromBody] InscribirParticipanteRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddParticipanteSorteoCommand(id, req.TerceroId, req.NroTicket), ct);
        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible agregar el participante.";
            if (error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error });
            if (error.Contains("ya existe", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { error });
            return BadRequest(new { error });
        }

        return CreatedAtAction(nameof(GetParticipantes), new { id }, new { Id = result.Value });
    }

    [HttpPatch("{id:long}/participantes/{participanteId:long}/ganador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarGanador(long id, long participanteId, CancellationToken ct)
    {
        var result = await Mediator.Send(new MarkSorteoParticipanteGanadorCommand(id, participanteId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok(new { result.Value.Id, result.Value.Ganador });
    }

    [HttpGet("tipos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos([FromQuery] bool? activo, CancellationToken ct)
    {
        var query = db.SorteosListaTipos.AsNoTracking();
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        var result = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("tipos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTipo([FromBody] CrearSorteoTipoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateSorteoListaTipoCommand(req.Codigo, req.Descripcion), ct);
        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible crear el tipo.";
            return error.Contains("ya existe", StringComparison.OrdinalIgnoreCase)
                ? Conflict(new { error })
                : BadRequest(new { error });
        }

        return CreatedAtAction(nameof(GetTipos), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("tipos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTipo(long id, [FromBody] ActualizarSorteoTipoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateSorteoListaTipoCommand(id, req.Descripcion), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("tipos/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarTipo(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateSorteoListaTipoCommand(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("tipos/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarTipo(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateSorteoListaTipoCommand(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record CrearSorteoListaRequest(
    long SucursalId,
    long TipoId,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin);

public record ActualizarSorteoListaRequest(
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin);

public record InscribirParticipanteRequest(long TerceroId, int NroTicket);

public record CrearSorteoTipoRequest(string Codigo, string Descripcion);

public record ActualizarSorteoTipoRequest(string Descripcion);
