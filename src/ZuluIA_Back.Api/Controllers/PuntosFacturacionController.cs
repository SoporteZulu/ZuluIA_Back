using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.Queries;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class PuntosFacturacionController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna los puntos de facturación activos de una sucursal.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySucursal(
        [FromQuery] long sucursalId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetPuntosFacturacionQuery(sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna los tipos de punto de facturación disponibles.
    /// </summary>
    [HttpGet("tipos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos(CancellationToken ct)
    {
        var tipos = await db.TiposPuntoFacturacion
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.PorDefecto })
            .ToListAsync(ct);

        return Ok(tipos);
    }

    /// <summary>
    /// Retorna el próximo número de comprobante para un punto y tipo dado.
    /// Útil para pre-cargar en el formulario de emisión.
    /// </summary>
    [HttpGet("{id:long}/proximo-numero")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProximoNumero(
        long id,
        [FromQuery] long tipoComprobanteId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetProximoNumeroQuery(id, tipoComprobanteId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo punto de facturación.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePuntoFacturacionCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza la descripción y tipo de un punto de facturación.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdatePuntoFacturacionRequest request,
        CancellationToken ct)
    {
        var punto = await db.PuntosFacturacion
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (punto is null)
            return NotFound(new { error = $"No se encontró el punto de facturación con ID {id}." });

        punto.Actualizar(request.TipoId, request.Descripcion, null);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Punto de facturación actualizado correctamente." });
    }

    /// <summary>
    /// Desactiva un punto de facturación.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var punto = await db.PuntosFacturacion
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (punto is null)
            return NotFound(new { error = $"No se encontró el punto de facturación con ID {id}." });

        punto.Desactivar(null);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Punto de facturación desactivado correctamente." });
    }
}

public record UpdatePuntoFacturacionRequest(long TipoId, string? Descripcion);