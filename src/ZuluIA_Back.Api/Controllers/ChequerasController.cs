using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de chequeras (legacy VB6: CHEQUERAS).
/// </summary>
public class ChequerasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? cajaId,
        [FromQuery] bool? activa,
        CancellationToken ct)
    {
        var query = db.Chequeras.AsNoTracking();

        if (cajaId.HasValue)
            query = query.Where(x => x.CajaId == cajaId.Value);

        if (activa.HasValue)
            query = query.Where(x => x.Activa == activa.Value);

        var list = await query
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.CajaId,
                x.Banco,
                x.NroCuenta,
                x.NroDesde,
                x.NroHasta,
                x.UltimoChequeUsado,
                x.Activa,
                x.Observacion
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Chequeras
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.CajaId,
                x.Banco,
                x.NroCuenta,
                x.NroDesde,
                x.NroHasta,
                x.UltimoChequeUsado,
                x.Activa,
                x.Observacion
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateChequeraRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateChequeraCommand(
                req.CajaId,
                req.Banco,
                req.NroCuenta,
                req.NroDesde,
                req.NroHasta,
                req.Observacion),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateChequeraRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateChequeraCommand(id, req.Banco, req.NroCuenta, req.Observacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("{id:long}/usar/{numero:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UsarCheque(long id, int numero, CancellationToken ct)
    {
        var result = await Mediator.Send(new UsarChequeCommand(id, numero), ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { result.Value.Id, result.Value.UltimoChequeUsado });
    }

    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateChequeraCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateChequeraCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record CreateChequeraRequest(
    long CajaId,
    string Banco,
    string NroCuenta,
    int NroDesde,
    int NroHasta,
    string? Observacion);

public record UpdateChequeraRequest(
    string Banco,
    string NroCuenta,
    string? Observacion);
