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
        CancellationToken ct,
        [FromQuery] long? tipoComprobanteDestinoId = null,
        [FromQuery] bool incluirAnuladas = false)
    {
        var imputaciones = await repo.GetByComprobanteOrigenAsync(comprobanteId, incluirAnuladas, ct);

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
                x.TipoComprobanteId,
                x.Numero.Prefijo,
                x.Numero.Numero
            })
            .ToDictionaryAsync(x => x.Id, ct);

        var tipoIds = numeros.Values.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct);

        if (tipoComprobanteDestinoId.HasValue)
            imputaciones = imputaciones
                .Where(x => numeros.TryGetValue(x.ComprobanteDestinoId, out var n) && n.TipoComprobanteId == tipoComprobanteDestinoId.Value)
                .ToList();

        var origen = await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.Id == comprobanteId)
            .Select(x => new { x.Numero.Prefijo, x.Numero.Numero, x.TipoComprobanteId })
            .FirstOrDefaultAsync(ct);

        var tipoOrigen = origen is not null
            ? await db.TiposComprobante.AsNoTracking()
                .Where(x => x.Id == origen.TipoComprobanteId)
                .Select(x => x.Descripcion)
                .FirstOrDefaultAsync(ct)
            : null;

        var dtos = imputaciones.Select(i => new ImputacionDto
        {
            Id                   = i.Id,
            ComprobanteOrigenId  = i.ComprobanteOrigenId,
            NumeroOrigen         = origen is not null
                ? $"{origen.Prefijo:D4}-{origen.Numero:D8}"
                : "—",
            TipoComprobanteOrigenId = origen?.TipoComprobanteId,
            TipoComprobanteOrigen = tipoOrigen ?? "—",
            ComprobanteDestinoId = i.ComprobanteDestinoId,
            NumeroDestino        = numeros.ContainsKey(i.ComprobanteDestinoId)
                ? $"{numeros[i.ComprobanteDestinoId].Prefijo:D4}-{numeros[i.ComprobanteDestinoId].Numero:D8}"
                : "—",
            TipoComprobanteDestinoId = numeros.GetValueOrDefault(i.ComprobanteDestinoId)?.TipoComprobanteId,
            TipoComprobanteDestino = numeros.TryGetValue(i.ComprobanteDestinoId, out var destinoInfo)
                ? tipos.GetValueOrDefault(destinoInfo.TipoComprobanteId, "—")
                : "—",
            Importe              = i.Importe,
            Fecha                = i.Fecha,
            CreatedAt            = i.CreatedAt,
            Anulada              = i.Anulada,
            FechaDesimputacion   = i.FechaDesimputacion,
            MotivoDesimputacion  = i.MotivoDesimputacion,
            DesimputadaAt        = i.DesimputadaAt,
            RolComprobante       = "ORIGEN"
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
        CancellationToken ct,
        [FromQuery] long? tipoComprobanteOrigenId = null,
        [FromQuery] bool incluirAnuladas = false)
    {
        var imputaciones = await repo.GetByComprobanteDestinoAsync(comprobanteId, incluirAnuladas, ct);

        var origenIds = imputaciones
            .Select(x => x.ComprobanteOrigenId)
            .Distinct()
            .ToList();

        var numeros = await db.Comprobantes
            .AsNoTracking()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, x.TipoComprobanteId, x.Numero.Prefijo, x.Numero.Numero })
            .ToDictionaryAsync(x => x.Id, ct);

        var tipoIds = numeros.Values.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct);

        if (tipoComprobanteOrigenId.HasValue)
            imputaciones = imputaciones
                .Where(x => numeros.TryGetValue(x.ComprobanteOrigenId, out var n) && n.TipoComprobanteId == tipoComprobanteOrigenId.Value)
                .ToList();

        var destino = await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.Id == comprobanteId)
            .Select(x => new { x.Numero.Prefijo, x.Numero.Numero, x.TipoComprobanteId })
            .FirstOrDefaultAsync(ct);

        var tipoDestino = destino is not null
            ? await db.TiposComprobante.AsNoTracking()
                .Where(x => x.Id == destino.TipoComprobanteId)
                .Select(x => x.Descripcion)
                .FirstOrDefaultAsync(ct)
            : null;

        var dtos = imputaciones.Select(i => new ImputacionDto
        {
            Id                   = i.Id,
            ComprobanteOrigenId  = i.ComprobanteOrigenId,
            NumeroOrigen         = numeros.ContainsKey(i.ComprobanteOrigenId)
                ? $"{numeros[i.ComprobanteOrigenId].Prefijo:D4}-{numeros[i.ComprobanteOrigenId].Numero:D8}"
                : "—",
            TipoComprobanteOrigenId = numeros.GetValueOrDefault(i.ComprobanteOrigenId)?.TipoComprobanteId,
            TipoComprobanteOrigen = numeros.TryGetValue(i.ComprobanteOrigenId, out var origenInfo)
                ? tipos.GetValueOrDefault(origenInfo.TipoComprobanteId, "—")
                : "—",
            ComprobanteDestinoId = i.ComprobanteDestinoId,
            NumeroDestino        = destino is not null
                ? $"{destino.Prefijo:D4}-{destino.Numero:D8}"
                : "—",
            TipoComprobanteDestinoId = destino?.TipoComprobanteId,
            TipoComprobanteDestino = tipoDestino ?? "—",
            Importe              = i.Importe,
            Fecha                = i.Fecha,
            CreatedAt            = i.CreatedAt,
            Anulada              = i.Anulada,
            FechaDesimputacion   = i.FechaDesimputacion,
            MotivoDesimputacion  = i.MotivoDesimputacion,
            DesimputadaAt        = i.DesimputadaAt,
            RolComprobante       = "DESTINO"
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("historial/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorial(
        long comprobanteId,
        [FromQuery] bool incluirAnuladas = true,
        [FromQuery] long? tipoComprobanteRelacionadoId = null,
        CancellationToken ct = default)
    {
        var historial = await repo.GetHistorialByComprobanteAsync(comprobanteId, incluirAnuladas, ct);
        var relacionadosIds = historial
            .SelectMany(x => new[] { x.ComprobanteOrigenId, x.ComprobanteDestinoId })
            .Distinct()
            .ToList();

        var comprobantes = await db.Comprobantes
            .AsNoTracking()
            .Where(x => relacionadosIds.Contains(x.Id))
            .Select(x => new { x.Id, x.TipoComprobanteId, x.Numero.Prefijo, x.Numero.Numero })
            .ToDictionaryAsync(x => x.Id, ct);

        var tipoIds = comprobantes.Values.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct);

        var dtos = historial
            .Where(x => !tipoComprobanteRelacionadoId.HasValue ||
                (x.ComprobanteOrigenId != comprobanteId && comprobantes.GetValueOrDefault(x.ComprobanteOrigenId)?.TipoComprobanteId == tipoComprobanteRelacionadoId.Value) ||
                (x.ComprobanteDestinoId != comprobanteId && comprobantes.GetValueOrDefault(x.ComprobanteDestinoId)?.TipoComprobanteId == tipoComprobanteRelacionadoId.Value))
            .Select(i =>
            {
                var rol = i.ComprobanteOrigenId == comprobanteId ? "ORIGEN" : "DESTINO";
                var origenInfo = comprobantes.GetValueOrDefault(i.ComprobanteOrigenId);
                var destinoInfo = comprobantes.GetValueOrDefault(i.ComprobanteDestinoId);

                return new ImputacionDto
                {
                    Id = i.Id,
                    ComprobanteOrigenId = i.ComprobanteOrigenId,
                    NumeroOrigen = origenInfo is not null ? $"{origenInfo.Prefijo:D4}-{origenInfo.Numero:D8}" : "—",
                    TipoComprobanteOrigenId = origenInfo?.TipoComprobanteId,
                    TipoComprobanteOrigen = origenInfo is not null ? tipos.GetValueOrDefault(origenInfo.TipoComprobanteId, "—") : "—",
                    ComprobanteDestinoId = i.ComprobanteDestinoId,
                    NumeroDestino = destinoInfo is not null ? $"{destinoInfo.Prefijo:D4}-{destinoInfo.Numero:D8}" : "—",
                    TipoComprobanteDestinoId = destinoInfo?.TipoComprobanteId,
                    TipoComprobanteDestino = destinoInfo is not null ? tipos.GetValueOrDefault(destinoInfo.TipoComprobanteId, "—") : "—",
                    Importe = i.Importe,
                    Fecha = i.Fecha,
                    CreatedAt = i.CreatedAt,
                    Anulada = i.Anulada,
                    FechaDesimputacion = i.FechaDesimputacion,
                    MotivoDesimputacion = i.MotivoDesimputacion,
                    DesimputadaAt = i.DesimputadaAt,
                    RolComprobante = rol
                };
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

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

    [HttpPost("desimputar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desimputar(
        [FromBody] DesimputarComprobanteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { imputacionId = result.Value, mensaje = "Imputación deshecha correctamente." })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("desimputar-masivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DesimputarMasivo(
        [FromBody] DesimputarComprobantesMasivosCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { desimputaciones = result.Value.Count, ids = result.Value })
            : BadRequest(new { error = result.Error });
    }
}