using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class IntegradorasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? tipoSistema,
        [FromQuery] bool? activa,
        CancellationToken ct = default)
    {
        var query = db.Integradoras.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tipoSistema))
            query = query.Where(x => x.TipoSistema == tipoSistema.Trim().ToUpperInvariant());

        if (activa.HasValue)
            query = query.Where(x => x.Activa == activa.Value);

        var result = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Nombre,
                x.TipoSistema,
                x.UrlEndpoint,
                x.Configuracion,
                x.Activa,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}", Name = "GetIntegradoraById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.Integradoras.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Nombre,
                x.TipoSistema,
                x.UrlEndpoint,
                x.Configuracion,
                x.Activa,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CrearIntegradoraRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateIntegradoraCommand(
                req.Codigo,
                req.Nombre,
                req.TipoSistema,
                req.UrlEndpoint,
                req.ApiKey,
                req.Configuracion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetIntegradoraById", new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] ActualizarIntegradoraRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateIntegradoraCommand(id, req.Nombre, req.TipoSistema, req.UrlEndpoint, req.Configuracion),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    [HttpPatch("{id:long}/rotar-api-key")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RotarApiKey(long id, [FromBody] RotarApiKeyRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new RotateIntegradoraApiKeyCommand(id, req.NuevaApiKey), ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateIntegradoraCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateIntegradoraCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record CrearIntegradoraRequest(
    string Codigo,
    string Nombre,
    string TipoSistema,
    string? UrlEndpoint,
    string? ApiKey,
    string? Configuracion);

public record ActualizarIntegradoraRequest(
    string Nombre,
    string TipoSistema,
    string? UrlEndpoint,
    string? Configuracion);

public record RotarApiKeyRequest(string NuevaApiKey);
