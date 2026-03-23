using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Miembros.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Módulo de gestión de miembros/socios.
/// Implementado sobre la entidad Tercero con rol de cliente activo.
/// </summary>
[Route("api/miembros")]
public class MiembrosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = db.Terceros.AsNoTracking().Where(x => x.EsCliente);

        if (!incluirInactivos)
            query = query.Where(x => x.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.Legajo.ToLower().Contains(term) ||
                x.RazonSocial.ToLower().Contains(term) ||
                x.NroDocumento.ToLower().Contains(term));
        }

        var result = await query
            .OrderBy(x => x.RazonSocial)
            .Select(x => new
            {
                MiembroId = x.Id,
                x.Legajo,
                Nombre = x.RazonSocial,
                x.NroDocumento,
                x.Email,
                x.Telefono,
                x.SucursalId,
                Activo = x.Activo
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var miembro = await db.Terceros.AsNoTracking()
            .Where(x => x.Id == id && x.EsCliente)
            .Select(x => new
            {
                MiembroId = x.Id,
                x.Legajo,
                Nombre = x.RazonSocial,
                x.NombreFantasia,
                x.TipoDocumentoId,
                x.NroDocumento,
                x.CondicionIvaId,
                x.Email,
                x.Telefono,
                x.Celular,
                x.Web,
                x.SucursalId,
                x.Activo,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(miembro);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] MiembroCreateRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateMiembroCommand(
                req.Legajo,
                req.Nombre,
                req.TipoDocumentoId,
                req.NroDocumento,
                req.CondicionIvaId,
                req.SucursalId),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { miembroId = result.Value });
    }

    [HttpPatch("{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateMiembroCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateMiembroCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record MiembroCreateRequest(
    string Legajo,
    string Nombre,
    long TipoDocumentoId,
    string NroDocumento,
    long CondicionIvaId,
    long? SucursalId);
