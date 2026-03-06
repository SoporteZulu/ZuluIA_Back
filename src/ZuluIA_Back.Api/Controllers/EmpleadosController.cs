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
                EmpleadoNombre  = tercero!.RazonSocial,
                SucursalId      = x.SucursalId,
                Anio            = x.Anio,
                Mes             = x.Mes,
                Periodo         = $"{x.Anio}/{x.Mes:D2}",
                SueldoBasico    = x.SueldoBasico,
                TotalHaberes    = x.TotalHaberes,
                TotalDescuentos = x.TotalDescuentos,
                Neto            = x.Neto,
                MonedaId        = x.MonedaId,
                MonedaSimbolo   = moneda!.Simbolo,
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
        var emp = await repo.GetByIdAsync(id, ct);
        if (emp is null)
            return NotFound(new { error = $"No se encontró el empleado con ID {id}." });

        emp.Egresar(request.FechaEgreso);
        repo.Update(emp);
        await db.SaveChangesAsync(ct);

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
}

public record EgresarEmpleadoRequest(DateOnly FechaEgreso);