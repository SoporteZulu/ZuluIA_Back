using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;
using ZuluIA_Back.Application.Features.DescuentosComerciales.Queries;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de descuentos comerciales por tercero e ítem.
/// Equivale a la tabla DESCUENTOS_COMERCIALES / frmDescuentos del sistema VB6.
/// </summary>
public class DescuentosComercialesController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Obtiene descuentos comerciales con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] long? terceroId,
        [FromQuery] long? itemId,
        [FromQuery] DateOnly? vigenteEn,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetDescuentosComercialesQuery(terceroId, itemId, vigenteEn), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo descuento comercial.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDescuentoComercialCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Created($"api/descuentos-comerciales/{result.Value}", new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza un descuento comercial existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateDescuentoComercialCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Elimina un descuento comercial (soft delete).
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteDescuentoComercialCommand(id), ct);
        return FromResult(result);
    }
}
