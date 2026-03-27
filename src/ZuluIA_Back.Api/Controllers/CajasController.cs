using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cajas.Commands;
using ZuluIA_Back.Application.Features.Cajas.Queries;
using ZuluIA_Back.Application.Features.Cajas.DTOs;
using ZuluIA_Back.Application.Features.Tesoreria.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class CajasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todas las cajas/cuentas activas de una sucursal.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySucursal(
        [FromQuery] long sucursalId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCajasBySucursalQuery(sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de una caja/cuenta por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetCajaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCajaByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna los tipos de caja/cuenta disponibles.
    /// </summary>
    [HttpGet("tipos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos(CancellationToken ct)
    {
        var tipos = await db.TiposCajaCuenta
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.EsCaja })
            .ToListAsync(ct);

        return Ok(tipos);
    }

    /// <summary>
    /// Retorna las formas de pago habilitadas para una caja.
    /// </summary>
    [HttpGet("{id:long}/formas-pago")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFormasPago(long id, CancellationToken ct)
    {
        var formas = await db.FormasPagoCaja
            .AsNoTracking()
            .Where(x => x.CajaId == id && x.Habilitado)
            .Select(x => new
            {
                x.Id,
                x.CajaId,
                x.FormaPagoId,
                x.MonedaId,
                x.Habilitado
            })
            .ToListAsync(ct);

        return Ok(formas);
    }

    /// <summary>
    /// Crea una nueva caja o cuenta bancaria.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCajaCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCajaById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza una caja/cuenta existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateCajaCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva (soft delete) una caja/cuenta.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCajaCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Ejecuta el cierre de arqueo de una caja, incrementando el número de cierre.
    /// </summary>
    [HttpPost("{id:long}/cerrar-arqueo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CerrarArqueo(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CerrarCajaTesoreriaCommand(
                id,
                DateOnly.FromDateTime(DateTime.Today),
                0m,
                "Cierre desde CajasController"),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var cierre = await db.TesoreriaCierres.AsNoTracking()
            .Where(x => x.Id == result.Value)
            .Select(x => new { x.NroCierre })
            .FirstOrDefaultAsync(ct);

        return Ok(new
        {
            cajaId = id,
            nroCierre = cierre?.NroCierre,
            mensaje = $"Arqueo cerrado. Número de cierre: {cierre?.NroCierre}."
        });
    }

    /// <summary>
    /// Registra una apertura de caja (habilita la caja para operar en la fecha).
    /// Equivale a frmAperturaCajasCuentasBancarias del VB6.
    /// </summary>
    [HttpPost("{id:long}/abrir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AbrirCaja(
        long id,
        [FromBody] AbrirCajaRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AbrirCajaTesoreriaCommand(
                id,
                request.FechaApertura,
                request.SaldoInicial,
                "Apertura desde CajasController"),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new
        {
            cajaId = id,
            fechaApertura = request.FechaApertura,
            saldoInicial = request.SaldoInicial,
            mensaje = "Caja abierta correctamente."
        });
    }

    /// <summary>
    /// Registra una transferencia entre dos cajas/cuentas bancarias.
    /// Equivale a frmTransferenciasCajasCuentasBancarias del VB6.
    /// </summary>
    [HttpPost("transferencias")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transferencia(
        [FromBody] RegistrarTransferenciaCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Retorna el historial de transferencias de una caja.
    /// </summary>
    [HttpGet("{id:long}/transferencias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransferencias(
        long id,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken ct)
    {
        var query = db.TransferenciasCaja
            .AsNoTracking()
            .Where(x => (x.CajaOrigenId == id || x.CajaDestinoId == id) && !x.Anulada);

        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        var lista = await query
            .OrderByDescending(x => x.Fecha)
            .Select(x => new
            {
                x.Id,
                x.Fecha,
                x.CajaOrigenId,
                x.CajaDestinoId,
                x.Importe,
                x.MonedaId,
                x.Cotizacion,
                x.Concepto,
                Tipo = x.CajaOrigenId == id ? "EGRESO" : "INGRESO"
            })
            .ToListAsync(ct);

        return Ok(lista);
    }
}

// ── Request bodies ───────────────────────────────────────────────────────────
public record AbrirCajaRequest(DateOnly FechaApertura, decimal SaldoInicial);