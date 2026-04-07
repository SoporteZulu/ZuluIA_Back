using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Pagos.Commands;
using ZuluIA_Back.Application.Features.Tesoreria.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class TesoreriaController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpPost("cajas/{id:long}/abrir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AbrirCaja(long id, [FromBody] AbrirCajaTesoreriaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new AbrirCajaTesoreriaCommand(id, request.FechaApertura, request.SaldoInicial, request.Observacion), ct);
        return FromResult(result);
    }

    [HttpPost("cajas/{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CerrarCaja(long id, [FromBody] CerrarCajaTesoreriaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CerrarCajaTesoreriaCommand(id, request.FechaCierre, request.SaldoInformado, request.Observacion), ct);
        return FromResult(result);
    }

    [HttpPost("depositos-operar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarDepositoOperar([FromBody] RegistrarDepositoOperarCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("ventanilla/cobros")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarCobroVentanilla([FromBody] RegistrarCobroCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCobroById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("ventanilla/pagos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarPagoVentanilla([FromBody] RegistrarPagoCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedAtRoute("GetPagoById", new { id = result.IsSuccess ? result.Value : 0 }, new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("vales")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarVale([FromBody] RegistrarValeTesoreriaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("reintegros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarReintegro([FromBody] RegistrarReintegroTesoreriaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("movimientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovimientos(
        [FromQuery] long? cajaCuentaId = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] TipoOperacionTesoreria? tipoOperacion = null,
        [FromQuery] bool incluirAnulados = false,
        CancellationToken ct = default)
    {
        var query = db.TesoreriaMovimientos.AsNoTracking();

        if (cajaCuentaId.HasValue)
            query = query.Where(x => x.CajaCuentaId == cajaCuentaId.Value);
        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);
        if (tipoOperacion.HasValue)
            query = query.Where(x => x.TipoOperacion == tipoOperacion.Value);
        if (!incluirAnulados)
            query = query.Where(x => !x.Anulado);

        var movimientos = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        var cajaIds = movimientos.Select(x => x.CajaCuentaId).Distinct().ToList();
        var terceroIds = movimientos.Where(x => x.TerceroId.HasValue).Select(x => x.TerceroId!.Value).Distinct().ToList();

        var cajas = await db.CajasCuentasBancarias.AsNoTracking()
            .Where(x => cajaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion, x.EsCaja, x.Banco })
            .ToDictionaryAsync(x => x.Id, ct);

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        return Ok(movimientos.Select(x => new
        {
            x.Id,
            x.SucursalId,
            x.CajaCuentaId,
            CajaDescripcion = cajas.GetValueOrDefault(x.CajaCuentaId)?.Descripcion ?? "—",
            EsCaja = cajas.GetValueOrDefault(x.CajaCuentaId)?.EsCaja ?? true,
            Banco = cajas.GetValueOrDefault(x.CajaCuentaId)?.Banco,
            x.Fecha,
            TipoOperacion = x.TipoOperacion.ToString().ToUpperInvariant(),
            Sentido = x.Sentido.ToString().ToUpperInvariant(),
            x.TerceroId,
            TerceroRazonSocial = x.TerceroId.HasValue ? terceros.GetValueOrDefault(x.TerceroId.Value)?.RazonSocial : null,
            x.Importe,
            x.MonedaId,
            x.Cotizacion,
            x.ReferenciaTipo,
            x.ReferenciaId,
            x.Observacion,
            x.Anulado,
            x.CreatedAt,
            x.UpdatedAt,
            x.CreatedBy,
            x.UpdatedBy
        }));
    }

    [HttpGet("cajas/{id:long}/cierres")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCierres(long id, CancellationToken ct)
    {
        var cierres = await db.TesoreriaCierres.AsNoTracking()
            .Where(x => x.CajaCuentaId == id)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.NroCierre)
            .Select(x => new
            {
                x.Id,
                x.CajaCuentaId,
                x.NroCierre,
                x.Fecha,
                x.EsApertura,
                x.SaldoInformado,
                x.SaldoSistema,
                Diferencia = x.SaldoInformado - x.SaldoSistema,
                x.TotalIngresos,
                x.TotalEgresos,
                x.CantidadMovimientos,
                x.Observacion,
                x.CreatedAt,
                x.CreatedBy
            })
            .ToListAsync(ct);

        return Ok(cierres);
    }

    [HttpGet("auditoria")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditoria(
        [FromQuery] long? cajaCuentaId = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var movimientos = db.TesoreriaMovimientos.AsNoTracking();
        var cierres = db.TesoreriaCierres.AsNoTracking();

        if (cajaCuentaId.HasValue)
        {
            movimientos = movimientos.Where(x => x.CajaCuentaId == cajaCuentaId.Value);
            cierres = cierres.Where(x => x.CajaCuentaId == cajaCuentaId.Value);
        }

        if (desde.HasValue)
        {
            movimientos = movimientos.Where(x => x.Fecha >= desde.Value);
            cierres = cierres.Where(x => x.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            movimientos = movimientos.Where(x => x.Fecha <= hasta.Value);
            cierres = cierres.Where(x => x.Fecha <= hasta.Value);
        }

        var totalMovimientos = await movimientos.CountAsync(ct);
        var totalAnulados = await movimientos.CountAsync(x => x.Anulado, ct);
        var totalIngresos = await movimientos.Where(x => x.Sentido == SentidoMovimientoTesoreria.Ingreso && !x.Anulado).SumAsync(x => (decimal?)x.Importe * x.Cotizacion, ct) ?? 0m;
        var totalEgresos = await movimientos.Where(x => x.Sentido == SentidoMovimientoTesoreria.Egreso && !x.Anulado).SumAsync(x => (decimal?)x.Importe * x.Cotizacion, ct) ?? 0m;
        var totalCierres = await cierres.CountAsync(ct);

        return Ok(new
        {
            totalMovimientos,
            totalAnulados,
            totalIngresos,
            totalEgresos,
            totalCierres,
            saldoNeto = totalIngresos - totalEgresos
        });
    }
}

public record AbrirCajaTesoreriaRequest(DateOnly FechaApertura, decimal SaldoInicial, string? Observacion);
public record CerrarCajaTesoreriaRequest(DateOnly FechaCierre, decimal SaldoInformado, string? Observacion);
