using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Referencia.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// CRUD de Unidades de Medida con soporte de conversión entre unidades.
/// Migrado desde VB6: frmUnidades / ume_unidades.
/// </summary>
public class UnidadesMedidaController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/unidades-medida
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloActivas = true,
        CancellationToken ct = default)
    {
        var q = db.UnidadesMedida.AsNoTracking();
        if (soloActivas) q = q.Where(x => x.Activa);
        var result = await q
            .OrderBy(x => x.Descripcion)
            .Select(x => new
            {
                x.Id, x.Codigo, x.Descripcion, x.Disminutivo,
                x.Multiplicador, x.EsUnidadBase, x.UnidadBaseId, x.Activa
            })
            .ToListAsync(ct);
        return Ok(result);
    }

    // GET api/unidades-medida/{id}
    [HttpGet("{id:long}", Name = "GetUnidadMedidaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var u = await db.UnidadesMedida.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u is null) return NotFound(new { error = $"Unidad de medida {id} no encontrada." });
        return Ok(new
        {
            u.Id, u.Codigo, u.Descripcion, u.Disminutivo,
            u.Multiplicador, u.EsUnidadBase, u.UnidadBaseId, u.Activa
        });
    }

    // POST api/unidades-medida
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] UnidadMedidaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateUnidadMedidaCommand(
                req.Codigo,
                req.Descripcion,
                req.Disminutivo,
                req.Multiplicador,
                req.EsUnidadBase,
                req.UnidadBaseId),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetUnidadMedidaById", new { id = result.Value }, new { Id = result.Value });
    }

    // PUT api/unidades-medida/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UnidadMedidaUpdateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateUnidadMedidaCommand(
                id,
                req.Descripcion,
                req.Disminutivo,
                req.Multiplicador,
                req.EsUnidadBase,
                req.UnidadBaseId),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound()
                : BadRequest(new { error = result.Error });

        var u = await db.UnidadesMedida.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return Ok(u is null ? new { Id = id } : new { u.Id, u.Descripcion });
    }

    // PATCH api/unidades-medida/{id}/activar
    [HttpPatch("{id:long}/activar")]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateUnidadMedidaCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok(new { Id = id, activa = true });
    }

    // PATCH api/unidades-medida/{id}/desactivar
    [HttpPatch("{id:long}/desactivar")]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateUnidadMedidaCommand(id), ct);
        if (result.IsFailure)
            return NotFound();

        return Ok(new { Id = id, activa = false });
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record UnidadMedidaRequest(
    string Codigo,
    string Descripcion,
    string? Disminutivo,
    decimal Multiplicador = 1m,
    bool EsUnidadBase = true,
    long? UnidadBaseId = null);

public record UnidadMedidaUpdateRequest(
    string Descripcion,
    string? Disminutivo,
    decimal Multiplicador,
    bool EsUnidadBase,
    long? UnidadBaseId);
