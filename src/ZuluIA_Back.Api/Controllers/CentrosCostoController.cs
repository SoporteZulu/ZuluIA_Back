using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class CentrosCostoController(
    IMediator mediator,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todos los centros de costo activos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? soloActivos = true,
        CancellationToken ct = default)
    {
        var query = db.CentrosCosto.AsNoTracking();

        if (soloActivos.HasValue)
            query = query.Where(x => x.Activo == soloActivos.Value);

        var result = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.Activo,
                x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo centro de costo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCentroCostoRequest request,
        CancellationToken ct)
    {
        var existe = await db.CentrosCosto
            .AnyAsync(x => x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct);

        if (existe)
            return Conflict(new { error = $"Ya existe un centro de costo con código '{request.Codigo}'." });

        var cc = Domain.Entities.Contabilidad.CentroCosto.Crear(
            request.Codigo,
            request.Descripcion);

        await db.CentrosCosto.AddAsync(cc, ct);
        await db.SaveChangesAsync(ct);

        return Ok(new { id = cc.Id });
    }

    /// <summary>
    /// Actualiza un centro de costo existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateCentroCostoRequest request,
        CancellationToken ct)
    {
        var cc = await db.CentrosCosto
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (cc is null)
            return NotFound(new { error = $"No se encontró el centro de costo con ID {id}." });

        cc.Actualizar(request.Descripcion);
        db.CentrosCosto.Update(cc);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Centro de costo actualizado correctamente." });
    }

    /// <summary>
    /// Desactiva un centro de costo.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var cc = await db.CentrosCosto
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (cc is null)
            return NotFound(new { error = $"No se encontró el centro de costo con ID {id}." });

        // Verificar que no tenga líneas de asiento asociadas
        var tieneLineas = await db.AsientosLineas
            .AnyAsync(x => x.CentroCostoId == id, ct);

        if (tieneLineas)
            return Conflict(new
            {
                error = "No se puede desactivar un centro de costo con asientos registrados."
            });

        cc.Desactivar();
        db.CentrosCosto.Update(cc);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Centro de costo desactivado correctamente." });
    }
}

public record CreateCentroCostoRequest(string Codigo, string Descripcion);
public record UpdateCentroCostoRequest(string Descripcion);