using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.RRHH.Commands;
using ZuluIA_Back.Application.Features.RRHH.DTOs;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class EmpleadosController(
    IMediator mediator,
    IEmpleadoRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna empleados paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        EstadoEmpleado? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoEmpleado>(estado, true, out var parsed))
            estadoEnum = parsed;

        var result = await repo.GetPagedAsync(
            page, pageSize, sucursalId, estadoEnum, search, ct);

        var terceroIds = result.Items.Select(x => x.TerceroId).Distinct().ToList();
        var monedaIds = result.Items.Select(x => x.MonedaId).Distinct().ToList();

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.NroDocumento })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(e => new EmpleadoDto
        {
            Id                 = e.Id,
            TerceroId          = e.TerceroId,
            TerceroRazonSocial = terceros.GetValueOrDefault(e.TerceroId)?.RazonSocial   ?? "—",
            TerceroCuit        = terceros.GetValueOrDefault(e.TerceroId)?.NroDocumento  ?? "—",
            SucursalId         = e.SucursalId,
            Legajo             = e.Legajo,
            Cargo              = e.Cargo,
            Area               = e.Area,
            FechaIngreso       = e.FechaIngreso,
            FechaEgreso        = e.FechaEgreso,
            SueldoBasico       = e.SueldoBasico,
            MonedaId           = e.MonedaId,
            MonedaSimbolo      = monedas.GetValueOrDefault(e.MonedaId)?.Simbolo ?? "$",
            Estado             = e.Estado.ToString().ToUpperInvariant()
        }).ToList();

        return Ok(new
        {
            data = dtos,
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    /// <summary>
    /// Retorna el detalle de un empleado con sus liquidaciones.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetEmpleadoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var emp = await repo.GetByIdAsync(id, ct);
        if (emp is null)
            return NotFound(new { error = $"No se encontró el empleado con ID {id}." });

        var tercero = await db.Terceros.AsNoTracking()
            .Where(x => x.Id == emp.TerceroId)
            .Select(x => new { x.RazonSocial, x.NroDocumento })
            .FirstOrDefaultAsync(ct);

        var moneda = await db.Monedas.AsNoTracking()
            .Where(x => x.Id == emp.MonedaId)
            .Select(x => new { x.Simbolo })
            .FirstOrDefaultAsync(ct);

        var terceroRazonSocial = tercero?.RazonSocial ?? "—";
        var monedaSimbolo = moneda?.Simbolo ?? "$";

        var liquidaciones = await db.LiquidacionesSueldo.AsNoTracking()
            .Where(x => x.EmpleadoId == id)
            .OrderByDescending(x => x.Anio)
            .ThenByDescending(x => x.Mes)
            .Take(12)
            .Select(x => new LiquidacionSueldoDto
            {
                Id              = x.Id,
                EmpleadoId      = x.EmpleadoId,
                EmpleadoLegajo  = emp.Legajo,
                EmpleadoNombre  = terceroRazonSocial,
                SucursalId      = x.SucursalId,
                Anio            = x.Anio,
                Mes             = x.Mes,
                Periodo         = $"{x.Anio}/{x.Mes:D2}",
                SueldoBasico    = x.SueldoBasico,
                TotalHaberes    = x.TotalHaberes,
                TotalDescuentos = x.TotalDescuentos,
                Neto            = x.Neto,
                MonedaId        = x.MonedaId,
                MonedaSimbolo   = monedaSimbolo,
                Pagada          = x.Pagada,
                Observacion     = x.Observacion,
                CreatedAt       = x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(new EmpleadoDto
        {
            Id                 = emp.Id,
            TerceroId          = emp.TerceroId,
            TerceroRazonSocial = tercero?.RazonSocial  ?? "—",
            TerceroCuit        = tercero?.NroDocumento ?? "—",
            SucursalId         = emp.SucursalId,
            Legajo             = emp.Legajo,
            Cargo              = emp.Cargo,
            Area               = emp.Area,
            FechaIngreso       = emp.FechaIngreso,
            FechaEgreso        = emp.FechaEgreso,
            SueldoBasico       = emp.SueldoBasico,
            MonedaId           = emp.MonedaId,
            MonedaSimbolo      = moneda?.Simbolo ?? "$",
            Estado             = emp.Estado.ToString().ToUpperInvariant()
        });
    }

    /// <summary>
    /// Da de alta un nuevo empleado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmpleadoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetEmpleadoById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Registra el egreso de un empleado.
    /// </summary>
    [HttpPost("{id:long}/egresar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Egresar(
        long id,
        [FromBody] EgresarEmpleadoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new EgresarEmpleadoCommand(id, request.FechaEgreso), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = $"No se encontró el empleado con ID {id}." });

        return Ok(new { mensaje = "Egreso registrado correctamente." });
    }

    /// <summary>
    /// Crea una liquidación de sueldo para un empleado.
    /// </summary>
    [HttpPost("{id:long}/liquidaciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLiquidacion(
        long id,
        [FromBody] CreateLiquidacionSueldoCommand command,
        CancellationToken ct)
    {
        if (command.EmpleadoId != id)
            return BadRequest(new { error = "El ID del empleado no coincide." });

        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Retorna todas las liquidaciones de sueldo de un empleado.
    /// </summary>
    [HttpGet("{id:long}/liquidaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLiquidaciones(
        long id,
        [FromQuery] int? anio = null,
        [FromQuery] int? mes = null,
        CancellationToken ct = default)
    {
        var emp = await repo.GetByIdAsync(id, ct);
        if (emp is null)
            return NotFound(new { error = $"No se encontró el empleado con ID {id}." });

        var tercero = await db.Terceros.AsNoTracking()
            .Where(x => x.Id == emp.TerceroId)
            .Select(x => new { x.RazonSocial })
            .FirstOrDefaultAsync(ct);

        var empleadoNombre = tercero?.RazonSocial ?? "—";

        var query = db.LiquidacionesSueldo.AsNoTracking()
            .Where(x => x.EmpleadoId == id);

        if (anio.HasValue)
            query = query.Where(x => x.Anio == anio.Value);

        if (mes.HasValue)
            query = query.Where(x => x.Mes == mes.Value);

        var liquidaciones = await query
            .OrderByDescending(x => x.Anio)
            .ThenByDescending(x => x.Mes)
            .Select(x => new LiquidacionSueldoDto
            {
                Id              = x.Id,
                EmpleadoId      = x.EmpleadoId,
                EmpleadoLegajo  = emp.Legajo,
                EmpleadoNombre  = empleadoNombre,
                SucursalId      = x.SucursalId,
                Anio            = x.Anio,
                Mes             = x.Mes,
                Periodo         = $"{x.Anio}/{x.Mes:D2}",
                SueldoBasico    = x.SueldoBasico,
                TotalHaberes    = x.TotalHaberes,
                TotalDescuentos = x.TotalDescuentos,
                Neto            = x.Neto,
                MonedaId        = x.MonedaId,
                MonedaSimbolo   = "$",
                Pagada          = x.Pagada,
                Observacion     = x.Observacion,
                CreatedAt       = x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(liquidaciones);
    }

    /// <summary>
    /// Marca una liquidación de sueldo como pagada.
    /// </summary>
    [HttpPost("{id:long}/liquidaciones/{liquidacionId:long}/pagar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarLiquidacionPagada(
        long id,
        long liquidacionId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new MarcarLiquidacionPagadaCommand(id, liquidacionId), ct);
        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("No se encontro la liquidacion", StringComparison.OrdinalIgnoreCase) == true)
            {
                return NotFound(new
                {
                    error = $"No se encontró la liquidación {liquidacionId} para el empleado {id}."
                });
            }

            return BadRequest(new { error = "La liquidación ya se encuentra marcada como pagada." });
        }

        return Ok(new { mensaje = "Liquidación marcada como pagada correctamente." });
    }

    // ── Asignación Empleado ↔ Área ─────────────────────────────────────────────

    /// <summary>
    /// Retorna las áreas asignadas a un empleado.
    /// Equivale a la pestaña "Áreas" de frmEmpleado del VB6 (clsEmpleadoXArea / SUC_AREAXEMPLEADO).
    /// </summary>
    [HttpGet("{id:long}/areas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAreas(long id, CancellationToken ct)
    {
        var asignaciones = await db.EmpleadosXArea
            .AsNoTracking()
            .Where(x => x.EmpleadoId == id)
            .OrderBy(x => x.Orden)
            .Select(x => new { x.Id, x.EmpleadoId, x.AreaId, x.Orden })
            .ToListAsync(ct);
        return Ok(asignaciones);
    }

    /// <summary>
    /// Asigna un área a un empleado.
    /// </summary>
    [HttpPost("{id:long}/areas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddArea(
        long id,
        [FromBody] AsignarAreaEmpleadoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new AddEmpleadoAreaCommand(id, req.AreaId, req.Orden), ct);
        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = $"Empleado {id} no encontrado." });

            if (result.Error?.Contains("ya esta asignado", StringComparison.OrdinalIgnoreCase) == true)
                return BadRequest(new { error = "El empleado ya está asignado a esa área." });

            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetAreas), new { id }, new { Id = result.Value });
    }

    /// <summary>
    /// Elimina la asignación de un área a un empleado (y sus perfiles asociados).
    /// </summary>
    [HttpDelete("{id:long}/areas/{axeId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveArea(long id, long axeId, CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveEmpleadoAreaCommand(id, axeId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = "Asignación no encontrada." });

        return Ok();
    }

    /// <summary>
    /// Retorna los perfiles de una asignación empleado-área.
    /// Equivale a la pestaña "Perfiles" de frmEmpleado del VB6.
    /// </summary>
    [HttpGet("{id:long}/areas/{axeId:long}/perfiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerfilesDeArea(long id, long axeId, CancellationToken ct)
    {
        var perfiles = await db.EmpleadosXPerfil
            .AsNoTracking()
            .Where(p => p.EmpleadoXAreaId == axeId)
            .OrderBy(p => p.Orden)
            .Select(p => new { p.Id, p.EmpleadoXAreaId, p.PerfilId, p.Orden })
            .ToListAsync(ct);
        return Ok(perfiles);
    }

    /// <summary>
    /// Asigna un perfil a la relación empleado-área.
    /// </summary>
    [HttpPost("{id:long}/areas/{axeId:long}/perfiles")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPerfil(
        long id, long axeId,
        [FromBody] AsignarPerfilRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new AddEmpleadoPerfilCommand(id, axeId, req.PerfilId, req.Orden), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = "Asignación área-empleado no encontrada." });

        return CreatedAtAction(nameof(GetPerfilesDeArea), new { id, axeId }, new { Id = result.Value });
    }

    /// <summary>
    /// Elimina la asignación de un perfil a la relación empleado-área.
    /// </summary>
    [HttpDelete("{id:long}/areas/{axeId:long}/perfiles/{aepId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePerfil(long id, long axeId, long aepId, CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveEmpleadoPerfilCommand(axeId, aepId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = "Asignación de perfil no encontrada." });

        return Ok();
    }

    /// <summary>
    /// Retorna el catálogo de perfiles disponibles para asignar a empleados.
    /// </summary>
    [HttpGet("perfiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerfiles(CancellationToken ct)
    {
        var perfiles = await db.Perfiles
            .AsNoTracking()
            .OrderBy(p => p.Codigo)
            .Select(p => new { p.Id, p.Codigo, p.Descripcion })
            .ToListAsync(ct);
        return Ok(perfiles);
    }
}

public record EgresarEmpleadoRequest(DateOnly FechaEgreso);
public record AsignarAreaEmpleadoRequest(long AreaId, int Orden = 0);
public record AsignarPerfilRequest(long PerfilId, int Orden = 0);