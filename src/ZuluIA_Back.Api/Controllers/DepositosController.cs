using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class DepositosController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna los depósitos activos de una sucursal o todos los activos si no se informa sucursal.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySucursal(
        [FromQuery] long? sucursalId,
        [FromQuery] bool incluirInactivos = false,
        CancellationToken ct = default)
    {
        if (sucursalId.HasValue)
        {
            var result = await Mediator.Send(
                new GetDepositosBySucursalQuery(sucursalId.Value, incluirInactivos), ct);
            return Ok(result);
        }

        var depositos = await db.Depositos
            .AsNoTracking()
            .Where(x => incluirInactivos || x.Activo)
            .OrderBy(x => x.SucursalId)
            .ThenByDescending(x => x.Activo)
            .ThenByDescending(x => x.EsDefault)
            .ThenBy(x => x.Descripcion)
            .Select(x => new DepositoDto
            {
                Id = x.Id,
                SucursalId = x.SucursalId,
                Descripcion = x.Descripcion,
                EsDefault = x.EsDefault,
                Activo = x.Activo,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(depositos);
    }

    /// <summary>
    /// Retorna el depósito por defecto de una sucursal.
    /// </summary>
    [HttpGet("default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefault(
        [FromQuery] long sucursalId,
        CancellationToken ct)
    {
        var deposito = await db.Depositos
            .AsNoTracking()
            .Where(x => x.SucursalId == sucursalId &&
                        x.EsDefault  == true        &&
                        x.Activo     == true)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.Descripcion,
                x.EsDefault,
                x.Activo
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(deposito);
    }

    /// <summary>
    /// Crea un nuevo depósito.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepositoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza un depósito existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateDepositoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateDepositoCommand(id, request.Descripcion, request.EsDefault), ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Depósito actualizado correctamente." });
    }

    /// <summary>
    /// Desactiva un depósito.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteDepositoCommand(id), ct);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });

            if (result.Error?.Contains("No se puede desactivar", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { mensaje = "Depósito desactivado correctamente." });
    }

    /// <summary>
    /// Reactiva un depósito desactivado.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateDepositoCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Depósito activado correctamente." });
    }
}

public record UpdateDepositoRequest(string Descripcion, bool EsDefault);