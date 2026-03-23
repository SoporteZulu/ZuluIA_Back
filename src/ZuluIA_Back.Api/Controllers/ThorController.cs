using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Rankings.Queries;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Módulo de inteligencia de negocios y reportes avanzados (Thor/BI).
/// Expone un facade de consultas BI sobre cubos y rankings.
/// </summary>
[Route("api/thor")]
public class ThorController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard(
        [FromQuery] long sucursalId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] int top = 10,
        CancellationToken ct = default)
    {
        var clientes = await Mediator.Send(new GetRankingClientesQuery(sucursalId, desde, hasta, top), ct);
        var items = await Mediator.Send(new GetRankingItemsQuery(sucursalId, desde, hasta, top), ct);

        return Ok(new
        {
            Periodo = new { desde, hasta },
            Top = top,
            RankingClientes = clientes,
            RankingItems = items
        });
    }

    [HttpGet("cubos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCubos(
        [FromQuery] long? usuarioId = null,
        [FromQuery] int? ambienteId = null,
        CancellationToken ct = default)
    {
        var query = db.Cubos.AsNoTracking();

        if (usuarioId.HasValue)
            query = query.Where(c => c.EsSistema || c.UsuarioAltaId == usuarioId.Value);

        if (ambienteId.HasValue)
            query = query.Where(c => c.AmbienteId == ambienteId.Value);

        var cubos = await query
            .OrderBy(c => c.Descripcion)
            .Select(c => new
            {
                c.Id,
                c.Descripcion,
                c.AmbienteId,
                c.EsSistema,
                c.MenuCuboId,
                c.OrigenDatos,
                c.Observacion,
                c.UsuarioAltaId
            })
            .ToListAsync(ct);

        return Ok(cubos);
    }

    [HttpGet("cubos/{cuboId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCuboById(long cuboId, CancellationToken ct)
    {
        var cubo = await db.Cubos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cuboId, ct);
        if (cubo is null) return NotFound(new { error = $"Cubo {cuboId} no encontrado." });

        var campos = await db.CubosCampos.AsNoTracking()
            .Where(c => c.CuboId == cuboId)
            .OrderBy(c => c.Orden)
            .Select(c => new
            {
                c.Id,
                c.SourceName,
                c.Descripcion,
                c.Ubicacion,
                c.Posicion,
                c.Visible,
                c.Calculado,
                c.Filtro,
                c.Orden
            })
            .ToListAsync(ct);

        var filtros = await db.CubosFiltros.AsNoTracking()
            .Where(f => f.CuboId == cuboId)
            .OrderBy(f => f.Orden)
            .Select(f => new { f.Id, f.Filtro, f.Operador, f.Orden })
            .ToListAsync(ct);

        return Ok(new
        {
            Cubo = new
            {
                cubo.Id,
                cubo.Descripcion,
                cubo.AmbienteId,
                cubo.EsSistema,
                cubo.OrigenDatos,
                cubo.Observacion,
                cubo.UsuarioAltaId
            },
            Campos = campos,
            Filtros = filtros
        });
    }

    [HttpGet("analisis-mensual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AnalisisMensual(
        [FromQuery] long sucursalId,
        [FromQuery] int anio,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetAnalisisMensualQuery(sucursalId, anio), ct));
}
