using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Api.Controllers;

public class TercerosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    // ─────────────────────────────────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Listado paginado de terceros con filtros combinables.
    /// Equivalente a la grilla del ABM de Clientes/Proveedores del VB6.
    /// GET /api/terceros?page=1&pageSize=20&search=garcia&soloClientes=true
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? soloClientes = null,
        [FromQuery] bool? soloProveedores = null,
        [FromQuery] bool? soloEmpleados = null,
        [FromQuery] bool? soloActivos = true,
        [FromQuery] long? condicionIvaId = null,
        [FromQuery] long? categoriaId = null,
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetTercerosPagedQuery(
                page,
                pageSize,
                search,
                soloClientes,
                soloProveedores,
                soloEmpleados,
                soloActivos,
                condicionIvaId,
                categoriaId,
                sucursalId),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Detalle completo de un tercero por Id.
    /// Equivalente a abrir el formulario de ABM al hacer doble click en una fila.
    /// GET /api/terceros/42
    /// </summary>
    [HttpGet("{id:long}", Name = "GetTerceroById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroByIdQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Busca un tercero por su legajo (identificador de negocio).
    /// Equivalente a la búsqueda rápida por legajo en el VB6.
    /// GET /api/terceros/legajo/CLI001
    /// </summary>
    [HttpGet("legajo/{legajo}", Name = "GetTerceroByLegajo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLegajo(string legajo, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroByLegajoQuery(legajo), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Lista de clientes activos para combos y selectores.
    /// Equivalente al llenarComboClientes() del VB6.
    /// GET /api/terceros/clientes-activos?sucursalId=1
    /// </summary>
    [HttpGet("clientes-activos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientesActivos(
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetClientesActivosQuery(sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lista de proveedores activos para combos y selectores.
    /// Equivalente al llenarComboProveedores() del VB6.
    /// GET /api/terceros/proveedores-activos?sucursalId=1
    /// </summary>
    [HttpGet("proveedores-activos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProveedoresActivos(
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetProveedoresActivosQuery(sucursalId), ct);
        return Ok(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COMMANDS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuevo tercero.
    /// Equivalente al agregarNuevo() → Guardar() del VB6.
    /// POST /api/terceros
    /// Retorna 201 Created con header Location: /api/terceros/{id}
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTerceroCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(
            result,
            "GetTerceroById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza los datos de un tercero existente.
    /// Equivalente al Guardar() en modo edición del VB6.
    /// PUT /api/terceros/42
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTerceroCommand command,
        CancellationToken ct)
    {
        // Guardia: el Id de la URL debe coincidir con el del body.
        // Patrón del proyecto (ver ItemsController).
        if (id != command.Id)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Da de baja lógica (soft delete) a un tercero.
    /// Equivalente al eliminar() + validarEliminar() del VB6.
    /// DELETE /api/terceros/42
    /// Retorna 400 con mensaje si tiene comprobantes, cuenta corriente o empleado activo.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reactiva un tercero dado de baja lógica.
    /// No existía en VB6 (se hacía manual en BD),
    /// pero es necesario para el flujo de administración.
    /// PATCH /api/terceros/42/activar
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivarTerceroCommand(id), ct);
        return FromResult(result);
    }

    // ─── Domicilios ──────────────────────────────────────────────────────────

    /// <summary>Retorna los domicilios de un tercero. VB6: clsDomicilio / PER_DOMICILIO.</summary>
    [HttpGet("{id:long}/domicilios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDomicilios(long id, CancellationToken ct)
    {
        var lista = await db.Domicilios
            .AsNoTracking()
            .Where(d => d.PersonaId == id)
            .OrderBy(d => d.Orden)
            .Select(d => new { d.Id, d.TipoDomicilioId, d.ProvinciaId, d.LocalidadId,
                               d.Calle, d.Barrio, d.CodigoPostal, d.Observacion, d.Orden, d.EsDefecto })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Agrega un domicilio al tercero.</summary>
    [HttpPost("{id:long}/domicilios")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddDomicilio(long id, [FromBody] DomicilioRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddTerceroDomicilioCommand(
                id,
                req.TipoDomicilioId,
                req.ProvinciaId,
                req.LocalidadId,
                req.Calle,
                req.Barrio,
                req.CodigoPostal,
                req.Observacion,
                req.Orden,
                req.EsDefecto),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetDomicilios), new { id }, new { Id = result.Value });
    }

    /// <summary>Actualiza un domicilio del tercero.</summary>
    [HttpPut("{id:long}/domicilios/{domId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDomicilio(long id, long domId, [FromBody] DomicilioRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateTerceroDomicilioCommand(
                id,
                domId,
                req.TipoDomicilioId,
                req.ProvinciaId,
                req.LocalidadId,
                req.Calle,
                req.Barrio,
                req.CodigoPostal,
                req.Observacion,
                req.Orden,
                req.EsDefecto),
            ct);

        return FromNestedResult(result, new { Id = domId });
    }

    /// <summary>Elimina un domicilio del tercero.</summary>
    [HttpDelete("{id:long}/domicilios/{domId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDomicilio(long id, long domId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroDomicilioCommand(id, domId), ct);
        return FromNestedResult(result);
    }

    // ─── Medios de Contacto ───────────────────────────────────────────────────

    /// <summary>Retorna los medios de contacto de un tercero. VB6: clsMedioContacto / PER_MEDIOCONTACTO.</summary>
    [HttpGet("{id:long}/medios-contacto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMediosContacto(long id, CancellationToken ct)
    {
        var lista = await db.MediosContacto
            .AsNoTracking()
            .Where(m => m.PersonaId == id)
            .OrderBy(m => m.Orden)
            .Select(m => new { m.Id, m.TipoMedioContactoId, m.Valor, m.Orden, m.EsDefecto, m.Observacion })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Agrega un medio de contacto al tercero.</summary>
    [HttpPost("{id:long}/medios-contacto")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddMedioContacto(long id, [FromBody] MedioContactoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddTerceroMedioContactoCommand(
                id,
                req.Valor,
                req.TipoMedioContactoId,
                req.Orden,
                req.EsDefecto,
                req.Observacion),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetMediosContacto), new { id }, new { Id = result.Value });
    }

    /// <summary>Actualiza un medio de contacto del tercero.</summary>
    [HttpPut("{id:long}/medios-contacto/{mconId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMedioContacto(long id, long mconId, [FromBody] MedioContactoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateTerceroMedioContactoCommand(
                id,
                mconId,
                req.Valor,
                req.TipoMedioContactoId,
                req.Orden,
                req.EsDefecto,
                req.Observacion),
            ct);

        return FromNestedResult(result, new { Id = mconId });
    }

    /// <summary>Elimina un medio de contacto del tercero.</summary>
    [HttpDelete("{id:long}/medios-contacto/{mconId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedioContacto(long id, long mconId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroMedioContactoCommand(id, mconId), ct);
        return FromNestedResult(result);
    }

    // ─── Tipos de Persona ─────────────────────────────────────────────────────

    /// <summary>Retorna los tipos de persona asignados al tercero. VB6: clsPersonaXTipoPersona / PER_PERSONAxTIPOPERSONA.</summary>
    [HttpGet("{id:long}/tipos-persona")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposPersona(long id, CancellationToken ct)
    {
        var lista = await db.PersonasXTipoPersona
            .AsNoTracking()
            .Where(p => p.PersonaId == id)
            .Select(p => new { p.Id, p.TipoPersonaId, p.Legajo, p.LegajoOrden })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Asigna un tipo de persona al tercero.</summary>
    [HttpPost("{id:long}/tipos-persona")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTipoPersona(long id, [FromBody] PersonaXTipoPersonaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddTerceroTipoPersonaCommand(id, req.TipoPersonaId, req.Legajo, req.LegajoOrden),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetTiposPersona), new { id }, new { Id = result.Value });
    }

    /// <summary>Elimina un tipo de persona del tercero.</summary>
    [HttpDelete("{id:long}/tipos-persona/{pxtpId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTipoPersona(long id, long pxtpId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroTipoPersonaCommand(id, pxtpId), ct);
        return FromNestedResult(result);
    }

    // ─── Vinculaciones ────────────────────────────────────────────────────────

    /// <summary>Retorna las vinculaciones del tercero (como cliente). VB6: clsVinculacionPersona / PER_VINCULACIONPERSONA.</summary>
    [HttpGet("{id:long}/vinculaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVinculaciones(long id, CancellationToken ct)
    {
        var lista = await db.VinculacionesPersona
            .AsNoTracking()
            .Where(v => v.ClienteId == id || v.ProveedorId == id)
            .Select(v => new { v.Id, v.ClienteId, v.ProveedorId, v.EsPredeterminado, v.TipoRelacionId })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Crea una vinculación para el tercero.</summary>
    [HttpPost("{id:long}/vinculaciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddVinculacion(long id, [FromBody] VinculacionPersonaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddTerceroVinculacionCommand(id, req.ClienteId, req.ProveedorId, req.EsPredeterminado, req.TipoRelacionId),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetVinculaciones), new { id }, new { Id = result.Value });
    }

    /// <summary>Elimina una vinculación del tercero.</summary>
    [HttpDelete("{id:long}/vinculaciones/{vincId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVinculacion(long id, long vincId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroVinculacionCommand(id, vincId), ct);
        return FromNestedResult(result);
    }

    // ─── Contactos CRM ────────────────────────────────────────────────────────

    /// <summary>Retorna las relaciones/contactos del tercero. VB6: clsRelacion / CONTACTOS.</summary>
    [HttpGet("{id:long}/contactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContactos(long id, CancellationToken ct)
    {
        var lista = await db.Contactos
            .AsNoTracking()
            .Where(c => c.PersonaId == id)
            .Select(c => new { c.Id, c.PersonaContactoId, c.TipoRelacionId })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Agrega un contacto al tercero.</summary>
    [HttpPost("{id:long}/contactos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddContacto(long id, [FromBody] ContactoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddTerceroContactoCommand(id, req.PersonaContactoId, req.TipoRelacionId), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetContactos), new { id }, new { Id = result.Value });
    }

    /// <summary>Elimina un contacto del tercero.</summary>
    [HttpDelete("{id:long}/contactos/{contactoId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContacto(long id, long contactoId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroContactoCommand(id, contactoId), ct);
        return FromNestedResult(result);
    }

    private IActionResult FromNestedResult(Result result, object? okPayload = null)
    {
        if (result.IsSuccess)
            return okPayload is null ? Ok() : Ok(okPayload);

        var isNotFound = result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
            || result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true;

        return isNotFound
            ? NotFound(new { error = result.Error })
            : BadRequest(new { error = result.Error });
    }
}

// ─── Request records ──────────────────────────────────────────────────────────
public record DomicilioRequest(
    long? TipoDomicilioId, long? ProvinciaId, long? LocalidadId,
    string? Calle, string? Barrio, string? CodigoPostal, string? Observacion,
    int Orden = 0, bool EsDefecto = false);

public record MedioContactoRequest(
    string Valor, long? TipoMedioContactoId = null,
    int Orden = 0, bool EsDefecto = false, string? Observacion = null);

public record PersonaXTipoPersonaRequest(long TipoPersonaId, string? Legajo = null, int? LegajoOrden = null);

public record VinculacionPersonaRequest(long ClienteId, long ProveedorId, bool EsPredeterminado = false, long? TipoRelacionId = null);

public record ContactoRequest(long PersonaContactoId, long? TipoRelacionId = null);