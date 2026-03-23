using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class ContabilidadController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet("asientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsientos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long ejercicioId = 0,
        [FromQuery] long? sucursalId = null,
        [FromQuery] Domain.Enums.EstadoAsiento? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetAsientosPagedQuery(page, pageSize, ejercicioId, sucursalId, estado, desde, hasta), ct);
        return Ok(result);
    }

    [HttpGet("asientos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsientoById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAsientoByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    [HttpPost("asientos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsiento(
        [FromBody] CreateAsientoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetAsientoById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    // -- Libro Mayor -----------------------------------------------------------

    /// <summary>
    /// Retorna el libro mayor de una cuenta contable: todas las l�neas de asiento
    /// que afectan a esa cuenta en un ejercicio y rango de fechas opcional.
    /// Equivale al libro mayor (LIBRO_MAYOR) del m�dulo de contabilidad VB6.
    /// </summary>
    [HttpGet("mayor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMayor(
        [FromQuery] long cuentaId,
        [FromQuery] long ejercicioId,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        if (cuentaId <= 0 || ejercicioId <= 0)
            return BadRequest(new { error = "cuentaId y ejercicioId son requeridos." });

        var q = db.AsientosLineas
            .Where(l => l.CuentaId == cuentaId && db.Asientos
                .Where(a => a.EjercicioId == ejercicioId
                    && (desde == null || a.Fecha >= desde)
                    && (hasta == null || a.Fecha <= hasta))
                .Select(a => a.Id)
                .Contains(l.AsientoId))
            .Join(db.Asientos, l => l.AsientoId, a => a.Id, (l, a) => new
            {
                l.Id,
                l.AsientoId,
                a.Numero,
                a.Fecha,
                AsientoDescripcion = a.Descripcion,
                a.EjercicioId,
                a.SucursalId,
                l.Debe,
                l.Haber,
                LineaDescripcion = l.Descripcion,
                l.CentroCostoId
            });

        var total = await q.CountAsync(ct);
        var lineas = await q
            .OrderBy(x => x.Fecha).ThenBy(x => x.Numero)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new { lineas, page, pageSize, totalCount = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    /// <summary>
    /// Retorna el libro mayor filtrado por centro de costo adem�s de cuenta y ejercicio.
    /// </summary>
    [HttpGet("mayor/centro-costos/{centroCostoId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMayorPorCentroCosto(
        long centroCostoId,
        [FromQuery] long cuentaId,
        [FromQuery] long ejercicioId,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var lineas = await db.AsientosLineas
            .Where(l => l.CuentaId == cuentaId && l.CentroCostoId == centroCostoId
                && db.Asientos
                    .Where(a => a.EjercicioId == ejercicioId
                        && (desde == null || a.Fecha >= desde)
                        && (hasta == null || a.Fecha <= hasta))
                    .Select(a => a.Id)
                    .Contains(l.AsientoId))
            .Join(db.Asientos, l => l.AsientoId, a => a.Id, (l, a) => new
            {
                l.Id,
                l.AsientoId,
                a.Numero,
                a.Fecha,
                a.Descripcion,
                a.EjercicioId,
                l.Debe,
                l.Haber,
                l.CentroCostoId
            })
            .OrderBy(x => x.Fecha).ThenBy(x => x.Numero)
            .ToListAsync(ct);

        return Ok(lineas);
    }
}
