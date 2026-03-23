using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;
using ZuluIA_Back.Domain.Entities.Geografia;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Administración de regiones comerciales/geográficas jerárquicas.
/// Equivale a FRA_REGIONES del sistema VB6 (clsRegiones).
/// </summary>
public class RegionesController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todas las regiones, opcionalmente sólo las integradoras (nivel superior).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloIntegradoras = false,
        CancellationToken ct = default)
    {
        var q = db.Regiones.AsNoTracking();
        if (soloIntegradoras)
            q = q.Where(r => r.EsRegionIntegradora);

        var lista = await q
            .OrderBy(r => r.Orden)
            .ThenBy(r => r.Codigo)
            .Select(r => new
            {
                r.Id,
                r.Codigo,
                r.Descripcion,
                r.RegionIntegradoraId,
                r.Orden,
                r.Nivel,
                r.CodigoEstructura,
                r.EsRegionIntegradora,
                r.Observacion
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Retorna los hijos directos de una región integradora.
    /// </summary>
    [HttpGet("{id:long}/hijos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHijos(long id, CancellationToken ct)
    {
        var lista = await db.Regiones
            .Where(r => r.RegionIntegradoraId == id)
            .OrderBy(r => r.Orden)
            .Select(r => new { r.Id, r.Codigo, r.Descripcion, r.Nivel, r.Orden, r.EsRegionIntegradora })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Retorna el detalle de una región por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetRegionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var region = await db.Regiones.FindAsync([id], ct);
        return region is null ? NotFound(new { error = $"Región {id} no encontrada." }) : Ok(region);
    }

    /// <summary>
    /// Busca una región por código.
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCodigo(string codigo, CancellationToken ct)
    {
        var region = await db.Regiones
            .FirstOrDefaultAsync(r => r.Codigo == codigo.ToUpperInvariant(), ct);
        return region is null ? NotFound(new { error = $"Región '{codigo}' no encontrada." }) : Ok(region);
    }

    /// <summary>
    /// Crea una nueva región.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRegionRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateRegionCommand(
                req.Codigo,
                req.Descripcion,
                req.RegionIntegradoraId,
                req.Orden,
                req.Nivel,
                req.CodigoEstructura,
                req.EsRegionIntegradora,
                req.Observacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetRegionById", new { id = result.Value }, new { Id = result.Value, Codigo = req.Codigo });
    }

    /// <summary>
    /// Actualiza una región existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateRegionRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateRegionCommand(
                id,
                req.Descripcion,
                req.RegionIntegradoraId,
                req.Orden,
                req.Nivel,
                req.CodigoEstructura,
                req.EsRegionIntegradora,
                req.Observacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    /// <summary>
    /// Elimina una región (solo si no tiene regiones hijas).
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteRegionCommand(id), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("sub-regiones", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok();
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record CreateRegionRequest(
    string Codigo,
    string Descripcion,
    long? RegionIntegradoraId = null,
    int Orden = 0,
    int Nivel = 0,
    string? CodigoEstructura = null,
    bool EsRegionIntegradora = false,
    string? Observacion = null);

public record UpdateRegionRequest(
    string Descripcion,
    long? RegionIntegradoraId = null,
    int Orden = 0,
    int Nivel = 0,
    string? CodigoEstructura = null,
    bool EsRegionIntegradora = false,
    string? Observacion = null);
