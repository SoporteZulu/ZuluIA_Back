using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class CartaPorteController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna cartas de porte paginadas con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? comprobanteId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        EstadoCartaPorte? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoCartaPorte>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.CartasPorte.AsNoTracking();

        if (comprobanteId.HasValue)
            query = query.Where(x => x.ComprobanteId == comprobanteId.Value);

        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);

        if (desde.HasValue)
            query = query.Where(x => x.FechaEmision >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(x => x.FechaEmision <= hasta.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.FechaEmision)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CartaPorteDto
            {
                Id               = x.Id,
                ComprobanteId    = x.ComprobanteId,
                NroCtg           = x.NroCtg,
                CuitRemitente    = x.CuitRemitente,
                CuitDestinatario = x.CuitDestinatario,
                CuitTransportista = x.CuitTransportista,
                FechaEmision     = x.FechaEmision,
                Estado           = x.Estado.ToString().ToUpperInvariant(),
                Observacion      = x.Observacion,
                CreatedAt        = x.CreatedAt,
                UpdatedAt        = x.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(new
        {
            page,
            pageSize,
            totalCount = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items
        });
    }

    /// <summary>
    /// Retorna el detalle de una carta de porte por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetCartaPorteById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var carta = await db.CartasPorte
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CartaPorteDto
            {
                Id                = x.Id,
                ComprobanteId     = x.ComprobanteId,
                NroCtg            = x.NroCtg,
                CuitRemitente     = x.CuitRemitente,
                CuitDestinatario  = x.CuitDestinatario,
                CuitTransportista = x.CuitTransportista,
                FechaEmision      = x.FechaEmision,
                Estado            = x.Estado.ToString().ToUpperInvariant(),
                Observacion       = x.Observacion,
                CreatedAt         = x.CreatedAt,
                UpdatedAt         = x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(carta);
    }

    /// <summary>
    /// Retorna un payload dedicado para reimpresion de la carta de porte.
    /// </summary>
    [HttpGet("{id:long}/reimpresion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReimpresion(long id, CancellationToken ct)
    {
        var carta = await db.CartasPorte
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CartaPorteDto
            {
                Id                = x.Id,
                ComprobanteId     = x.ComprobanteId,
                NroCtg            = x.NroCtg,
                CuitRemitente     = x.CuitRemitente,
                CuitDestinatario  = x.CuitDestinatario,
                CuitTransportista = x.CuitTransportista,
                FechaEmision      = x.FechaEmision,
                Estado            = x.Estado.ToString().ToUpperInvariant(),
                Observacion       = x.Observacion,
                CreatedAt         = x.CreatedAt,
                UpdatedAt         = x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return carta is null
            ? NotFound()
            : Ok(new CartaPorteReimpresionResponse(true, DateTimeOffset.UtcNow, carta));
    }

    /// <summary>
    /// Crea una nueva carta de porte en estado PENDIENTE.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCartaPorteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCartaPorteById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Asigna el número de CTG obtenido de AFIP a una carta de porte pendiente.
    /// </summary>
    [HttpPost("{id:long}/asignar-ctg")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarCtg(
        long id,
        [FromBody] AsignarCtgRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AsignarCtgCommand(id, request.NroCtg), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Confirma una carta de porte activa.
    /// </summary>
    [HttpPost("{id:long}/confirmar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirmar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ConfirmarCartaPorteCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Carta de porte confirmada correctamente." });
    }

    /// <summary>
    /// Anula una carta de porte.
    /// </summary>
    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(
        long id,
        [FromBody] AnularCartaPorteRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularCartaPorteCommand(id, request.Observacion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Carta de porte anulada correctamente." });
    }
}

public record AsignarCtgRequest(string NroCtg);
public record AnularCartaPorteRequest(string? Observacion);
public record CartaPorteReimpresionResponse(
    bool EsReimpresion,
    DateTimeOffset GeneradoEn,
    object Documento);