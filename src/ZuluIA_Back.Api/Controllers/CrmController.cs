using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.CRM.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Módulo de gestión de relaciones con clientes / contactos (CRM).
/// VB6: clsRelacion / CONTACTOS. También wrapeado por clsClienteRelacion y clsProveedorRelacion.
/// </summary>
[Route("api/crm")]
public class CrmController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    /// <summary>Retorna todos los contactos, filtrables por persona.</summary>
    [HttpGet]
    [HttpGet("contactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? personaId = null,
        CancellationToken ct = default)
    {
        var query = db.Contactos.AsNoTracking();
        if (personaId.HasValue) query = query.Where(c => c.PersonaId == personaId);

        var lista = await query
            .Select(c => new { c.Id, c.PersonaId, c.PersonaContactoId, c.TipoRelacionId })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Retorna un contacto por ID.</summary>
    [HttpGet("{id:long}", Name = "GetContactoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var c = await db.Contactos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) return NotFound(new { error = $"Contacto {id} no encontrado." });
        return Ok(new { c.Id, c.PersonaId, c.PersonaContactoId, c.TipoRelacionId });
    }

    /// <summary>Crea un nuevo contacto (relación entre dos personas).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ContactoCrmRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateContactoCrmCommand(req.PersonaId, req.PersonaContactoId, req.TipoRelacionId), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    /// <summary>Actualiza el tipo de relación de un contacto.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] ContactoCrmUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateContactoCrmCommand(id, req.TipoRelacionId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    /// <summary>Elimina un contacto.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteContactoCrmCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("tipos-comunicado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposComunicado([FromQuery] bool? activo, CancellationToken ct)
    {
        var query = db.CrmTiposComunicado.AsNoTracking();
        if (activo.HasValue) query = query.Where(x => x.Activo == activo.Value);

        var result = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("tipos-comunicado")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTipoComunicado([FromBody] CrmCatalogoCreateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateCrmTipoComunicadoCommand(req.Codigo, req.Descripcion), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetTiposComunicado), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("tipos-comunicado/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTipoComunicado(long id, [FromBody] CrmCatalogoUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateCrmTipoComunicadoCommand(id, req.Descripcion), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("tipos-comunicado/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarTipoComunicado(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateCrmTipoComunicadoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("tipos-comunicado/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarTipoComunicado(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateCrmTipoComunicadoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("motivos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMotivos([FromQuery] bool? activo, CancellationToken ct)
    {
        var query = db.CrmMotivos.AsNoTracking();
        if (activo.HasValue) query = query.Where(x => x.Activo == activo.Value);

        var result = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("motivos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateMotivo([FromBody] CrmCatalogoCreateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateCrmMotivoCommand(req.Codigo, req.Descripcion), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetMotivos), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("motivos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMotivo(long id, [FromBody] CrmCatalogoUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateCrmMotivoCommand(id, req.Descripcion), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("motivos/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarMotivo(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateCrmMotivoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("motivos/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarMotivo(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateCrmMotivoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("intereses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIntereses([FromQuery] bool? activo, CancellationToken ct)
    {
        var query = db.CrmIntereses.AsNoTracking();
        if (activo.HasValue) query = query.Where(x => x.Activo == activo.Value);

        var result = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Activo })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("intereses")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateInteres([FromBody] CrmCatalogoCreateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateCrmInteresCommand(req.Codigo, req.Descripcion), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetIntereses), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("intereses/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInteres(long id, [FromBody] CrmCatalogoUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateCrmInteresCommand(id, req.Descripcion), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("intereses/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactivarInteres(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateCrmInteresCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("intereses/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarInteres(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateCrmInteresCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("campanas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampanas(
        [FromQuery] long? sucursalId,
        [FromQuery] bool? activa,
        CancellationToken ct)
    {
        var query = db.CrmCampanas.AsNoTracking();
        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (activa.HasValue) query = query.Where(x => x.Activa == activa.Value);

        var result = await query
            .OrderByDescending(x => x.FechaInicio)
            .ThenBy(x => x.Nombre)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.Nombre,
                x.Descripcion,
                x.FechaInicio,
                x.FechaFin,
                x.Presupuesto,
                x.Activa
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("campanas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCampana([FromBody] CrmCampanaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateCrmCampanaCommand(
                req.SucursalId,
                req.Nombre,
                req.Descripcion,
                req.FechaInicio,
                req.FechaFin,
                req.Presupuesto),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetCampanas), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("campanas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCampana(long id, [FromBody] CrmCampanaUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateCrmCampanaCommand(id, req.Nombre, req.Descripcion, req.FechaInicio, req.FechaFin, req.Presupuesto),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("campanas/{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CerrarCampana(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CloseCrmCampanaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("comunicados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComunicados(
        [FromQuery] long? sucursalId,
        [FromQuery] long? terceroId,
        [FromQuery] long? campanaId,
        CancellationToken ct)
    {
        var query = db.CrmComunicados.AsNoTracking();
        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue) query = query.Where(x => x.TerceroId == terceroId.Value);
        if (campanaId.HasValue) query = query.Where(x => x.CampanaId == campanaId.Value);

        var result = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TerceroId,
                x.CampanaId,
                x.TipoId,
                x.Fecha,
                x.Asunto,
                x.Contenido,
                x.UsuarioId
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("comunicados")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateComunicado([FromBody] CrmComunicadoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateCrmComunicadoCommand(
                req.SucursalId,
                req.TerceroId,
                req.CampanaId,
                req.TipoId,
                req.Fecha,
                req.Asunto,
                req.Contenido,
                req.UsuarioId),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetComunicados), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("comunicados/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComunicado(long id, [FromBody] CrmComunicadoUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateCrmComunicadoCommand(id, req.Asunto, req.Contenido), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpDelete("comunicados/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComunicado(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmComunicadoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("seguimientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeguimientos(
        [FromQuery] long? sucursalId,
        [FromQuery] long? terceroId,
        [FromQuery] long? campanaId,
        CancellationToken ct)
    {
        var query = db.CrmSeguimientos.AsNoTracking();
        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue) query = query.Where(x => x.TerceroId == terceroId.Value);
        if (campanaId.HasValue) query = query.Where(x => x.CampanaId == campanaId.Value);

        var result = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TerceroId,
                x.MotivoId,
                x.InteresId,
                x.CampanaId,
                x.Fecha,
                x.Descripcion,
                x.ProximaAccion,
                x.UsuarioId
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("seguimientos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSeguimiento([FromBody] CrmSeguimientoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateCrmSeguimientoCommand(
                req.SucursalId,
                req.TerceroId,
                req.MotivoId,
                req.InteresId,
                req.CampanaId,
                req.Fecha,
                req.Descripcion,
                req.ProximaAccion,
                req.UsuarioId),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetSeguimientos), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("seguimientos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSeguimiento(long id, [FromBody] CrmSeguimientoUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateCrmSeguimientoCommand(id, req.Descripcion, req.ProximaAccion), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpDelete("seguimientos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSeguimiento(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmSeguimientoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record ContactoCrmRequest(long PersonaId, long PersonaContactoId, long? TipoRelacionId = null);
public record ContactoCrmUpdateRequest(long? TipoRelacionId = null);
public record CrmCatalogoCreateRequest(string Codigo, string Descripcion);
public record CrmCatalogoUpdateRequest(string Descripcion);
public record CrmCampanaRequest(long SucursalId, string Nombre, string? Descripcion, DateOnly FechaInicio, DateOnly FechaFin, decimal? Presupuesto);
public record CrmCampanaUpdateRequest(string Nombre, string? Descripcion, DateOnly FechaInicio, DateOnly FechaFin, decimal? Presupuesto);
public record CrmComunicadoRequest(long SucursalId, long TerceroId, long? CampanaId, long? TipoId, DateOnly Fecha, string Asunto, string? Contenido, long? UsuarioId);
public record CrmComunicadoUpdateRequest(string Asunto, string? Contenido);
public record CrmSeguimientoRequest(long SucursalId, long TerceroId, long? MotivoId, long? InteresId, long? CampanaId, DateOnly Fecha, string Descripcion, DateOnly? ProximaAccion, long? UsuarioId);
public record CrmSeguimientoUpdateRequest(string Descripcion, DateOnly? ProximaAccion);

