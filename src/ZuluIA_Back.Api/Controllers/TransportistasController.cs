using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.DTOs;
using ZuluIA_Back.Application.Features.Extras.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class TransportistasController(
    IMediator mediator,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna transportistas activos enriquecidos con datos del tercero.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? soloActivos = true,
        [FromQuery] string? patente = null,
        CancellationToken ct = default)
    {
        var query = db.Transportistas.AsNoTracking();

        if (soloActivos.HasValue)
            query = query.Where(x => x.Activo == soloActivos.Value);

        if (!string.IsNullOrWhiteSpace(patente))
            query = query.Where(x => x.Patente != null &&
                x.Patente.Contains(patente.ToUpperInvariant()));

        var transportistas = await query
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

        var terceroIds = transportistas.Select(x => x.TerceroId).Distinct().ToList();
        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.NroDocumento })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = transportistas.Select(t => new TransportistaDto
        {
            Id                   = t.Id,
            TerceroId            = t.TerceroId,
            TerceroRazonSocial   = terceros.GetValueOrDefault(t.TerceroId)?.RazonSocial  ?? "—",
            TerceroCuit          = terceros.GetValueOrDefault(t.TerceroId)?.NroDocumento ?? "—",
            NroCuitTransportista = t.NroCuitTransportista,
            DomicilioPartida     = t.DomicilioPartida,
            Patente              = t.Patente,
            MarcaVehiculo        = t.MarcaVehiculo,
            Activo               = t.Activo
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Retorna un transportista por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetTransportistaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var t = await db.Transportistas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (t is null)
            return NotFound(new { error = $"No se encontró el transportista con ID {id}." });

        var tercero = await db.Terceros.AsNoTracking()
            .Where(x => x.Id == t.TerceroId)
            .Select(x => new { x.RazonSocial, x.NroDocumento })
            .FirstOrDefaultAsync(ct);

        return Ok(new TransportistaDto
        {
            Id                   = t.Id,
            TerceroId            = t.TerceroId,
            TerceroRazonSocial   = tercero?.RazonSocial  ?? "—",
            TerceroCuit          = tercero?.NroDocumento ?? "—",
            NroCuitTransportista = t.NroCuitTransportista,
            DomicilioPartida     = t.DomicilioPartida,
            Patente              = t.Patente,
            MarcaVehiculo        = t.MarcaVehiculo,
            Activo               = t.Activo
        });
    }

    /// <summary>
    /// Crea un nuevo transportista.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransportistaRequest request,
        CancellationToken ct)
    {
        var command = new CreateTransportistaCommand(
            request.TerceroId,
            request.NroCuitTransportista,
            request.DomicilioPartida,
            request.Patente,
            request.MarcaVehiculo);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return Conflict(new { error = result.Error });

        return CreatedAtRoute("GetTransportistaById",
            new { id = result.Value },
            new { id = result.Value });
    }

    /// <summary>
    /// Actualiza los datos de un transportista.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTransportistaRequest request,
        CancellationToken ct)
    {
        var command = new UpdateTransportistaCommand(
            id,
            request.DomicilioPartida,
            request.Patente,
            request.MarcaVehiculo);

        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Transportista actualizado correctamente." });
    }

    /// <summary>
    /// Desactiva un transportista.
    /// </summary>
    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateTransportistaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Transportista desactivado correctamente." });
    }

    /// <summary>
    /// Reactiva un transportista desactivado.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateTransportistaCommand(id), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { mensaje = "Transportista activado correctamente." });
    }
}

public record CreateTransportistaRequest(
    long TerceroId,
    string? NroCuitTransportista,
    string? DomicilioPartida,
    string? Patente,
    string? MarcaVehiculo);

public record UpdateTransportistaRequest(
    string? DomicilioPartida,
    string? Patente,
    string? MarcaVehiculo);