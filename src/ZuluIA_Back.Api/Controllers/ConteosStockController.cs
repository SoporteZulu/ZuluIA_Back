using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.Commands;
using ZuluIA_Back.Application.Features.Stock.DTOs;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/stock/conteos")]
public class ConteosStockController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna la agenda operativa de conteos cíclicos con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? estado = null,
        CancellationToken ct = default)
    {
        var query = db.ConteosCiclicos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoNormalizado = estado.Trim().ToLowerInvariant();
            query = query.Where(x => x.Estado == estadoNormalizado);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x =>
                x.Deposito.ToUpper().Contains(term) ||
                x.Zona.ToUpper().Contains(term) ||
                x.Frecuencia.ToUpper().Contains(term) ||
                x.Responsable.ToUpper().Contains(term) ||
                x.Observacion.ToUpper().Contains(term) ||
                x.NextStep.ToUpper().Contains(term));
        }

        var items = await query
            .OrderBy(x => x.ProximoConteo)
            .ThenBy(x => x.Deposito)
            .ThenBy(x => x.Zona)
            .Select(x => new ConteoCiclicoDto(
                x.Id,
                x.Deposito,
                x.Zona,
                x.Frecuencia,
                x.ProximoConteo,
                x.Estado,
                x.DivergenciaPct,
                x.Responsable,
                x.Observacion,
                x.NextStep,
                x.ExecutionNote))
            .ToListAsync(ct);

        return Ok(items);
    }

    /// <summary>
    /// Crea un nuevo plan de conteo cíclico.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] UpsertConteoCiclicoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateConteoCiclicoCommand(
                request.Deposito,
                request.Zona,
                request.Frecuencia,
                request.ProximoConteo,
                request.Estado,
                request.DivergenciaPct,
                request.Responsable,
                request.Observacion,
                request.NextStep,
                request.ExecutionNote),
            ct);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Siembra los conteos cíclicos base de la referencia legacy cuando aún no existen.
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        var result = await Mediator.Send(new SeedConteosCiclicosCommand(), ct);

        return Ok(new
        {
            itemsProcesados = result.Value,
            mensaje = result.Value == 0
                ? "Los conteos base ya estaban sembrados."
                : "Conteos base sembrados correctamente."
        });
    }

    /// <summary>
    /// Actualiza un plan de conteo existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpsertConteoCiclicoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateConteoCiclicoCommand(
                id,
                request.Deposito,
                request.Zona,
                request.Frecuencia,
                request.ProximoConteo,
                request.Estado,
                request.DivergenciaPct,
                request.Responsable,
                request.Observacion,
                request.NextStep,
                request.ExecutionNote),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Conteo actualizado correctamente." });
    }

    /// <summary>
    /// Elimina un plan de conteo.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteConteoCiclicoCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Conteo eliminado correctamente." });
    }
}

public sealed record UpsertConteoCiclicoRequest(
    string Deposito,
    string Zona,
    string Frecuencia,
    DateOnly ProximoConteo,
    string Estado,
    decimal DivergenciaPct,
    string? Responsable,
    string? Observacion,
    string? NextStep,
    string? ExecutionNote);
