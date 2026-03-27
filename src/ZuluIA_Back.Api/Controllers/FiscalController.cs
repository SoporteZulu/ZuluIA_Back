using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Fiscal.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class FiscalController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpPost("cierres-periodo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CerrarPeriodo([FromBody] CerrarPeriodoContableCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("cierres-periodo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCierresPeriodo([FromQuery] long? ejercicioId = null, CancellationToken ct = default)
    {
        var query = db.CierresPeriodoContable.AsNoTracking();
        if (ejercicioId.HasValue)
            query = query.Where(x => x.EjercicioId == ejercicioId.Value);

        var items = await query.OrderByDescending(x => x.Desde).ThenByDescending(x => x.Id).ToListAsync(ct);
        return Ok(items);
    }

    [HttpPost("reorganizaciones-asientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorganizarAsientos([FromBody] ReorganizarAsientosCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("reorganizaciones-asientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReorganizacionesAsientos(CancellationToken ct)
        => Ok(await db.ReorganizacionesAsientos.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync(ct));

    [HttpPost("libro-viajantes/generar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerarLibroViajantes([FromBody] GenerarLibroViajantesCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("libro-viajantes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLibroViajantes([FromQuery] long sucursalId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken ct)
    {
        var items = await db.LibrosViajantesRegistros.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Desde == desde && x.Hasta == hasta)
            .OrderBy(x => x.VendedorId)
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpPost("rentas-bsas/generar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerarRentasBsAs([FromBody] GenerarRentasBsAsCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("rentas-bsas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRentasBsAs(CancellationToken ct)
        => Ok(await db.RentasBsAsRegistros.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync(ct));

    [HttpPost("hechauka/generar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerarHechauka([FromBody] GenerarHechaukaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("hechauka")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHechauka(CancellationToken ct)
        => Ok(await db.HechaukaRegistros.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync(ct));

    [HttpPost("liquidaciones-primarias-granos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarLiquidacionPrimariaGrano([FromBody] RegistrarLiquidacionPrimariaGranoCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("liquidaciones-primarias-granos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiquidacionesPrimariasGranos(CancellationToken ct)
        => Ok(await db.LiquidacionesPrimariasGranos.AsNoTracking().OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct));

    [HttpPost("salidas-regulatorias/generar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerarSalidaRegulatoria([FromBody] GenerarSalidaRegulatoriaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("salidas-regulatorias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalidasRegulatorias(CancellationToken ct)
        => Ok(await db.SalidasRegulatorias.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync(ct));
}
