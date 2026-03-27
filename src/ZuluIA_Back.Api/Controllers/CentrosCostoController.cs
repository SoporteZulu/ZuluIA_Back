using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class CentrosCostoController(
    IMediator mediator,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todos los centros de costo activos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? soloActivos = true,
        CancellationToken ct = default)
    {
        var query = db.CentrosCosto.AsNoTracking();

        if (soloActivos.HasValue)
            query = query.Where(x => x.Activo == soloActivos.Value);

        var result = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.Activo,
                x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo centro de costo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCentroCostoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateCentroCostoCommand(request.Codigo, request.Descripcion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value });
    }

    /// <summary>
    /// Actualiza un centro de costo existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateCentroCostoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateCentroCostoCommand(id, request.Descripcion), ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Centro de costo actualizado correctamente." });
    }

    /// <summary>
    /// Desactiva un centro de costo.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCentroCostoCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se puede desactivar", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                    ? NotFound(new { error = result.Error })
                    : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Centro de costo desactivado correctamente." });
    }

    /// <summary>
    /// Reactiva un centro de costo desactivado.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateCentroCostoCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Centro de costo activado correctamente." });
    }
}

public record CreateCentroCostoRequest(string Codigo, string Descripcion);
public record UpdateCentroCostoRequest(string Descripcion);