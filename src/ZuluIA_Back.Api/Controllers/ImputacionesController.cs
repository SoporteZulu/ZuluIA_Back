using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class ImputacionesController(
    IMediator mediator,
    IImputacionRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna las imputaciones de un comprobante como origen
    /// (lo que este comprobante cancela en otros).
    /// </summary>
    [HttpGet("origen/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrigen(
        long comprobanteId,
        CancellationToken ct)
    {
        var imputaciones = await repo.GetByComprobanteOrigenAsync(comprobanteId, ct);

        var destinoIds = imputaciones
            .Select(x => x.ComprobanteDestinoId)
            .Distinct()
            .ToList();

        var numeros = await db.Comprobantes
            .AsNoTracking()
            .Where(x => destinoIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Numero.Prefijo,
                x.Numero.Numero
            })
            .ToDictionaryAsync(x => x.Id, ct);

        var origen = await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.Id == comprobanteId)
            .Select(x => new { x.Numero.Prefijo, x.Numero.Numero })
            .FirstOrDefaultAsync(ct);

        var dtos = imputaciones.Select(i => new ImputacionDto
        {
            Id                   = i.Id,
            ComprobanteOrigenId  = i.ComprobanteOrigenId,
            NumeroOrigen         = origen is not null
                ? $"{origen.Prefijo:D4}-{origen.Numero:D8}"
                : "—",
            ComprobanteDestinoId = i.ComprobanteDestinoId,
            NumeroDestino        = numeros.ContainsKey(i.ComprobanteDestinoId)
                ? $"{numeros[i.ComprobanteDestinoId].Prefijo:D4}-{numeros[i.ComprobanteDestinoId].Numero:D8}"
                : "—",
            Importe              = i.Importe,
            Fecha                = i.Fecha,
            CreatedAt            = i.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Retorna las imputaciones de un comprobante como destino
    /// (lo que otros comprobantes le cancelan a este).
    /// </summary>
    [HttpGet("destino/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDestino(
        long comprobanteId,
        CancellationToken ct)
    {
        var imputaciones = await repo.GetByComprobanteDestinoAsync(comprobanteId, ct);

        var origenIds = imputaciones
            .Select(x => x.ComprobanteOrigenId)
            .Distinct()
            .ToList();

        var numeros = await db.Comprobantes
            .AsNoTracking()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Numero.Prefijo, x.Numero.Numero })
            .ToDictionaryAsync(x => x.Id, ct);

        var destino = await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.Id == comprobanteId)
            .Select(x => new { x.Numero.Prefijo, x.Numero.Numero })
            .FirstOrDefaultAsync(ct);

        var dtos = imputaciones.Select(i => new ImputacionDto
        {
            Id                   = i.Id,
            ComprobanteOrigenId  = i.ComprobanteOrigenId,
            NumeroOrigen         = numeros.ContainsKey(i.ComprobanteOrigenId)
                ? $"{numeros[i.ComprobanteOrigenId].Prefijo:D4}-{numeros[i.ComprobanteOrigenId].Numero:D8}"
                : "—",
            ComprobanteDestinoId = i.ComprobanteDestinoId,
            NumeroDestino        = destino is not null
                ? $"{destino.Prefijo:D4}-{destino.Numero:D8}"
                : "—",
            Importe              = i.Importe,
            Fecha                = i.Fecha,
            CreatedAt            = i.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Retorna el total imputado de un comprobante destino.
    /// </summary>
    [HttpGet("total-imputado/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalImputado(
        long comprobanteId,
        CancellationToken ct)
    {
        var total = await repo.GetTotalImputadoAsync(comprobanteId, ct);
        return Ok(new { comprobanteId, totalImputado = total });
    }

    /// <summary>
    /// Imputa un comprobante origen contra un destino.
    /// Actualiza los saldos de ambos comprobantes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Imputar(
        [FromBody] ImputarComprobanteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new
            {
                imputacionId = result.Value,
                mensaje = "Imputación registrada correctamente."
            })
            : BadRequest(new { error = result.Error });
    }
}