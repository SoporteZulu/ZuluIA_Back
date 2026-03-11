using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.Items.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class DepositosController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna los depósitos activos de una sucursal.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySucursal(
        [FromQuery] long sucursalId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetDepositosBySucursalQuery(sucursalId), ct);
        return Ok(result);
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
        var deposito = await db.Depositos
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (deposito is null)
            return NotFound(new { error = $"No se encontró el depósito con ID {id}." });

        // Si se quiere marcar como default, quitar el default actual de la sucursal
        if (request.EsDefault && !deposito.EsDefault)
        {
            var defaultActual = await db.Depositos
                .Where(x => x.SucursalId == deposito.SucursalId &&
                            x.EsDefault  == true                 &&
                            x.Id         != id)
                .FirstOrDefaultAsync(ct);

            if (defaultActual is not null)
            {
                defaultActual.UnsetDefault();
                db.Depositos.Update(defaultActual);
            }
        }

        deposito.Actualizar(request.Descripcion, request.EsDefault);
        await db.SaveChangesAsync(ct);

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
        var deposito = await db.Depositos
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (deposito is null)
            return NotFound(new { error = $"No se encontró el depósito con ID {id}." });

        if (deposito.EsDefault)
            return Conflict(new
            {
                error = "No se puede desactivar el depósito por defecto. Asigne otro depósito como default primero."
            });

        // Verificar stock activo
        var tieneStock = await db.Stock
            .AnyAsync(x => x.DepositoId == id && x.Cantidad > 0, ct);

        if (tieneStock)
            return Conflict(new
            {
                error = "No se puede desactivar un depósito que tiene stock disponible."
            });

        deposito.Desactivar();
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Depósito desactivado correctamente." });
    }
}

public record UpdateDepositoRequest(string Descripcion, bool EsDefault);