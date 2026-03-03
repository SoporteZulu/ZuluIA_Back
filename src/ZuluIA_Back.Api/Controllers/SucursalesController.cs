using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Sucursales.Commands;
using ZuluIA_Back.Application.Features.Sucursales.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class SucursalesController(IMediator mediator) : BaseController(mediator)
{
    /// <summary>
    /// Retorna la lista de sucursales. Por defecto solo las activas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloActivas = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetSucursalesQuery(soloActivas), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de una sucursal por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetSucursalById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetSucursalByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Crea una nueva sucursal.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSucursalCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetSucursalById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza una sucursal existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateSucursalCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva (soft delete) una sucursal.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteSucursalCommand(id), ct);
        return FromResult(result);
    }
}