using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Text.Json;
using DomainCrmSegmento = ZuluIA_Back.Domain.Entities.CRM.CrmSegmento;
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
    [HttpGet("relaciones-contacto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? personaId = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = db.Contactos.AsNoTracking();
            if (personaId.HasValue) query = query.Where(c => c.PersonaId == personaId);

            var lista = await query
                .Select(c => new { c.Id, c.PersonaId, c.PersonaContactoId, c.TipoRelacionId })
                .ToListAsync(ct);
            return Ok(lista);
        }
        catch (DbException ex) when (IsMissingLegacyContactoRelation(ex))
        {
            return Ok(Array.Empty<object>());
        }
    }

    /// <summary>Retorna un contacto por ID.</summary>
    [HttpGet("relaciones-contacto/{id:long}", Name = "GetContactoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        try
        {
            var c = await db.Contactos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return NotFound(new { error = $"Contacto {id} no encontrado." });
            return Ok(new { c.Id, c.PersonaId, c.PersonaContactoId, c.TipoRelacionId });
        }
        catch (DbException ex) when (IsMissingLegacyContactoRelation(ex))
        {
            return NotFound(new { error = "La base local no tiene la tabla CONTACTOS. Aplicá el script actualizado de docs/crm-postgresql-local-script.md para habilitar relaciones de contacto." });
        }
    }

    /// <summary>Crea un nuevo contacto (relación entre dos personas).</summary>
    [HttpPost("relaciones-contacto")]
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
    [HttpPut("relaciones-contacto/{id:long}")]
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
    [HttpDelete("relaciones-contacto/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteContactoCrmCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    /// <summary>Retorna el catálogo semántico de tipos de relación para contactos legacy.</summary>
    [HttpGet("tipos-relacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposRelacion([FromQuery] bool? activo, CancellationToken ct = default)
    {
        var query = db.TiposRelacionesContacto.AsNoTracking();
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        var result = await query
            .OrderBy(x => x.Descripcion)
            .Select(x => new CrmTipoRelacionResponse(x.Id.ToString(), x.Codigo, x.Descripcion, x.Activo))
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPatch("oportunidades/{id:long}/cerrar-ganada")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CerrarOportunidadGanada(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CloseCrmOportunidadGanadaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var updated = await GetCrmOportunidadPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpPatch("oportunidades/{id:long}/cerrar-perdida")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CerrarOportunidadPerdida(long id, [FromBody] CrmOportunidadPerdidaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CloseCrmOportunidadPerdidaCommand(id, req.MotivoPerdida), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var updated = await GetCrmOportunidadPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpPatch("oportunidades/{id:long}/reasignar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReasignarOportunidad(long id, [FromBody] CrmReassignRequest req, CancellationToken ct)
    {
        var responsableId = ParseNullableLong(req.ResponsableId);
        if (!responsableId.HasValue)
            return BadRequest(new { error = "responsableId es requerido." });

        var result = await Mediator.Send(new ReassignCrmOportunidadCommand(id, responsableId.Value), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var updated = await GetCrmOportunidadPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
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

    /// <summary>Retorna catálogos y selectores base para formularios del módulo CRM.</summary>
    [HttpGet("catalogos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalogos(CancellationToken ct)
    {
        var tiposRelacion = await db.TiposRelacionesContacto.AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Descripcion)
            .Select(x => new CrmTipoRelacionResponse(x.Id.ToString(), x.Codigo, x.Descripcion, x.Activo))
            .ToListAsync(ct);
        var clientes = await LoadCrmReportClientsAsync(ct);
        var contactos = await db.CrmContactos.AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ThenBy(x => x.Apellido)
            .Select(x => new
            {
                Id = x.Id.ToString(),
                ClienteId = x.ClienteId.ToString(),
                Nombre = string.IsNullOrWhiteSpace(x.Apellido) ? x.Nombre : $"{x.Nombre} {x.Apellido}",
                x.Cargo,
                x.EstadoContacto
            })
            .ToListAsync(ct);
        var usuarios = await LoadCrmReportUsersAsync(ct);
        var segmentos = await db.CrmSegmentos.AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new CrmCatalogoSegmentoOptionResponse(x.Id.ToString(), x.Nombre, x.TipoSegmento))
            .ToListAsync(ct);

        return Ok(new CrmCatalogosResponse(
            MapCatalogOptions("prospecto", "activo", "inactivo", "perdido"),
            MapCatalogOptions("pyme", "corporativo", "gobierno", "startup", "otro"),
            MapCatalogOptions("campana", "referido", "web", "llamada", "evento", "otro"),
            MapCatalogOptions("nuevo", "en_negociacion", "en_riesgo", "fidelizado"),
            MapCatalogOptions("email", "telefono", "whatsapp", "presencial"),
            MapCatalogOptions("activo", "no_contactar", "bounced", "inactivo"),
            MapCatalogOptions("lead", "calificado", "propuesta", "negociacion", "cerrado_ganado", "cerrado_perdido"),
            MapCatalogOptions("USD", "ARS", "EUR", "MXN"),
            MapCatalogOptions("campana", "referido", "web", "llamada", "evento", "otro"),
            MapCatalogOptions("llamada", "email", "reunion", "visita", "ticket", "mensaje"),
            MapCatalogOptions("telefono", "email", "whatsapp", "presencial", "videollamada"),
            MapCatalogOptions("exitosa", "sin_respuesta", "reprogramada", "cancelada"),
            MapCatalogOptions("llamar", "enviar_email", "preparar_propuesta", "visitar", "seguimiento", "otro"),
            MapCatalogOptions("alta", "media", "baja"),
            MapCatalogOptions("pendiente", "en_curso", "completada", "vencida"),
            MapCatalogOptions("email", "evento", "llamadas", "redes_sociales", "publicidad"),
            MapCatalogOptions("generacion_leads", "upselling", "fidelizacion", "recuperacion", "branding"),
            MapCatalogOptions("estatico", "dinamico"),
            MapCatalogOptions("administrador", "supervisor", "comercial", "marketing", "soporte"),
            MapCatalogOptions("activo", "inactivo"),
            tiposRelacion,
            clientes.OrderBy(x => x.Nombre).Select(x => new CrmCatalogoClienteOptionResponse(x.Id.ToString(), x.Nombre, x.TipoCliente, x.Segmento, x.EstadoRelacion)).ToList(),
            contactos.Select(x => new CrmCatalogoContactoOptionResponse(x.Id, x.ClienteId, x.Nombre, x.Cargo, x.EstadoContacto)).ToList(),
            usuarios.Values.OrderBy(x => x.Nombre).Select(x => new CrmCatalogoUsuarioOptionResponse(x.Id.ToString(), x.Nombre, x.Rol)).ToList(),
            segmentos));
    }

    [HttpPost("segmentos/preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewSegmento([FromBody] CrmSegmentoPreviewRequest req, CancellationToken ct)
    {
        if (!string.Equals(req.TipoSegmento, "dinamico", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "El preview solo está disponible para segmentos CRM dinámicos." });

        if (!CrmDomainRules.TryValidateSegmentDefinition(req.TipoSegmento, SerializeSegmentCriteria(req.Criterios), out var error))
            return BadRequest(new { error });

        var clientes = await LoadCrmClientProjectionsAsync(ct);
        var miembros = ResolveDynamicSegmentMembers(clientes, req.Criterios);
        return Ok(new { CantidadClientes = miembros.Count, Clientes = miembros.Select(MapSegmentClient) });
    }

    [HttpGet("contactos/{id:long}", Name = "GetCrmContactoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCrmContactoById(long id, CancellationToken ct)
    {
        var result = await GetCrmContactoPayloadAsync(id, ct);
        return OkOrNotFound(result);
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

    [HttpPatch("tareas/{id:long}/completar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompletarTarea(long id, [FromBody] CrmCompleteTaskRequest? req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CompleteCrmTareaCommand(
                id,
                req?.FechaCompletado.HasValue == true ? DateOnly.FromDateTime(req.FechaCompletado.Value.UtcDateTime) : null),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var updated = await GetCrmTareaPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpPatch("tareas/{id:long}/reabrir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReabrirTarea(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ReopenCrmTareaCommand(id), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var updated = await GetCrmTareaPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
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

    [HttpGet("oportunidades/{id:long}", Name = "GetCrmOportunidadById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOportunidadById(long id, CancellationToken ct)
    {
        var result = await GetCrmOportunidadPayloadAsync(id, ct);
        return OkOrNotFound(result);
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

    [HttpGet("interacciones/{id:long}", Name = "GetCrmInteraccionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInteraccionById(long id, CancellationToken ct)
    {
        var result = await GetCrmInteraccionPayloadAsync(id, ct);
        return OkOrNotFound(result);
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
                Id = x.Id.ToString(),
                SucursalId = x.SucursalId,
                Nombre = x.Nombre,
                TipoCampana = x.TipoCampana,
                Objetivo = x.Objetivo,
                SegmentoObjetivoId = x.SegmentoObjetivoId.HasValue ? x.SegmentoObjetivoId.Value.ToString() : null,
                FechaInicio = x.FechaInicio,
                FechaFin = x.FechaFin,
                PresupuestoEstimado = x.Presupuesto ?? 0m,
                PresupuestoGastado = x.PresupuestoGastado ?? 0m,
                ResponsableId = x.ResponsableId.HasValue ? x.ResponsableId.Value.ToString() : null,
                Notas = string.IsNullOrWhiteSpace(x.Notas) ? x.Descripcion : x.Notas,
                LeadsGenerados = x.LeadsGenerados,
                OportunidadesGeneradas = x.OportunidadesGeneradas,
                NegociosGanados = x.NegociosGanados,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Activa = x.Activa
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("campanas/{id:long}", Name = "GetCrmCampanaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCampanaById(long id, CancellationToken ct)
    {
        var result = await GetCampanaPayloadAsync(id, ct);
        return OkOrNotFound(result);
    }

    [HttpGet("tareas/{id:long}", Name = "GetCrmTareaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTareaById(long id, CancellationToken ct)
    {
        var result = await GetCrmTareaPayloadAsync(id, ct);
        return OkOrNotFound(result);
    }

    [HttpPost("campanas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCampana([FromBody] CrmCampanaRequest req, CancellationToken ct)
    {
        var sucursalId = await ResolveCampanaSucursalIdAsync(req.SucursalId, ct);
        if (!sucursalId.HasValue)
            return BadRequest(new { error = "No existe una sucursal activa disponible para registrar la campaña CRM." });

        var segmentoObjetivoId = ParseNullableLong(req.SegmentoObjetivoId);
        var responsableId = ParseNullableLong(req.ResponsableId);
        var result = await Mediator.Send(
            new CreateCrmCampanaCommand(
                sucursalId.Value,
                req.Nombre,
                req.Notas,
                DateOnly.FromDateTime(req.FechaInicio.UtcDateTime),
                DateOnly.FromDateTime((req.FechaFin ?? req.FechaInicio).UtcDateTime),
                req.PresupuestoEstimado,
                req.TipoCampana,
                req.Objetivo,
                segmentoObjetivoId,
                req.PresupuestoGastado,
                responsableId,
                req.Notas,
                req.LeadsGenerados,
                req.OportunidadesGeneradas,
                req.NegociosGanados),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var created = await GetCampanaPayloadAsync(result.Value, ct);
        return StatusCode(StatusCodes.Status201Created, created is not null ? created : new { Id = result.Value.ToString() });
    }

    [HttpPut("campanas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCampana(long id, [FromBody] CrmCampanaUpdateRequest req, CancellationToken ct)
    {
        var segmentoObjetivoId = ParseNullableLong(req.SegmentoObjetivoId);
        var responsableId = ParseNullableLong(req.ResponsableId);
        var result = await Mediator.Send(
            new UpdateCrmCampanaCommand(
                id,
                req.Nombre,
                req.Notas,
                DateOnly.FromDateTime(req.FechaInicio.UtcDateTime),
                DateOnly.FromDateTime((req.FechaFin ?? req.FechaInicio).UtcDateTime),
                req.PresupuestoEstimado,
                req.TipoCampana,
                req.Objetivo,
                segmentoObjetivoId,
                req.PresupuestoGastado,
                responsableId,
                req.Notas,
                req.LeadsGenerados,
                req.OportunidadesGeneradas,
                req.NegociosGanados),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var updated = await GetCampanaPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
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

    [HttpDelete("campanas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteCampana(long id, CancellationToken ct)
        => CerrarCampana(id, ct);

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

    [HttpGet("segmentos/{id:long}", Name = "GetCrmSegmentoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSegmentoById(long id, CancellationToken ct)
    {
        var result = await GetCrmSegmentoPayloadAsync(id, ct);
        return OkOrNotFound(result);
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

    [HttpGet("clientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientes([FromQuery] string? busqueda = null, CancellationToken ct = default)
    {
        var query =
            from perfil in db.CrmClientes.AsNoTracking()
            join tercero in db.Terceros.AsNoTracking() on perfil.TerceroId equals tercero.Id
            where perfil.Activo
            select new
            {
                tercero.Id,
                Nombre = tercero.RazonSocial,
                perfil.TipoCliente,
                perfil.Segmento,
                perfil.Industria,
                Cuit = tercero.NroDocumento,
                perfil.Pais,
                perfil.Provincia,
                perfil.Ciudad,
                perfil.Direccion,
                TelefonoPrincipal = tercero.Telefono,
                EmailPrincipal = tercero.Email,
                SitioWeb = tercero.Web,
                perfil.OrigenCliente,
                perfil.EstadoRelacion,
                ResponsableId = perfil.ResponsableId.HasValue ? perfil.ResponsableId.Value.ToString() : null,
                FechaAlta = DateOnly.FromDateTime(tercero.CreatedAt.UtcDateTime),
                perfil.NotasGenerales,
                perfil.CreatedAt,
                perfil.UpdatedAt
            };

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Nombre.ToLower().Contains(term)
                || (x.EmailPrincipal != null && x.EmailPrincipal.ToLower().Contains(term))
                || (x.Industria != null && x.Industria.ToLower().Contains(term))
                || (x.Cuit != null && x.Cuit.ToLower().Contains(term)));
        }

        var result = await query
            .OrderBy(x => x.Nombre)
            .Select(x => new CrmClienteResponse(
                x.Id.ToString(),
                x.Nombre,
                x.TipoCliente,
                x.Segmento,
                x.Industria,
                x.Cuit != null && x.Cuit.StartsWith("CRM-", StringComparison.OrdinalIgnoreCase) ? null : x.Cuit,
                x.Pais,
                x.Provincia,
                x.Ciudad,
                x.Direccion,
                x.TelefonoPrincipal,
                x.EmailPrincipal,
                x.SitioWeb,
                x.OrigenCliente,
                x.EstadoRelacion,
                x.ResponsableId,
                x.FechaAlta,
                x.NotasGenerales,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("clientes/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClienteById(long id, CancellationToken ct)
    {
        var result = await GetClientePayloadAsync(id, ct);
        return OkOrNotFound(result);
    }

    [HttpPost("clientes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCliente([FromBody] CrmClienteUpsertRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateCrmClienteCommand(
                req.Nombre,
                req.TipoCliente,
                req.Segmento,
                req.Industria,
                req.Cuit,
                req.Pais,
                req.Provincia,
                req.Ciudad,
                req.Direccion,
                req.TelefonoPrincipal,
                req.EmailPrincipal,
                req.SitioWeb,
                req.OrigenCliente,
                req.EstadoRelacion,
                ParseNullableLong(req.ResponsableId),
                req.FechaAlta.HasValue ? DateOnly.FromDateTime(req.FechaAlta.Value.UtcDateTime) : null,
                req.NotasGenerales),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var created = await GetClientePayloadAsync(result.Value, ct);
        return StatusCode(StatusCodes.Status201Created, created is not null ? created : new { Id = result.Value.ToString() });
    }

    [HttpPut("clientes/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCliente(long id, [FromBody] CrmClienteUpsertRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateCrmClienteCommand(
                id,
                req.Nombre,
                req.TipoCliente,
                req.Segmento,
                req.Industria,
                req.Cuit,
                req.Pais,
                req.Provincia,
                req.Ciudad,
                req.Direccion,
                req.TelefonoPrincipal,
                req.EmailPrincipal,
                req.SitioWeb,
                req.OrigenCliente,
                req.EstadoRelacion,
                ParseNullableLong(req.ResponsableId),
                req.FechaAlta.HasValue ? DateOnly.FromDateTime(req.FechaAlta.Value.UtcDateTime) : null,
                req.NotasGenerales),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var updated = await GetClientePayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpDelete("clientes/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCliente(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmClienteCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("contactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCrmContactos([FromQuery] string? clienteId = null, CancellationToken ct = default)
    {
        var clienteIdValue = ParseNullableLong(clienteId);
        var query = db.CrmContactos.AsNoTracking().Where(x => x.Activo);
        if (clienteIdValue.HasValue)
            query = query.Where(x => x.ClienteId == clienteIdValue.Value);

        var result = await query
            .OrderBy(x => x.Nombre)
            .ThenBy(x => x.Apellido)
            .Select(x => new CrmContactoResponse(
                x.Id.ToString(),
                x.ClienteId.ToString(),
                x.Nombre,
                x.Apellido,
                x.Cargo,
                x.Email,
                x.Telefono,
                x.CanalPreferido,
                x.EstadoContacto,
                x.Notas,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("contactos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCrmContactoItem([FromBody] CrmContactoRequest req, CancellationToken ct)
    {
        var clienteId = ParseNullableLong(req.ClienteId);
        if (!clienteId.HasValue)
            return BadRequest(new { error = "clienteId es requerido." });

        var result = await Mediator.Send(
            new CreateCrmContactoCommand(
                clienteId.Value,
                req.Nombre,
                req.Apellido,
                req.Cargo,
                req.Email,
                req.Telefono,
                req.CanalPreferido,
                req.EstadoContacto,
                req.Notas),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var created = await GetCrmContactoPayloadAsync(result.Value, ct);
        return StatusCode(StatusCodes.Status201Created, created is not null ? created : new { Id = result.Value.ToString() });
    }

    [HttpPut("contactos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCrmContactoItem(long id, [FromBody] CrmContactoRequest req, CancellationToken ct)
    {
        var clienteId = ParseNullableLong(req.ClienteId);
        if (!clienteId.HasValue)
            return BadRequest(new { error = "clienteId es requerido." });

        var result = await Mediator.Send(
            new UpdateCrmContactoCommand(
                id,
                clienteId.Value,
                req.Nombre,
                req.Apellido,
                req.Cargo,
                req.Email,
                req.Telefono,
                req.CanalPreferido,
                req.EstadoContacto,
                req.Notas),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var updated = await GetCrmContactoPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpDelete("contactos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCrmContactoItem(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmContactoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("oportunidades")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOportunidades(
        [FromQuery] string? clienteId = null,
        [FromQuery] string? responsableId = null,
        [FromQuery] string? etapa = null,
        [FromQuery] string? busqueda = null,
        [FromQuery] DateOnly? fechaCierreDesde = null,
        [FromQuery] DateOnly? fechaCierreHasta = null,
        CancellationToken ct = default)
    {
        var clienteIdValue = ParseNullableLong(clienteId);
        var responsableIdValue = ParseNullableLong(responsableId);
        var query = db.CrmOportunidades.AsNoTracking().Where(x => x.Activa);
        if (clienteIdValue.HasValue)
            query = query.Where(x => x.ClienteId == clienteIdValue.Value);
        if (responsableIdValue.HasValue)
            query = query.Where(x => x.ResponsableId == responsableIdValue.Value);
        if (!string.IsNullOrWhiteSpace(etapa) && !string.Equals(etapa, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => x.Etapa == etapa);
        if (fechaCierreDesde.HasValue)
            query = query.Where(x => x.FechaEstimadaCierre.HasValue && x.FechaEstimadaCierre.Value >= fechaCierreDesde.Value);
        if (fechaCierreHasta.HasValue)
            query = query.Where(x => x.FechaEstimadaCierre.HasValue && x.FechaEstimadaCierre.Value <= fechaCierreHasta.Value);
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Titulo.ToLower().Contains(term)
                || (x.Notas != null && x.Notas.ToLower().Contains(term)));
        }

        var result = await query
            .OrderByDescending(x => x.FechaApertura)
            .Select(x => new CrmOportunidadResponse(
                x.Id.ToString(),
                x.ClienteId.ToString(),
                x.ContactoPrincipalId.HasValue ? x.ContactoPrincipalId.Value.ToString() : null,
                x.Titulo,
                x.Etapa,
                x.Probabilidad,
                x.MontoEstimado,
                x.Moneda,
                x.FechaApertura,
                x.FechaEstimadaCierre,
                x.ResponsableId.HasValue ? x.ResponsableId.Value.ToString() : null,
                x.Origen,
                x.MotivoPerdida,
                x.Notas,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("oportunidades")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOportunidad([FromBody] CrmOportunidadRequest req, CancellationToken ct)
    {
        var clienteId = ParseNullableLong(req.ClienteId);
        if (!clienteId.HasValue)
            return BadRequest(new { error = "clienteId es requerido." });

        var result = await Mediator.Send(
            new CreateCrmOportunidadCommand(
                clienteId.Value,
                ParseNullableLong(req.ContactoPrincipalId),
                req.Titulo,
                req.Etapa,
                req.Probabilidad,
                req.MontoEstimado,
                req.Moneda,
                DateOnly.FromDateTime(req.FechaApertura.UtcDateTime),
                req.FechaEstimadaCierre.HasValue ? DateOnly.FromDateTime(req.FechaEstimadaCierre.Value.UtcDateTime) : null,
                ParseNullableLong(req.ResponsableId),
                req.Origen,
                req.MotivoPerdida,
                req.Notas),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var created = await GetCrmOportunidadPayloadAsync(result.Value, ct);
        return StatusCode(StatusCodes.Status201Created, created is not null ? created : new { Id = result.Value.ToString() });
    }

    [HttpPut("oportunidades/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOportunidad(long id, [FromBody] CrmOportunidadRequest req, CancellationToken ct)
    {
        var clienteId = ParseNullableLong(req.ClienteId);
        if (!clienteId.HasValue)
            return BadRequest(new { error = "clienteId es requerido." });

        var result = await Mediator.Send(
            new UpdateCrmOportunidadCommand(
                id,
                clienteId.Value,
                ParseNullableLong(req.ContactoPrincipalId),
                req.Titulo,
                req.Etapa,
                req.Probabilidad,
                req.MontoEstimado,
                req.Moneda,
                DateOnly.FromDateTime(req.FechaApertura.UtcDateTime),
                req.FechaEstimadaCierre.HasValue ? DateOnly.FromDateTime(req.FechaEstimadaCierre.Value.UtcDateTime) : null,
                ParseNullableLong(req.ResponsableId),
                req.Origen,
                req.MotivoPerdida,
                req.Notas),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var updated = await GetCrmOportunidadPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpDelete("oportunidades/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOportunidad(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmOportunidadCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("interacciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInteracciones([FromQuery] string? clienteId = null, CancellationToken ct = default)
    {
        var clienteIdValue = ParseNullableLong(clienteId);
        var query = db.CrmInteracciones.AsNoTracking();
        if (clienteIdValue.HasValue)
            query = query.Where(x => x.ClienteId == clienteIdValue.Value);

        var result = await query
            .OrderByDescending(x => x.FechaHora)
            .Select(x => new CrmInteraccionResponse(
                x.Id.ToString(),
                x.ClienteId.ToString(),
                x.ContactoId.HasValue ? x.ContactoId.Value.ToString() : null,
                x.OportunidadId.HasValue ? x.OportunidadId.Value.ToString() : null,
                x.TipoInteraccion,
                x.Canal,
                x.FechaHora,
                x.UsuarioResponsableId.ToString(),
                x.Resultado,
                x.Descripcion,
                DeserializeStringList(x.AdjuntosJson),
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("interacciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInteraccion([FromBody] CrmInteraccionRequest req, CancellationToken ct)
    {
        var clienteId = ParseNullableLong(req.ClienteId);
        var usuarioResponsableId = ParseNullableLong(req.UsuarioResponsableId);
        if (!clienteId.HasValue || !usuarioResponsableId.HasValue)
            return BadRequest(new { error = "clienteId y usuarioResponsableId son requeridos." });

        var result = await Mediator.Send(
            new CreateCrmInteraccionCommand(
                clienteId.Value,
                ParseNullableLong(req.ContactoId),
                ParseNullableLong(req.OportunidadId),
                req.TipoInteraccion,
                req.Canal,
                req.FechaHora,
                usuarioResponsableId.Value,
                req.Resultado,
                req.Descripcion,
                req.Adjuntos),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var created = await GetCrmInteraccionPayloadAsync(result.Value, ct);
        return StatusCode(StatusCodes.Status201Created, created is not null ? created : new { Id = result.Value.ToString() });
    }

    [HttpPut("interacciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInteraccion(long id, [FromBody] CrmInteraccionRequest req, CancellationToken ct)
    {
        var clienteId = ParseNullableLong(req.ClienteId);
        var usuarioResponsableId = ParseNullableLong(req.UsuarioResponsableId);
        if (!clienteId.HasValue || !usuarioResponsableId.HasValue)
            return BadRequest(new { error = "clienteId y usuarioResponsableId son requeridos." });

        var result = await Mediator.Send(
            new UpdateCrmInteraccionCommand(
                id,
                clienteId.Value,
                ParseNullableLong(req.ContactoId),
                ParseNullableLong(req.OportunidadId),
                req.TipoInteraccion,
                req.Canal,
                req.FechaHora,
                usuarioResponsableId.Value,
                req.Resultado,
                req.Descripcion,
                req.Adjuntos),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var updated = await GetCrmInteraccionPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpDelete("interacciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInteraccion(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmInteraccionCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("tareas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTareas(
        [FromQuery] string? clienteId = null,
        [FromQuery] string? responsableId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? prioridad = null,
        [FromQuery] string? busqueda = null,
        [FromQuery] DateOnly? fechaVencimientoDesde = null,
        [FromQuery] DateOnly? fechaVencimientoHasta = null,
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var clienteIdValue = ParseNullableLong(clienteId);
        var responsableIdValue = ParseNullableLong(responsableId);
        var query = db.CrmTareas.AsNoTracking().Where(x => x.Activa);
        if (clienteIdValue.HasValue)
            query = query.Where(x => x.ClienteId == clienteIdValue.Value);
        if (responsableIdValue.HasValue)
            query = query.Where(x => x.AsignadoAId == responsableIdValue.Value);
        if (!string.IsNullOrWhiteSpace(prioridad) && !string.Equals(prioridad, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => x.Prioridad == prioridad);
        if (fechaVencimientoDesde.HasValue)
            query = query.Where(x => x.FechaVencimiento >= fechaVencimientoDesde.Value);
        if (fechaVencimientoHasta.HasValue)
            query = query.Where(x => x.FechaVencimiento <= fechaVencimientoHasta.Value);
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Titulo.ToLower().Contains(term)
                || (x.Descripcion != null && x.Descripcion.ToLower().Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(estado) && !string.Equals(estado, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = string.Equals(estado, "vencida", StringComparison.OrdinalIgnoreCase)
                ? query.Where(x => x.Estado == "vencida" || (x.Estado != "completada" && x.FechaVencimiento < today))
                : query.Where(x => x.Estado == estado);
        }

        var result = await query
            .OrderBy(x => x.FechaVencimiento)
            .Select(x => new CrmTareaResponse(
                x.Id.ToString(),
                x.ClienteId.HasValue ? x.ClienteId.Value.ToString() : null,
                x.OportunidadId.HasValue ? x.OportunidadId.Value.ToString() : null,
                x.AsignadoAId.ToString(),
                x.Titulo,
                x.Descripcion,
                x.TipoTarea,
                x.FechaVencimiento,
                x.Prioridad,
                x.Estado,
                x.FechaCompletado,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpPost("tareas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTarea([FromBody] CrmTareaRequest req, CancellationToken ct)
    {
        var asignadoAId = ParseNullableLong(req.AsignadoAId);
        if (!asignadoAId.HasValue)
            return BadRequest(new { error = "asignadoAId es requerido." });

        var result = await Mediator.Send(
            new CreateCrmTareaCommand(
                ParseNullableLong(req.ClienteId),
                ParseNullableLong(req.OportunidadId),
                asignadoAId.Value,
                req.Titulo,
                req.Descripcion,
                req.TipoTarea,
                DateOnly.FromDateTime(req.FechaVencimiento.UtcDateTime),
                req.Prioridad,
                req.Estado,
                req.FechaCompletado.HasValue ? DateOnly.FromDateTime(req.FechaCompletado.Value.UtcDateTime) : null),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var created = await GetCrmTareaPayloadAsync(result.Value, ct);
        return StatusCode(StatusCodes.Status201Created, created is not null ? created : new { Id = result.Value.ToString() });
    }

    [HttpPut("tareas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTarea(long id, [FromBody] CrmTareaRequest req, CancellationToken ct)
    {
        var asignadoAId = ParseNullableLong(req.AsignadoAId);
        if (!asignadoAId.HasValue)
            return BadRequest(new { error = "asignadoAId es requerido." });

        var result = await Mediator.Send(
            new UpdateCrmTareaCommand(
                id,
                ParseNullableLong(req.ClienteId),
                ParseNullableLong(req.OportunidadId),
                asignadoAId.Value,
                req.Titulo,
                req.Descripcion,
                req.TipoTarea,
                DateOnly.FromDateTime(req.FechaVencimiento.UtcDateTime),
                req.Prioridad,
                req.Estado,
                req.FechaCompletado.HasValue ? DateOnly.FromDateTime(req.FechaCompletado.Value.UtcDateTime) : null),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var updated = await GetCrmTareaPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpDelete("tareas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTarea(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmTareaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("segmentos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSegmentos(CancellationToken ct)
    {
        var clientes = await LoadCrmClientProjectionsAsync(ct);
        var membresias = await db.CrmSegmentosMiembros.AsNoTracking()
            .Where(x => x.Activo)
            .GroupBy(x => x.SegmentoId)
            .Select(x => new { SegmentoId = x.Key, Cantidad = x.Count() })
            .ToDictionaryAsync(x => x.SegmentoId, x => x.Cantidad, ct);
        var segmentos = await db.CrmSegmentos.AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(ct);

        var result = segmentos.Select(x =>
        {
            var criterios = DeserializeSegmentCriteria(x.CriteriosJson);
            var cantidadClientes = x.EsEstatico()
                ? membresias.GetValueOrDefault(x.Id, 0)
                : criterios.Count == 0
                ? clientes.Count
                : clientes.Count(cliente => criterios.All(criterion => MatchesCriterion(cliente, criterion)));

            return new CrmSegmentoResponse(
                x.Id.ToString(),
                x.Nombre,
                x.Descripcion,
                criterios,
                x.TipoSegmento,
                cantidadClientes,
                x.CreatedAt,
                x.UpdatedAt);
        });

        return Ok(result);
    }

    [HttpPost("segmentos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSegmento([FromBody] CrmSegmentoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateCrmSegmentoCommand(req.Nombre, req.Descripcion, SerializeSegmentCriteria(req.Criterios), req.TipoSegmento),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var created = await GetCrmSegmentoPayloadAsync(result.Value, ct);
        return StatusCode(StatusCodes.Status201Created, created is not null ? created : new { Id = result.Value.ToString() });
    }

    [HttpPut("segmentos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSegmento(long id, [FromBody] CrmSegmentoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateCrmSegmentoCommand(id, req.Nombre, req.Descripcion, SerializeSegmentCriteria(req.Criterios), req.TipoSegmento),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : NotFound(new { error = result.Error });

        var updated = await GetCrmSegmentoPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpGet("segmentos/{id:long}/miembros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSegmentoMiembros(long id, CancellationToken ct)
    {
        var segmento = await db.CrmSegmentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Activo, ct);
        if (segmento is null)
            return NotFound(new { error = $"Segmento CRM {id} no encontrado." });

        var miembros = await ResolveSegmentMembersAsync(segmento, ct);
        return Ok(miembros.Select(MapSegmentClient));
    }

    [HttpPost("segmentos/{id:long}/miembros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSegmentoMiembro(long id, [FromBody] CrmSegmentoMiembroRequest req, CancellationToken ct)
    {
        var clienteId = ParseNullableLong(req.ClienteId);
        if (!clienteId.HasValue)
            return BadRequest(new { error = "clienteId es requerido." });

        var result = await Mediator.Send(new AddCrmSegmentoClienteCommand(id, clienteId.Value), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var members = await GetSegmentoMiembros(id, ct) as OkObjectResult;
        return Ok(members?.Value ?? Array.Empty<object>());
    }

    [HttpDelete("segmentos/{id:long}/miembros/{clienteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSegmentoMiembro(long id, long clienteId, CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveCrmSegmentoClienteCommand(id, clienteId), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpDelete("segmentos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSegmento(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmSegmentoCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    /// <summary>Retorna métricas analíticas consolidadas del workspace CRM.</summary>
    [HttpGet("reportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportes(
        [FromQuery] string? clienteId = null,
        [FromQuery] string? responsableId = null,
        [FromQuery] string? segmento = null,
        [FromQuery] string? campanaId = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var clienteIdValue = ParseNullableLong(clienteId);
        var responsableIdValue = ParseNullableLong(responsableId);
        var campanaIdValue = ParseNullableLong(campanaId);

        var clientes = await LoadCrmReportClientsAsync(ct);
        if (clienteIdValue.HasValue)
            clientes = clientes.Where(x => x.Id == clienteIdValue.Value).ToList();
        if (responsableIdValue.HasValue)
            clientes = clientes.Where(x => x.ResponsableId == responsableIdValue.Value).ToList();
        if (!string.IsNullOrWhiteSpace(segmento) && !string.Equals(segmento, "all", StringComparison.OrdinalIgnoreCase))
            clientes = clientes.Where(x => string.Equals(x.Segmento, segmento, StringComparison.OrdinalIgnoreCase)).ToList();

        var usuarios = await LoadCrmReportUsersAsync(ct);
        if (responsableIdValue.HasValue)
            usuarios = usuarios.Where(x => x.Key == responsableIdValue.Value).ToDictionary();

        var filterByClientScope = clienteIdValue.HasValue || (!string.IsNullOrWhiteSpace(segmento) && !string.Equals(segmento, "all", StringComparison.OrdinalIgnoreCase));
        var clienteIds = clientes.Select(x => x.Id).ToHashSet();
        var oportunidades = await db.CrmOportunidades.AsNoTracking()
            .Where(x => x.Activa)
            .Select(x => new CrmReportOpportunityProjection(
                x.Id,
                x.ClienteId,
                x.Titulo,
                x.Etapa,
                x.Probabilidad,
                x.MontoEstimado,
                x.Moneda,
                x.FechaEstimadaCierre,
                x.ResponsableId,
                x.Origen))
            .ToListAsync(ct);
        oportunidades = oportunidades
            .Where(x => !filterByClientScope || clienteIds.Contains(x.ClienteId))
            .Where(x => !responsableIdValue.HasValue || x.ResponsableId == responsableIdValue.Value)
            .ToList();

        var interacciones = await db.CrmInteracciones.AsNoTracking()
            .Select(x => new CrmReportInteractionProjection(
                x.Id,
                x.ClienteId,
                x.OportunidadId,
                x.TipoInteraccion,
                x.Canal,
                x.FechaHora,
                x.UsuarioResponsableId,
                x.Resultado,
                x.Descripcion))
            .ToListAsync(ct);
        interacciones = interacciones
            .Where(x => !filterByClientScope || clienteIds.Contains(x.ClienteId))
            .Where(x => !responsableIdValue.HasValue || x.UsuarioResponsableId == responsableIdValue.Value)
            .Where(x => MatchesDateRange(x.FechaHora, desde, hasta))
            .ToList();

        var campanas = await db.CrmCampanas.AsNoTracking()
            .Select(x => new CrmReportCampaignProjection(
                x.Id,
                x.Nombre,
                x.TipoCampana,
                x.Objetivo,
                x.FechaInicio,
                x.FechaFin,
                x.Presupuesto,
                x.PresupuestoGastado,
                x.ResponsableId,
                x.LeadsGenerados,
                x.OportunidadesGeneradas,
                x.NegociosGanados,
                x.Activa))
            .ToListAsync(ct);
        campanas = campanas
            .Where(x => !campanaIdValue.HasValue || x.Id == campanaIdValue.Value)
            .Where(x => !responsableIdValue.HasValue || x.ResponsableId == responsableIdValue.Value)
            .Where(x => MatchesDateRange(x.FechaInicio, x.FechaFin, desde, hasta))
            .ToList();

        var lastInteractionByCliente = interacciones
            .GroupBy(x => x.ClienteId)
            .ToDictionary(x => x.Key, x => x.Max(i => i.FechaHora));
        var lastInteractionByOportunidad = interacciones
            .Where(x => x.OportunidadId.HasValue)
            .GroupBy(x => x.OportunidadId!.Value)
            .ToDictionary(x => x.Key, x => x.Max(i => i.FechaHora));

        var resumenComercial = new CrmResumenComercialResponse(
            clientes.Count(x => string.Equals(x.TipoCliente, "activo", StringComparison.OrdinalIgnoreCase)),
            oportunidades.Where(x => !IsClosedOpportunityStage(x.Etapa)).Sum(x => x.MontoEstimado),
            oportunidades.Count(x => !IsClosedOpportunityStage(x.Etapa) && x.FechaEstimadaCierre.HasValue && x.FechaEstimadaCierre.Value < today),
            clientes.Count(x => !lastInteractionByCliente.TryGetValue(x.Id, out var last) || last.UtcDateTime.Date < today.AddDays(-30).ToDateTime(TimeOnly.MinValue)));

        var pipelinePorEtapa = new[] { "lead", "calificado", "propuesta", "negociacion", "cerrado_ganado", "cerrado_perdido" }
            .Select(etapa => new CrmPipelineEtapaResponse(
                etapa,
                oportunidades.Count(x => string.Equals(x.Etapa, etapa, StringComparison.OrdinalIgnoreCase)),
                oportunidades.Where(x => string.Equals(x.Etapa, etapa, StringComparison.OrdinalIgnoreCase)).Sum(x => x.MontoEstimado)))
            .ToList();

        var distribucionProbabilidad = new List<CrmProbabilidadResponse>
        {
            new("0-25", oportunidades.Count(x => x.Probabilidad <= 25)),
            new("26-50", oportunidades.Count(x => x.Probabilidad > 25 && x.Probabilidad <= 50)),
            new("51-75", oportunidades.Count(x => x.Probabilidad > 50 && x.Probabilidad <= 75)),
            new("76-100", oportunidades.Count(x => x.Probabilidad > 75))
        };

        var rankingVendedores = usuarios.Values
            .Where(x => string.Equals(x.Rol, "comercial", StringComparison.OrdinalIgnoreCase) || string.Equals(x.Rol, "administrador", StringComparison.OrdinalIgnoreCase))
            .Select(usuario =>
            {
                var ganadas = oportunidades.Where(x => x.ResponsableId == usuario.Id && string.Equals(x.Etapa, "cerrado_ganado", StringComparison.OrdinalIgnoreCase)).ToList();
                var activas = oportunidades.Where(x => x.ResponsableId == usuario.Id && !IsClosedOpportunityStage(x.Etapa)).ToList();
                return new CrmRankingVendedorResponse(
                    usuario.Id.ToString(),
                    usuario.Nombre,
                    ganadas.Count,
                    ganadas.Sum(x => x.MontoEstimado),
                    activas.Count,
                    activas.Sum(x => x.MontoEstimado));
            })
            .OrderByDescending(x => x.MontoGanado)
            .ToList();

        var radarOportunidades = oportunidades
            .Where(x => !IsClosedOpportunityStage(x.Etapa))
            .Select(oportunidad =>
            {
                var cliente = clientes.FirstOrDefault(x => x.Id == oportunidad.ClienteId);
                var responsable = oportunidad.ResponsableId.HasValue && usuarios.TryGetValue(oportunidad.ResponsableId.Value, out var owner) ? owner.Nombre : "Sin asignar";
                lastInteractionByOportunidad.TryGetValue(oportunidad.Id, out var ultimaGestion);
                var riesgo = 0;
                if (oportunidad.FechaEstimadaCierre.HasValue && oportunidad.FechaEstimadaCierre.Value < today) riesgo += 3;
                if (ultimaGestion != default && ultimaGestion.UtcDateTime.Date < today.AddDays(-14).ToDateTime(TimeOnly.MinValue)) riesgo += 2;
                if (oportunidad.Probabilidad <= 40) riesgo += 1;
                if (oportunidad.MontoEstimado >= 10000m) riesgo += 1;

                return new CrmRadarOportunidadResponse(
                    oportunidad.Id.ToString(),
                    oportunidad.Titulo,
                    cliente?.Nombre ?? "Cliente no disponible",
                    responsable,
                    oportunidad.MontoEstimado,
                    oportunidad.FechaEstimadaCierre,
                    ultimaGestion == default ? null : ultimaGestion,
                    oportunidad.Origen,
                    riesgo);
            })
            .OrderByDescending(x => x.Riesgo)
            .ThenByDescending(x => x.MontoEstimado)
            .Take(8)
            .ToList();

        var radarClientes = clientes
            .Select(cliente =>
            {
                lastInteractionByCliente.TryGetValue(cliente.Id, out var ultimaGestion);
                var pipeline = oportunidades.Where(x => x.ClienteId == cliente.Id && !IsClosedOpportunityStage(x.Etapa)).Sum(x => x.MontoEstimado);
                var criticidad = 0;
                if (string.Equals(cliente.EstadoRelacion, "en_riesgo", StringComparison.OrdinalIgnoreCase)) criticidad += 3;
                if (ultimaGestion != default && ultimaGestion.UtcDateTime.Date < today.AddDays(-30).ToDateTime(TimeOnly.MinValue)) criticidad += 2;
                if (pipeline > 0) criticidad += 1;
                var responsable = cliente.ResponsableId.HasValue && usuarios.TryGetValue(cliente.ResponsableId.Value, out var owner) ? owner.Nombre : "Sin asignar";

                return new CrmRadarClienteResponse(
                    cliente.Id.ToString(),
                    cliente.Nombre,
                    responsable,
                    cliente.Segmento,
                    ultimaGestion == default ? null : ultimaGestion,
                    pipeline,
                    cliente.EstadoRelacion,
                    criticidad);
            })
            .Where(x => x.Criticidad > 0)
            .OrderByDescending(x => x.Criticidad)
            .ThenByDescending(x => x.Pipeline)
            .Take(8)
            .ToList();

        var resumenMarketing = new CrmResumenMarketingResponse(
            campanas.Count(x => x.Activa && x.FechaInicio <= today && x.FechaFin >= today),
            campanas.Sum(x => x.PresupuestoGastado) - campanas.Sum(x => x.Presupuesto),
            campanas.Sum(x => x.LeadsGenerados),
            campanas.Sum(x => x.LeadsGenerados) > 0 ? (decimal)campanas.Sum(x => x.NegociosGanados) * 100m / campanas.Sum(x => x.LeadsGenerados) : 0m);

        var clientesPorSegmento = clientes
            .GroupBy(x => x.Segmento)
            .Select(x => new CrmDistribucionResponse(x.Key, x.Count()))
            .OrderByDescending(x => x.Cantidad)
            .ToList();

        var clientesPorIndustria = clientes
            .Where(x => !string.IsNullOrWhiteSpace(x.Industria))
            .GroupBy(x => x.Industria!)
            .Select(x => new CrmDistribucionResponse(x.Key, x.Count()))
            .OrderByDescending(x => x.Cantidad)
            .ToList();

        var resultadosCampanas = campanas
            .OrderByDescending(x => x.LeadsGenerados)
            .ThenBy(x => x.Nombre)
            .Select(campana => new CrmResultadoCampanaResponse(
                campana.Id.ToString(),
                campana.Nombre,
                campana.TipoCampana,
                campana.Presupuesto,
                campana.PresupuestoGastado,
                campana.LeadsGenerados,
                campana.OportunidadesGeneradas,
                campana.NegociosGanados))
            .ToList();

        var radarCampanas = campanas
            .Select(campana =>
            {
                var responsable = campana.ResponsableId.HasValue && usuarios.TryGetValue(campana.ResponsableId.Value, out var owner) ? owner.Nombre : "Sin asignar";
                var costoPorLead = campana.LeadsGenerados > 0 ? campana.PresupuestoGastado / campana.LeadsGenerados : 0m;
                var tasaConversion = campana.LeadsGenerados > 0 ? (decimal)campana.NegociosGanados * 100m / campana.LeadsGenerados : 0m;
                return new CrmRadarCampanaResponse(
                    campana.Id.ToString(),
                    campana.Nombre,
                    campana.Objetivo,
                    responsable,
                    campana.FechaInicio,
                    campana.FechaFin,
                    campana.PresupuestoGastado - campana.Presupuesto,
                    costoPorLead,
                    campana.OportunidadesGeneradas,
                    tasaConversion);
            })
            .OrderByDescending(x => Math.Abs(x.Desvio))
            .ThenByDescending(x => x.CostoPorLead)
            .Take(8)
            .ToList();

        var actividadPorUsuario = usuarios.Values
            .Select(usuario =>
            {
                var interaccionesUsuario = interacciones.Where(x => x.UsuarioResponsableId == usuario.Id).ToList();
                return new CrmActividadUsuarioResponse(
                    usuario.Id.ToString(),
                    usuario.Nombre,
                    interaccionesUsuario.Count(x => string.Equals(x.TipoInteraccion, "llamada", StringComparison.OrdinalIgnoreCase)),
                    interaccionesUsuario.Count(x => string.Equals(x.TipoInteraccion, "email", StringComparison.OrdinalIgnoreCase)),
                    interaccionesUsuario.Count(x => string.Equals(x.TipoInteraccion, "reunion", StringComparison.OrdinalIgnoreCase)),
                    interaccionesUsuario.Count(x => string.Equals(x.TipoInteraccion, "visita", StringComparison.OrdinalIgnoreCase)),
                    interaccionesUsuario.Count);
            })
            .Where(x => x.Total > 0)
            .ToList();

        var actividadReciente = interacciones
            .OrderByDescending(x => x.FechaHora)
            .Take(10)
            .Select(interaccion => new CrmActividadRecienteResponse(
                interaccion.Id.ToString(),
                interaccion.FechaHora,
                interaccion.TipoInteraccion,
                interaccion.Canal,
                interaccion.Resultado,
                clientes.FirstOrDefault(x => x.Id == interaccion.ClienteId)?.Nombre ?? "Cliente no disponible",
                usuarios.TryGetValue(interaccion.UsuarioResponsableId, out var owner) ? owner.Nombre : "Sin asignar",
                interaccion.Descripcion))
            .ToList();

        return Ok(new CrmReportesResponse(
            resumenComercial,
            pipelinePorEtapa,
            distribucionProbabilidad,
            rankingVendedores,
            radarOportunidades,
            radarClientes,
            resumenMarketing,
            clientesPorSegmento,
            clientesPorIndustria,
            resultadosCampanas,
            radarCampanas,
            actividadPorUsuario,
            actividadReciente));
    }

    [HttpGet("usuarios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsuariosCrm(CancellationToken ct)
    {
        var result = await (
            from usuario in db.Usuarios.AsNoTracking()
            join perfil in db.CrmUsuariosPerfiles.AsNoTracking() on usuario.Id equals perfil.UsuarioId into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            orderby usuario.NombreCompleto, usuario.UserName
            select new
            {
                Id = usuario.Id.ToString(),
                NombreCompleto = usuario.NombreCompleto,
                usuario.UserName,
                usuario.Email,
                Rol = perfil != null ? perfil.Rol : "comercial",
                Estado = usuario.Activo ? "activo" : "inactivo",
                Avatar = perfil != null ? perfil.Avatar : null,
                CreatedAt = perfil != null ? perfil.CreatedAt : usuario.CreatedAt,
                UpdatedAt = perfil != null ? perfil.UpdatedAt : usuario.UpdatedAt
            }).ToListAsync(ct);

        var mapped = result.Select(x =>
        {
            var (nombre, apellido) = SplitFullName(x.NombreCompleto, x.UserName);
            return new CrmUsuarioResponse(x.Id, nombre, apellido, x.Email, x.Rol, x.Estado, x.Avatar, x.CreatedAt, x.UpdatedAt);
        }).ToList();

        return Ok(mapped);
    }

    [HttpGet("usuarios/{id:long}", Name = "GetCrmUsuarioById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsuarioCrmById(long id, CancellationToken ct)
    {
        var result = await GetCrmUsuarioPayloadAsync(id, ct);
        return OkOrNotFound(result);
    }

    [HttpPost("usuarios")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUsuarioCrm([FromBody] CrmUsuarioRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateCrmUsuarioCommand(req.Nombre, req.Apellido, req.Email, req.Rol, req.Estado, req.Avatar), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var created = await GetCrmUsuarioPayloadAsync(result.Value, ct);
        return StatusCode(StatusCodes.Status201Created, created is not null ? created : new { Id = result.Value.ToString() });
    }

    [HttpPut("usuarios/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUsuarioCrm(long id, [FromBody] CrmUsuarioRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateCrmUsuarioCommand(id, req.Nombre, req.Apellido, req.Email, req.Rol, req.Estado, req.Avatar), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var updated = await GetCrmUsuarioPayloadAsync(id, ct);
        return Ok(updated is not null ? updated : new { Id = id.ToString() });
    }

    [HttpDelete("usuarios/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUsuarioCrm(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCrmUsuarioCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    private async Task<CrmClienteResponse?> GetClientePayloadAsync(long id, CancellationToken ct)
        => await (
            from perfil in db.CrmClientes.AsNoTracking()
            join tercero in db.Terceros.AsNoTracking() on perfil.TerceroId equals tercero.Id
            where perfil.Activo && perfil.TerceroId == id
            select new CrmClienteResponse(
                tercero.Id.ToString(),
                tercero.RazonSocial,
                perfil.TipoCliente,
                perfil.Segmento,
                perfil.Industria,
                tercero.NroDocumento != null && tercero.NroDocumento.StartsWith("CRM-", StringComparison.OrdinalIgnoreCase) ? null : tercero.NroDocumento,
                perfil.Pais,
                perfil.Provincia,
                perfil.Ciudad,
                perfil.Direccion,
                tercero.Telefono,
                tercero.Email,
                tercero.Web,
                perfil.OrigenCliente,
                perfil.EstadoRelacion,
                perfil.ResponsableId.HasValue ? perfil.ResponsableId.Value.ToString() : null,
                DateOnly.FromDateTime(tercero.CreatedAt.UtcDateTime),
                perfil.NotasGenerales,
                perfil.CreatedAt,
                perfil.UpdatedAt)).FirstOrDefaultAsync(ct);

    private Task<CrmContactoResponse?> GetCrmContactoPayloadAsync(long id, CancellationToken ct)
        => db.CrmContactos.AsNoTracking()
            .Where(x => x.Id == id && x.Activo)
            .Select(x => new CrmContactoResponse(x.Id.ToString(), x.ClienteId.ToString(), x.Nombre, x.Apellido, x.Cargo, x.Email, x.Telefono, x.CanalPreferido, x.EstadoContacto, x.Notas, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

    private Task<CrmOportunidadResponse?> GetCrmOportunidadPayloadAsync(long id, CancellationToken ct)
        => db.CrmOportunidades.AsNoTracking()
            .Where(x => x.Id == id && x.Activa)
            .Select(x => new CrmOportunidadResponse(x.Id.ToString(), x.ClienteId.ToString(), x.ContactoPrincipalId.HasValue ? x.ContactoPrincipalId.Value.ToString() : null, x.Titulo, x.Etapa, x.Probabilidad, x.MontoEstimado, x.Moneda, x.FechaApertura, x.FechaEstimadaCierre, x.ResponsableId.HasValue ? x.ResponsableId.Value.ToString() : null, x.Origen, x.MotivoPerdida, x.Notas, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

    private Task<CrmInteraccionResponse?> GetCrmInteraccionPayloadAsync(long id, CancellationToken ct)
        => db.CrmInteracciones.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CrmInteraccionResponse(x.Id.ToString(), x.ClienteId.ToString(), x.ContactoId.HasValue ? x.ContactoId.Value.ToString() : null, x.OportunidadId.HasValue ? x.OportunidadId.Value.ToString() : null, x.TipoInteraccion, x.Canal, x.FechaHora, x.UsuarioResponsableId.ToString(), x.Resultado, x.Descripcion, DeserializeStringList(x.AdjuntosJson), x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

    private Task<CrmTareaResponse?> GetCrmTareaPayloadAsync(long id, CancellationToken ct)
        => db.CrmTareas.AsNoTracking()
            .Where(x => x.Id == id && x.Activa)
            .Select(x => new CrmTareaResponse(x.Id.ToString(), x.ClienteId.HasValue ? x.ClienteId.Value.ToString() : null, x.OportunidadId.HasValue ? x.OportunidadId.Value.ToString() : null, x.AsignadoAId.ToString(), x.Titulo, x.Descripcion, x.TipoTarea, x.FechaVencimiento, x.Prioridad, x.Estado, x.FechaCompletado, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(ct);

    private async Task<CrmSegmentoResponse?> GetCrmSegmentoPayloadAsync(long id, CancellationToken ct)
    {
        var segmento = await db.CrmSegmentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Activo, ct);
        if (segmento is null)
            return null;

        var criterios = DeserializeSegmentCriteria(segmento.CriteriosJson);
        var miembros = await ResolveSegmentMembersAsync(segmento, ct);

        return new CrmSegmentoResponse(segmento.Id.ToString(), segmento.Nombre, segmento.Descripcion, criterios, segmento.TipoSegmento, miembros.Count, segmento.CreatedAt, segmento.UpdatedAt);
    }

    private async Task<List<CrmClientSegmentProjection>> ResolveSegmentMembersAsync(DomainCrmSegmento segmento, CancellationToken ct)
    {
        var clientes = await LoadCrmClientProjectionsAsync(ct);
        if (segmento.EsEstatico())
        {
            var clienteIds = await db.CrmSegmentosMiembros.AsNoTracking()
                .Where(x => x.SegmentoId == segmento.Id && x.Activo)
                .Select(x => x.ClienteId)
                .ToListAsync(ct);

            return clientes
                .Where(x => clienteIds.Contains(x.Id))
                .OrderBy(x => x.Nombre)
                .ToList();
        }

        return ResolveDynamicSegmentMembers(clientes, DeserializeSegmentCriteria(segmento.CriteriosJson));
    }

    private static List<CrmClientSegmentProjection> ResolveDynamicSegmentMembers(
        IReadOnlyCollection<CrmClientSegmentProjection> clientes,
        IReadOnlyList<CrmSegmentCriterionRequest>? criterios)
    {
        var criteria = criterios ?? [];
        var query = criteria.Count == 0
            ? clientes
            : clientes.Where(cliente => criteria.All(criterion => MatchesCriterion(cliente, criterion))).ToList();

        return query.OrderBy(x => x.Nombre).ToList();
    }

    private static CrmSegmentoMiembroResponse MapSegmentClient(CrmClientSegmentProjection cliente)
        => new(
            cliente.Id.ToString(),
            cliente.Nombre,
            cliente.TipoCliente,
            cliente.Segmento,
            cliente.Industria,
            cliente.OrigenCliente,
            cliente.EstadoRelacion,
            cliente.Pais,
            cliente.Provincia,
            cliente.Ciudad);

    private async Task<List<CrmReportClientProjection>> LoadCrmReportClientsAsync(CancellationToken ct)
        => await (
            from perfil in db.CrmClientes.AsNoTracking()
            join tercero in db.Terceros.AsNoTracking() on perfil.TerceroId equals tercero.Id
            where perfil.Activo
            select new CrmReportClientProjection(
                perfil.TerceroId,
                tercero.RazonSocial,
                perfil.TipoCliente,
                perfil.Segmento,
                perfil.Industria,
                perfil.OrigenCliente,
                perfil.EstadoRelacion,
                perfil.ResponsableId))
            .ToListAsync(ct);

    private async Task<Dictionary<long, CrmReportUserProjection>> LoadCrmReportUsersAsync(CancellationToken ct)
    {
        var users = await (
            from usuario in db.Usuarios.AsNoTracking()
            join perfil in db.CrmUsuariosPerfiles.AsNoTracking() on usuario.Id equals perfil.UsuarioId into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            select new
            {
                usuario.Id,
                usuario.NombreCompleto,
                usuario.UserName,
                Rol = perfil != null ? perfil.Rol : "comercial"
            })
            .ToListAsync(ct);

        return users.ToDictionary(
            x => x.Id,
            x =>
            {
                var (nombre, apellido) = SplitFullName(x.NombreCompleto, x.UserName);
                return new CrmReportUserProjection(x.Id, string.IsNullOrWhiteSpace(apellido) ? nombre : $"{nombre} {apellido}", x.Rol);
            });
    }

    private static bool IsClosedOpportunityStage(string etapa)
        => string.Equals(etapa, "cerrado_ganado", StringComparison.OrdinalIgnoreCase)
            || string.Equals(etapa, "cerrado_perdido", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<CrmCatalogOptionResponse> MapCatalogOptions(params string[] values)
        => values.Select(value => new CrmCatalogOptionResponse(value, value)).ToList();

    private static bool MatchesDateRange(DateTimeOffset value, DateOnly? desde, DateOnly? hasta)
    {
        var date = DateOnly.FromDateTime(value.UtcDateTime.Date);
        return (!desde.HasValue || date >= desde.Value)
            && (!hasta.HasValue || date <= hasta.Value);
    }

    private static bool MatchesDateRange(DateOnly fechaInicio, DateOnly fechaFin, DateOnly? desde, DateOnly? hasta)
        => (!desde.HasValue || fechaFin >= desde.Value)
            && (!hasta.HasValue || fechaInicio <= hasta.Value);

    private async Task<CrmUsuarioResponse?> GetCrmUsuarioPayloadAsync(long id, CancellationToken ct)
    {
        var result = await (
            from usuario in db.Usuarios.AsNoTracking()
            join perfil in db.CrmUsuariosPerfiles.AsNoTracking() on usuario.Id equals perfil.UsuarioId into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where usuario.Id == id
            select new
            {
                Id = usuario.Id.ToString(),
                NombreCompleto = usuario.NombreCompleto,
                usuario.UserName,
                usuario.Email,
                Rol = perfil != null ? perfil.Rol : "comercial",
                Estado = usuario.Activo ? "activo" : "inactivo",
                Avatar = perfil != null ? perfil.Avatar : null,
                CreatedAt = perfil != null ? perfil.CreatedAt : usuario.CreatedAt,
                UpdatedAt = perfil != null ? perfil.UpdatedAt : usuario.UpdatedAt
            }).FirstOrDefaultAsync(ct);

        if (result is null)
            return null;

        var (nombre, apellido) = SplitFullName(result.NombreCompleto, result.UserName);
        return new CrmUsuarioResponse(result.Id, nombre, apellido, result.Email, result.Rol, result.Estado, result.Avatar, result.CreatedAt, result.UpdatedAt);
    }

    private Task<CrmCampanaResponse?> GetCampanaPayloadAsync(long id, CancellationToken ct)
        => db.CrmCampanas.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CrmCampanaResponse(x.Id.ToString(), x.SucursalId, x.Nombre, x.TipoCampana, x.Objetivo, x.SegmentoObjetivoId.HasValue ? x.SegmentoObjetivoId.Value.ToString() : null, x.FechaInicio, x.FechaFin, x.Presupuesto ?? 0m, x.PresupuestoGastado ?? 0m, x.ResponsableId.HasValue ? x.ResponsableId.Value.ToString() : null, string.IsNullOrWhiteSpace(x.Notas) ? x.Descripcion : x.Notas, x.LeadsGenerados, x.OportunidadesGeneradas, x.NegociosGanados, x.CreatedAt, x.UpdatedAt, x.Activa))
            .FirstOrDefaultAsync(ct);

    private async Task<long?> ResolveCampanaSucursalIdAsync(long? sucursalId, CancellationToken ct)
    {
        if (sucursalId.HasValue && sucursalId.Value > 0)
            return sucursalId.Value;

        return await db.Sucursales.AsNoTracking()
            .Where(x => x.Activa)
            .OrderByDescending(x => x.CasaMatriz)
            .ThenBy(x => x.Id)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    private static long? ParseNullableLong(string? value)
        => long.TryParse(value, out var parsed) && parsed > 0 ? parsed : null;

    private static IReadOnlyList<string> DeserializeStringList(string? json)
        => JsonSerializer.Deserialize<IReadOnlyList<string>>(string.IsNullOrWhiteSpace(json) ? "[]" : json) ?? [];

    private static IReadOnlyList<CrmSegmentCriterionRequest> DeserializeSegmentCriteria(string? json)
        => JsonSerializer.Deserialize<IReadOnlyList<CrmSegmentCriterionRequest>>(string.IsNullOrWhiteSpace(json) ? "[]" : json) ?? [];

    private static string SerializeSegmentCriteria(IReadOnlyList<CrmSegmentCriterionRequest>? criteria)
        => JsonSerializer.Serialize(criteria ?? []);

    private static bool IsMissingLegacyContactoRelation(DbException exception)
    {
        const string undefinedTableSqlState = "42P01";
        const string relationName = "CONTACTOS";

        var sqlState = exception.GetType().GetProperty("SqlState")?.GetValue(exception) as string;
        return (string.Equals(sqlState, undefinedTableSqlState, StringComparison.Ordinal)
                || exception.Message.Contains(undefinedTableSqlState, StringComparison.Ordinal))
            && exception.Message.Contains(relationName, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<List<CrmClientSegmentProjection>> LoadCrmClientProjectionsAsync(CancellationToken ct)
        => await (
            from perfil in db.CrmClientes.AsNoTracking()
            join tercero in db.Terceros.AsNoTracking() on perfil.TerceroId equals tercero.Id
            where perfil.Activo
            select new CrmClientSegmentProjection(
                perfil.TerceroId,
                tercero.RazonSocial,
                perfil.TipoCliente,
                perfil.Segmento,
                perfil.Industria,
                perfil.OrigenCliente,
                perfil.EstadoRelacion,
                perfil.Pais,
                perfil.Provincia,
                perfil.Ciudad))
            .ToListAsync(ct);

    private static bool MatchesCriterion(CrmClientSegmentProjection client, CrmSegmentCriterionRequest criterion)
    {
        var value = criterion.Valor?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var clientValue = criterion.Campo.Trim().ToLowerInvariant() switch
        {
            "nombre" => client.Nombre,
            "tipocliente" => client.TipoCliente,
            "segmento" => client.Segmento,
            "industria" => client.Industria,
            "origencliente" => client.OrigenCliente,
            "estadorelacion" => client.EstadoRelacion,
            "pais" => client.Pais,
            "provincia" => client.Provincia,
            "ciudad" => client.Ciudad,
            _ => null
        };

        if (clientValue is null)
            return false;

        return criterion.Operador.Trim().ToLowerInvariant() switch
        {
            "igual" => string.Equals(clientValue, value, StringComparison.OrdinalIgnoreCase),
            "contiene" => clientValue.Contains(value, StringComparison.OrdinalIgnoreCase),
            "mayor_que" => string.Compare(clientValue, value, StringComparison.OrdinalIgnoreCase) > 0,
            "menor_que" => string.Compare(clientValue, value, StringComparison.OrdinalIgnoreCase) < 0,
            "entre" => clientValue.Contains(value, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static (string nombre, string apellido) SplitFullName(string? nombreCompleto, string fallback)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            return (fallback, string.Empty);

        var parts = nombreCompleto.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1 ? (parts[0], string.Empty) : (parts[0], parts[1]);
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record ContactoCrmRequest(long PersonaId, long PersonaContactoId, long? TipoRelacionId = null);
public record ContactoCrmUpdateRequest(long? TipoRelacionId = null);
public record CrmCatalogoCreateRequest(string Codigo, string Descripcion);
public record CrmCatalogoUpdateRequest(string Descripcion);
public record CrmCampanaRequest(
    long? SucursalId,
    string Nombre,
    string TipoCampana,
    string Objetivo,
    string? SegmentoObjetivoId,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFin,
    decimal PresupuestoEstimado,
    decimal PresupuestoGastado,
    string? ResponsableId,
    string? Notas,
    int LeadsGenerados,
    int OportunidadesGeneradas,
    int NegociosGanados);
public record CrmCampanaUpdateRequest(
    string Nombre,
    string TipoCampana,
    string Objetivo,
    string? SegmentoObjetivoId,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFin,
    decimal PresupuestoEstimado,
    decimal PresupuestoGastado,
    string? ResponsableId,
    string? Notas,
    int LeadsGenerados,
    int OportunidadesGeneradas,
    int NegociosGanados);
public record CrmComunicadoRequest(long SucursalId, long TerceroId, long? CampanaId, long? TipoId, DateOnly Fecha, string Asunto, string? Contenido, long? UsuarioId);
public record CrmComunicadoUpdateRequest(string Asunto, string? Contenido);
public record CrmSeguimientoRequest(long SucursalId, long TerceroId, long? MotivoId, long? InteresId, long? CampanaId, DateOnly Fecha, string Descripcion, DateOnly? ProximaAccion, long? UsuarioId);
public record CrmSeguimientoUpdateRequest(string Descripcion, DateOnly? ProximaAccion);
public record CrmClienteUpsertRequest(
    string Nombre,
    string TipoCliente,
    string Segmento,
    string? Industria,
    string? Cuit,
    string Pais,
    string? Provincia,
    string? Ciudad,
    string? Direccion,
    string? TelefonoPrincipal,
    string? EmailPrincipal,
    string? SitioWeb,
    string OrigenCliente,
    string EstadoRelacion,
    string? ResponsableId,
    DateTimeOffset? FechaAlta,
    string? NotasGenerales);
public record CrmContactoRequest(
    string ClienteId,
    string Nombre,
    string Apellido,
    string? Cargo,
    string? Email,
    string? Telefono,
    string CanalPreferido,
    string EstadoContacto,
    string? Notas);
public record CrmOportunidadRequest(
    string ClienteId,
    string? ContactoPrincipalId,
    string Titulo,
    string Etapa,
    int Probabilidad,
    decimal MontoEstimado,
    string Moneda,
    DateTimeOffset FechaApertura,
    DateTimeOffset? FechaEstimadaCierre,
    string? ResponsableId,
    string Origen,
    string? MotivoPerdida,
    string? Notas);
public record CrmOportunidadPerdidaRequest(string MotivoPerdida);
public record CrmReassignRequest(string ResponsableId);
public record CrmInteraccionRequest(
    string ClienteId,
    string? ContactoId,
    string? OportunidadId,
    string TipoInteraccion,
    string Canal,
    DateTimeOffset FechaHora,
    string UsuarioResponsableId,
    string Resultado,
    string? Descripcion,
    IReadOnlyList<string>? Adjuntos);
public record CrmTareaRequest(
    string? ClienteId,
    string? OportunidadId,
    string AsignadoAId,
    string Titulo,
    string? Descripcion,
    string TipoTarea,
    DateTimeOffset FechaVencimiento,
    string Prioridad,
    string Estado,
    DateTimeOffset? FechaCompletado);
public record CrmCompleteTaskRequest(DateTimeOffset? FechaCompletado);
public record CrmSegmentoRequest(
    string Nombre,
    string? Descripcion,
    IReadOnlyList<CrmSegmentCriterionRequest>? Criterios,
    string TipoSegmento);
public record CrmSegmentoPreviewRequest(IReadOnlyList<CrmSegmentCriterionRequest>? Criterios, string TipoSegmento);
public record CrmSegmentCriterionRequest(string Campo, string Operador, string? Valor);
public record CrmSegmentoMiembroRequest(string ClienteId);
public record CrmUsuarioRequest(string Nombre, string Apellido, string Email, string Rol, string Estado, string? Avatar);
sealed record CrmClientSegmentProjection(
    long Id,
    string Nombre,
    string TipoCliente,
    string Segmento,
    string? Industria,
    string OrigenCliente,
    string EstadoRelacion,
    string Pais,
    string? Provincia,
    string? Ciudad);
sealed record CrmReportClientProjection(
    long Id,
    string Nombre,
    string TipoCliente,
    string Segmento,
    string? Industria,
    string OrigenCliente,
    string EstadoRelacion,
    long? ResponsableId);
sealed record CrmReportUserProjection(long Id, string Nombre, string Rol);
sealed record CrmReportOpportunityProjection(
    long Id,
    long ClienteId,
    string Titulo,
    string Etapa,
    int Probabilidad,
    decimal MontoEstimado,
    string Moneda,
    DateOnly? FechaEstimadaCierre,
    long? ResponsableId,
    string Origen);
sealed record CrmReportInteractionProjection(
    long Id,
    long ClienteId,
    long? OportunidadId,
    string TipoInteraccion,
    string Canal,
    DateTimeOffset FechaHora,
    long UsuarioResponsableId,
    string Resultado,
    string? Descripcion);
sealed record CrmReportCampaignProjection(
    long Id,
    string Nombre,
    string TipoCampana,
    string Objetivo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    decimal? RawPresupuesto,
    decimal? RawPresupuestoGastado,
    long? ResponsableId,
    int LeadsGenerados,
    int OportunidadesGeneradas,
    int NegociosGanados,
    bool Activa)
{
    public decimal Presupuesto => RawPresupuesto ?? 0m;
    public decimal PresupuestoGastado => RawPresupuestoGastado ?? 0m;
}

