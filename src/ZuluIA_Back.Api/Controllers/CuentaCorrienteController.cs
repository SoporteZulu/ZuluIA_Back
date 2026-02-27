using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class CuentaCorrienteController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet("tercero/{terceroId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovimientos(
        long terceroId,
        [FromQuery] long? monedaId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = db.MovimientosStock
            .AsNoTracking();

        var movimientos = await db.Comprobantes
            .AsNoTracking()
            .Where(c => c.TerceroId == terceroId)
            .OrderByDescending(c => c.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Fecha,
                NroFormateado = c.Numero.Prefijo.ToString("D4") + "-" + c.Numero.Numero.ToString("D8"),
                Estado = c.Estado.ToString(),
                c.Total,
                c.Saldo
            })
            .ToListAsync(ct);

        return Ok(movimientos);
    }

    [HttpGet("saldo/{terceroId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSaldo(
        long terceroId,
        [FromQuery] long? monedaId = null,
        CancellationToken ct = default)
    {
        var saldo = await db.Comprobantes
            .AsNoTracking()
            .Where(c => c.TerceroId == terceroId)
            .SumAsync(c => c.Saldo, ct);

        return Ok(new { terceroId, saldo });
    }
}