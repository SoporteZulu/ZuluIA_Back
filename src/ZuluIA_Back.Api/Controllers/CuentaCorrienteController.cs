using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class CuentaCorrienteController(
    IMediator mediator,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna los saldos de cuenta corriente de un tercero
    /// por moneda y opcionalmente por sucursal.
    /// </summary>
    [HttpGet("{terceroId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSaldos(
        long terceroId,
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetCuentaCorrienteTerceroQuery(terceroId, sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna los movimientos de cuenta corriente paginados de un tercero.
    /// </summary>
    [HttpGet("{terceroId:long}/movimientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovimientos(
        long terceroId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? monedaId = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetMovimientosCtaCteQuery(
                page, pageSize,
                terceroId, sucursalId,
                monedaId, desde, hasta),
            ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el resumen de saldos de todos los terceros con saldo distinto de cero.
    /// Útil para listados de deudores/acreedores.
    /// </summary>
    [HttpGet("deudores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeudores(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? monedaId = null,
        [FromQuery] bool soloDeudores = true,
        CancellationToken ct = default)
    {
        var query = db.CuentaCorriente
            .AsNoTracking()
            .Where(x => x.Saldo != 0);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value
                                  || x.SucursalId == null);

        if (monedaId.HasValue)
            query = query.Where(x => x.MonedaId == monedaId.Value);

        if (soloDeudores)
            query = query.Where(x => x.Saldo > 0);

        var cuentas = await query
            .OrderByDescending(x => Math.Abs(x.Saldo))
            .Select(x => new
            {
                x.TerceroId,
                x.SucursalId,
                x.MonedaId,
                x.Saldo,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        // Enriquecer con nombres
        var terceroIds = cuentas.Select(x => x.TerceroId).Distinct().ToList();
        var monedaIds = cuentas.Select(x => x.MonedaId).Distinct().ToList();

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var resultado = cuentas.Select(c => new
        {
            c.TerceroId,
            TerceroRazonSocial = terceros.GetValueOrDefault(c.TerceroId)?.RazonSocial ?? "—",
            c.SucursalId,
            c.MonedaId,
            MonedaSimbolo = monedas.GetValueOrDefault(c.MonedaId)?.Simbolo ?? "$",
            c.Saldo,
            c.UpdatedAt
        });

        return Ok(resultado);
    }

    /// <summary>
    /// Retorna los saldos de cuenta corriente con intereses moratorios calculados
    /// según la tasa vigente para la fecha de corte indicada.
    /// </summary>
    [HttpGet("{terceroId:long}/con-intereses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConIntereses(
        long terceroId,
        [FromQuery] long? sucursalId = null,
        [FromQuery] DateOnly? fechaCalculo = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new ZuluIA_Back.Application.Features.TasasInteres.Queries.GetCuentaCorrienteConInteresQuery(
                terceroId, sucursalId, fechaCalculo), ct);
        return Ok(result);
    }
}


