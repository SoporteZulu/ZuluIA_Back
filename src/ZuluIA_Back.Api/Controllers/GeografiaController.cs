using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class GeografiaController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet("paises")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaises(CancellationToken ct)
    {
        var paises = await db.Paises
            .AsNoTracking()
            .OrderBy(p => p.Descripcion)
            .Select(p => new { p.Id, p.Codigo, p.Descripcion })
            .ToListAsync(ct);

        return Ok(paises);
    }

    [HttpGet("provincias")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProvincias(
        [FromQuery] long? paisId,
        CancellationToken ct)
    {
        var query = db.Provincias.AsNoTracking();

        if (paisId.HasValue)
            query = query.Where(p => p.PaisId == paisId.Value);

        var provincias = await query
            .OrderBy(p => p.Descripcion)
            .Select(p => new { p.Id, p.Codigo, p.Descripcion, p.PaisId })
            .ToListAsync(ct);

        return Ok(provincias);
    }

    [HttpGet("localidades")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocalidades(
        [FromQuery] long? provinciaId,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = db.Localidades.AsNoTracking();

        if (provinciaId.HasValue)
            query = query.Where(l => l.ProvinciaId == provinciaId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.Descripcion.ToLower().Contains(search.ToLower()));

        var localidades = await query
            .OrderBy(l => l.Descripcion)
            .Select(l => new { l.Id, l.Descripcion, l.CodigoPostal, l.ProvinciaId })
            .Take(100)
            .ToListAsync(ct);

        return Ok(localidades);
    }

    [HttpGet("barrios")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBarrios(
        [FromQuery] long? localidadId,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = db.Barrios.AsNoTracking();

        if (localidadId.HasValue)
            query = query.Where(b => b.LocalidadId == localidadId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Descripcion.ToLower().Contains(search.ToLower()));

        var barrios = await query
            .OrderBy(b => b.Descripcion)
            .Select(b => new { b.Id, b.Descripcion, b.LocalidadId })
            .Take(100)
            .ToListAsync(ct);

        return Ok(barrios);
    }
}