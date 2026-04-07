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
        [FromQuery] long? transportistaId = null,
        [FromQuery] string? estado = null,
        [FromQuery] bool? soloConErrorCtg = null,
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

        if (transportistaId.HasValue)
            query = query.Where(x => x.TransportistaId == transportistaId.Value);

        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);

        if (soloConErrorCtg == true)
            query = query.Where(x => x.UltimoErrorCtg != null);

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
                OrdenCargaId     = x.OrdenCargaId,
                TransportistaId  = x.TransportistaId,
                NroCtg           = x.NroCtg,
                CuitRemitente    = x.CuitRemitente,
                CuitDestinatario = x.CuitDestinatario,
                CuitTransportista = x.CuitTransportista,
                FechaEmision     = x.FechaEmision,
                FechaSolicitudCtg = x.FechaSolicitudCtg,
                IntentosCtg      = x.IntentosCtg,
                UltimoErrorCtg   = x.UltimoErrorCtg,
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
                OrdenCargaId      = x.OrdenCargaId,
                TransportistaId   = x.TransportistaId,
                NroCtg            = x.NroCtg,
                CuitRemitente     = x.CuitRemitente,
                CuitDestinatario  = x.CuitDestinatario,
                CuitTransportista = x.CuitTransportista,
                FechaEmision      = x.FechaEmision,
                FechaSolicitudCtg = x.FechaSolicitudCtg,
                IntentosCtg       = x.IntentosCtg,
                UltimoErrorCtg    = x.UltimoErrorCtg,
                Estado            = x.Estado.ToString().ToUpperInvariant(),
                Observacion       = x.Observacion,
                CreatedAt         = x.CreatedAt,
                UpdatedAt         = x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(carta);
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

    [HttpPost("{id:long}/orden-carga")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearOrdenCarga(long id, [FromBody] CrearOrdenCargaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CrearOrdenCargaCommand(id, request.TransportistaId, request.FechaCarga, request.Origen, request.Destino, request.Patente, request.Observacion),
            ct);

        return FromResult(result);
    }

    [HttpGet("{id:long}/orden-carga")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrdenCarga(long id, CancellationToken ct)
    {
        var orden = await db.OrdenesCarga.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CartaPorteId == id, ct);

        if (orden is null)
            return NotFound(new { error = $"No se encontró orden de carga para la carta de porte ID {id}." });

        string? transportista = null;
        if (orden.TransportistaId.HasValue)
        {
            transportista = await db.Terceros.AsNoTracking()
                .Join(db.Transportistas.AsNoTracking().Where(t => t.Id == orden.TransportistaId.Value),
                    tercero => tercero.Id,
                    t => t.TerceroId,
                    (tercero, _) => tercero.RazonSocial)
                .FirstOrDefaultAsync(ct);
        }

        return Ok(new OrdenCargaDto
        {
            Id = orden.Id,
            CartaPorteId = orden.CartaPorteId,
            TransportistaId = orden.TransportistaId,
            TransportistaRazonSocial = transportista,
            FechaCarga = orden.FechaCarga,
            Origen = orden.Origen,
            Destino = orden.Destino,
            Patente = orden.Patente,
            Confirmada = orden.Confirmada,
            Observacion = orden.Observacion
        });
    }

    [HttpPost("{id:long}/ctg/solicitar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SolicitarCtg(long id, [FromBody] SolicitarCtgRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SolicitarCtgCartaPorteCommand(id, request.FechaSolicitud, request.Observacion),
            ct);

        return FromResult(result);
    }

    [HttpPost("{id:long}/ctg/reintentar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReintentarCtg(long id, [FromBody] SolicitarCtgRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SolicitarCtgCartaPorteCommand(id, request.FechaSolicitud, request.Observacion, true),
            ct);

        return FromResult(result);
    }

    [HttpPost("{id:long}/ctg/consultar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarCtg(long id, [FromBody] ConsultarCtgRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ConsultarCtgCartaPorteCommand(id, request.FechaConsulta, request.NroCtg, request.Error, request.Observacion),
            ct);

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
        var result = await Mediator.Send(
            new ConfirmarCartaPorteCommand(id, DateOnly.FromDateTime(DateTime.Today), null),
            ct);

        return FromResult(result);
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
        var result = await Mediator.Send(
            new AnularCartaPorteWorkflowCommand(id, request.Fecha ?? DateOnly.FromDateTime(DateTime.Today), request.Observacion),
            ct);

        return FromResult(result);
    }

    [HttpGet("{id:long}/historial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorial(long id, CancellationToken ct)
    {
        var eventos = await db.CartasPorteEventos.AsNoTracking()
            .Where(x => x.CartaPorteId == id)
            .OrderByDescending(x => x.FechaEvento)
            .ThenByDescending(x => x.Id)
            .Select(x => new CartaPorteEventoDto
            {
                Id = x.Id,
                CartaPorteId = x.CartaPorteId,
                OrdenCargaId = x.OrdenCargaId,
                TipoEvento = x.TipoEvento.ToString().ToUpperInvariant(),
                EstadoAnterior = x.EstadoAnterior.HasValue ? x.EstadoAnterior.Value.ToString().ToUpperInvariant() : null,
                EstadoNuevo = x.EstadoNuevo.ToString().ToUpperInvariant(),
                FechaEvento = x.FechaEvento,
                Mensaje = x.Mensaje,
                NroCtg = x.NroCtg,
                IntentoCtg = x.IntentoCtg,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy
            })
            .ToListAsync(ct);

        return Ok(eventos);
    }
}

public record AsignarCtgRequest(string NroCtg);
public record CrearOrdenCargaRequest(long? TransportistaId, DateOnly FechaCarga, string Origen, string Destino, string? Patente, string? Observacion);
public record SolicitarCtgRequest(DateOnly FechaSolicitud, string? Observacion);
public record ConsultarCtgRequest(DateOnly FechaConsulta, string? NroCtg, string? Error, string? Observacion);
public record AnularCartaPorteRequest(DateOnly? Fecha, string? Observacion);
