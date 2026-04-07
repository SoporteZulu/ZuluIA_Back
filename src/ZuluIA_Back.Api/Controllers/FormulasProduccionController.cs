using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Produccion.Commands;
using ZuluIA_Back.Application.Features.Produccion.Queries;
using ZuluIA_Back.Application.Features.Produccion.Services;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class FormulasProduccionController(
    IMediator mediator,
    IFormulaProduccionRepository repo,
    IServiceProvider serviceProvider,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    private FormulaProduccionHistorialService historialService => serviceProvider.GetRequiredService<FormulaProduccionHistorialService>();

    /// <summary>
    /// Retorna todas las fórmulas de producción.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloActivas = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetFormulasProduccionQuery(soloActivas), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle de una fórmula con sus ingredientes.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetFormulaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var formula = await repo.GetByIdConIngredientesAsync(id, ct);
        if (formula is null)
            return NotFound(new { error = $"No se encontró la fórmula con ID {id}." });

        var itemIds = formula.Ingredientes
            .Select(i => i.ItemId)
            .Append(formula.ItemResultadoId)
            .Distinct().ToList();

        var items = await db.Items.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        return Ok(new
        {
            formula.Id,
            formula.Codigo,
            formula.Descripcion,
            ItemResultadoId = formula.ItemResultadoId,
            ItemResultadoCodigo = items.GetValueOrDefault(formula.ItemResultadoId)?.Codigo      ?? "—",
            ItemResultadoDescripcion = items.GetValueOrDefault(formula.ItemResultadoId)?.Descripcion ?? "—",
            formula.CantidadResultado,
            formula.UnidadMedidaId,
            formula.Activo,
            formula.Observacion,
            formula.CreatedAt,
            Ingredientes = formula.Ingredientes
                .OrderBy(i => i.Orden)
                .Select(i => new
                {
                    i.Id,
                    i.ItemId,
                    ItemCodigo = items.GetValueOrDefault(i.ItemId)?.Codigo      ?? "—",
                    ItemDescripcion = items.GetValueOrDefault(i.ItemId)?.Descripcion ?? "—",
                    i.Cantidad,
                    i.UnidadMedidaId,
                    i.EsOpcional,
                    i.Orden
                })
        });
    }

    /// <summary>
    /// Crea una nueva fórmula de producción con sus ingredientes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFormulaProduccionCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetFormulaById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza los datos de una fórmula existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateFormulaRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateFormulaProduccionCommand(
                id,
                request.Descripcion,
                request.CantidadResultado,
                request.Observacion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var formulaCompleta = await repo.GetByIdConIngredientesAsync(id, ct);
        if (formulaCompleta is not null)
        {
            await historialService.RegistrarSnapshotAsync(formulaCompleta, "Actualización fórmula", ct);
            await db.SaveChangesAsync(ct);
        }

        return Ok(new { mensaje = "Fórmula actualizada correctamente." });
    }

    [HttpGet("{id:long}/historial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorial(long id, CancellationToken ct)
    {
        var historial = await db.FormulasProduccionHistorial.AsNoTracking()
            .Where(x => x.FormulaId == id)
            .OrderByDescending(x => x.Version)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.FormulaId,
                x.Version,
                x.Codigo,
                x.Descripcion,
                x.CantidadResultado,
                x.Motivo,
                x.SnapshotJson,
                x.CreatedAt,
                x.CreatedBy
            })
            .ToListAsync(ct);

        return Ok(historial);
    }

    /// <summary>
    /// Desactiva una fórmula de producción.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateFormulaProduccionCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Fórmula desactivada correctamente." });
    }

    /// <summary>
    /// Reactiva una fórmula de producción.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateFormulaProduccionCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Fórmula activada correctamente." });
    }
}

public record UpdateFormulaRequest(
    string Descripcion,
    decimal CantidadResultado,
    string? Observacion);