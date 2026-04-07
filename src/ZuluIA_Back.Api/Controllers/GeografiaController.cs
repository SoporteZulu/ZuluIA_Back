using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;

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

    [HttpGet("paises/{id:long}", Name = "GetPaisById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaisById(long id, CancellationToken ct)
    {
        var pais = await db.Paises.FindAsync([id], ct);
        return pais is null ? NotFound(new { error = $"Pais {id} no encontrado." }) : Ok(pais);
    }

    [HttpPost("paises")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePais([FromBody] PaisRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreatePaisCommand(request.Codigo, request.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetPaisById", new { id = result.Value }, new { Id = result.Value, request.Codigo });
    }

    [HttpPut("paises/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePais(long id, [FromBody] PaisRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdatePaisCommand(id, request.Codigo, request.Descripcion), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { Id = id });
    }

    [HttpDelete("paises/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeletePais(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeletePaisCommand(id), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("provincias asociadas", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok();
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

    [HttpGet("provincias/{id:long}", Name = "GetProvinciaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProvinciaById(long id, CancellationToken ct)
    {
        var provincia = await db.Provincias.FindAsync([id], ct);
        return provincia is null ? NotFound(new { error = $"Provincia {id} no encontrada." }) : Ok(provincia);
    }

    [HttpPost("provincias")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProvincia([FromBody] ProvinciaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateProvinciaCommand(request.PaisId, request.Codigo, request.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetProvinciaById", new { id = result.Value }, new { Id = result.Value, request.Codigo });
    }

    [HttpPut("provincias/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProvincia(long id, [FromBody] ProvinciaRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateProvinciaCommand(id, request.PaisId, request.Codigo, request.Descripcion), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                || result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { Id = id });
    }

    [HttpDelete("provincias/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProvincia(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteProvinciaCommand(id), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("localidades asociadas", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok();
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

    [HttpGet("localidades/{id:long}", Name = "GetLocalidadById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocalidadById(long id, CancellationToken ct)
    {
        var localidad = await db.Localidades.FindAsync([id], ct);
        return localidad is null ? NotFound(new { error = $"Localidad {id} no encontrada." }) : Ok(localidad);
    }

    [HttpPost("localidades")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateLocalidad([FromBody] LocalidadRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateLocalidadCommand(request.ProvinciaId, request.Descripcion, request.CodigoPostal), ct);
        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetLocalidadById", new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("localidades/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateLocalidad(long id, [FromBody] LocalidadRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateLocalidadCommand(id, request.ProvinciaId, request.Descripcion, request.CodigoPostal), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { Id = id });
    }

    [HttpDelete("localidades/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteLocalidad(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteLocalidadCommand(id), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("barrios asociados", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok();
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

    [HttpGet("barrios/{id:long}", Name = "GetBarrioById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBarrioById(long id, CancellationToken ct)
    {
        var barrio = await db.Barrios.FindAsync([id], ct);
        return barrio is null ? NotFound(new { error = $"Barrio {id} no encontrado." }) : Ok(barrio);
    }

    [HttpPost("barrios")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBarrio([FromBody] BarrioRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateBarrioCommand(request.LocalidadId, request.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetBarrioById", new { id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("barrios/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateBarrio(long id, [FromBody] BarrioRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateBarrioCommand(id, request.LocalidadId, request.Descripcion), ct);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { Id = id });
    }

    [HttpDelete("barrios/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBarrio(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteBarrioCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok();
    }
}

public record PaisRequest(string Codigo, string Descripcion);
public record ProvinciaRequest(long PaisId, string Codigo, string Descripcion);
public record LocalidadRequest(long ProvinciaId, string Descripcion, string? CodigoPostal = null);
public record BarrioRequest(long LocalidadId, string Descripcion);