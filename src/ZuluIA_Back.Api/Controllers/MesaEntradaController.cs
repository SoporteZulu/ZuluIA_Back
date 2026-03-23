using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Documentos.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class MesaEntradaController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? sucursalId,
        [FromQuery] long? estadoId,
        [FromQuery] string? estadoFlow,
        [FromQuery] long? asignadoA,
        [FromQuery] bool incluirArchivados = false,
        CancellationToken ct = default)
    {
        var query = db.MesasEntrada.AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (estadoId.HasValue)
            query = query.Where(x => x.EstadoId == estadoId.Value);

        if (asignadoA.HasValue)
            query = query.Where(x => x.AsignadoA == asignadoA.Value);

        if (!string.IsNullOrWhiteSpace(estadoFlow) &&
            Enum.TryParse<EstadoMesaEntrada>(estadoFlow, true, out var flow))
        {
            query = query.Where(x => x.EstadoFlow == flow);
        }

        if (!incluirArchivados)
            query = query.Where(x =>
                x.EstadoFlow != EstadoMesaEntrada.Archivado &&
                x.EstadoFlow != EstadoMesaEntrada.Anulado);

        var result = await query
            .OrderByDescending(x => x.FechaIngreso)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TipoId,
                x.TerceroId,
                x.EstadoId,
                x.NroDocumento,
                x.Asunto,
                x.FechaIngreso,
                x.FechaVencimiento,
                x.Observacion,
                x.AsignadoA,
                EstadoFlow = x.EstadoFlow.ToString()
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.MesasEntrada.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TipoId,
                x.TerceroId,
                x.EstadoId,
                x.NroDocumento,
                x.Asunto,
                x.FechaIngreso,
                x.FechaVencimiento,
                x.Observacion,
                x.AsignadoA,
                EstadoFlow = x.EstadoFlow.ToString(),
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CrearMesaEntradaRequest req, CancellationToken ct)
    {
        var command = new CreateMesaEntradaCommand(
            req.SucursalId,
            req.TipoId,
            req.TerceroId,
            req.NroDocumento,
            req.Asunto,
            req.FechaIngreso,
            req.FechaVencimiento,
            req.Observacion);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPatch("{id:long}/asignar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Asignar(long id, [FromBody] AsignarMesaEntradaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AssignMesaEntradaCommand(id, req.UsuarioId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { result.Value.Id, result.Value.AsignadoA });
    }

    [HttpPatch("{id:long}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarEstado(long id, [FromBody] CambiarEstadoMesaEntradaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ChangeMesaEntradaEstadoCommand(id, req.EstadoId, req.EstadoFlow, req.Observacion),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("EstadoFlow inválido", StringComparison.OrdinalIgnoreCase) == true
                ? BadRequest(new { error = result.Error })
                : NotFound(new { error = result.Error });

        return Ok(new { result.Value.Id, result.Value.EstadoFlow, EstadoId = result.Value.EstadoId });
    }

    [HttpPatch("{id:long}/archivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ArchiveMesaEntradaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CancelMesaEntradaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("tipos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos([FromQuery] bool? activo, CancellationToken ct)
    {
        var query = db.MesasEntradaTipos.AsNoTracking();
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
    public async Task<IActionResult> CreateTipo([FromBody] MesaEntradaTipoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateMesaEntradaTipoCommand(req.Codigo, req.Descripcion), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetTipos), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("tipos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTipo(long id, [FromBody] MesaEntradaTipoUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateMesaEntradaTipoCommand(id, req.Descripcion), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("tipos/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarTipo(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateMesaEntradaTipoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("tipos/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarTipo(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateMesaEntradaTipoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("estados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstados([FromQuery] bool? activo, CancellationToken ct)
    {
        var query = db.MesasEntradaEstados.AsNoTracking();
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        var result = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.EsFinal, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("estados")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEstado([FromBody] MesaEntradaEstadoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateMesaEntradaEstadoCommand(req.Codigo, req.Descripcion, req.EsFinal), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetEstados), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("estados/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEstado(long id, [FromBody] MesaEntradaEstadoUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateMesaEntradaEstadoCommand(id, req.Descripcion, req.EsFinal), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("estados/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarEstado(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateMesaEntradaEstadoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("estados/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarEstado(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateMesaEntradaEstadoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record CrearMesaEntradaRequest(
    long SucursalId,
    long TipoId,
    long? TerceroId,
    string NroDocumento,
    string Asunto,
    DateOnly FechaIngreso,
    DateOnly? FechaVencimiento,
    string? Observacion);

public record AsignarMesaEntradaRequest(long UsuarioId);

public record CambiarEstadoMesaEntradaRequest(long EstadoId, string EstadoFlow, string? Observacion);

public record MesaEntradaTipoRequest(string Codigo, string Descripcion);

public record MesaEntradaTipoUpdateRequest(string Descripcion);

public record MesaEntradaEstadoRequest(string Codigo, string Descripcion, bool EsFinal);

public record MesaEntradaEstadoUpdateRequest(string Descripcion, bool EsFinal);
