using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Catálogo de bancos (legacy VB6: BANCOS).
/// </summary>
public class BancosController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var bancos = await db.Bancos
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion })
            .ToListAsync(ct);

        return Ok(bancos);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var banco = await db.Bancos
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Descripcion })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(banco);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] BancoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateBancoCommand(req.Descripcion), ct);

        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var banco = await db.Bancos
            .AsNoTracking()
            .Where(x => x.Id == result.Value)
            .Select(x => new { x.Id, x.Descripcion })
            .FirstAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, banco);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] BancoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateBancoCommand(id, req.Descripcion), ct);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value.Id, result.Value.Descripcion });
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteBancoCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("registros relacionados", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : NotFound(new { error = result.Error });

        return NoContent();
    }
}

public record BancoRequest(string Descripcion);
