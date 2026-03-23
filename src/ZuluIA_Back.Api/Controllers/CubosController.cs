using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;
using ZuluIA_Back.Domain.Entities.BI;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de Cubos de Análisis (BI/OLAP).
/// VB6: clsCubo / BI_CUBO + BI_CUBOCAMPO + BI_CUBOFILTROS.
/// Permite diseñar, guardar y recuperar cubos pivot personalizados.
/// </summary>
[Route("api/cubos")]
public class CubosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    // ── Cubos ─────────────────────────────────────────────────────────────────

    /// <summary>Retorna todos los cubos visibles para el usuario (sistema + propios).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? usuarioId = null,
        [FromQuery] int? ambienteId = null,
        CancellationToken ct = default)
    {
        var query = db.Cubos.AsNoTracking();
        if (usuarioId.HasValue)
            query = query.Where(c => c.EsSistema || c.UsuarioAltaId == usuarioId.Value);
        if (ambienteId.HasValue)
            query = query.Where(c => c.AmbienteId == ambienteId.Value);

        var lista = await query
            .Select(c => new {
                c.Id, c.Descripcion, c.MenuCuboId, c.OrigenDatos,
                c.Observacion, c.AmbienteId, c.EsSistema, c.FormatoId,
                c.UsuarioAltaId, c.CuboOrigenId, c.CreatedAt, c.UpdatedAt
            })
            .OrderBy(c => c.Descripcion)
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Retorna un cubo por ID con sus campos y filtros.</summary>
    [HttpGet("{id:long}", Name = "GetCuboById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var cubo = await db.Cubos.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cubo is null) return NotFound(new { error = $"Cubo {id} no encontrado." });

        var campos = await db.CubosCampos.AsNoTracking()
            .Where(c => c.CuboId == id)
            .OrderBy(c => c.Orden)
            .Select(c => new {
                c.Id, c.SourceName, c.Descripcion, c.Ubicacion, c.Posicion,
                c.Visible, c.Calculado, c.Filtro, c.CampoPadreId, c.Orden,
                c.TipoOrden, c.FuncionAgregado, c.VarName
            })
            .ToListAsync(ct);

        var filtros = await db.CubosFiltros.AsNoTracking()
            .Where(f => f.CuboId == id)
            .OrderBy(f => f.Orden)
            .Select(f => new { f.Id, f.Filtro, f.Operador, f.Orden })
            .ToListAsync(ct);

        return Ok(new {
            Cubo = new {
                cubo.Id, cubo.Descripcion, cubo.MenuCuboId, cubo.OrigenDatos,
                cubo.Observacion, cubo.AmbienteId, cubo.EsSistema, cubo.FormatoId,
                cubo.UsuarioAltaId, cubo.CuboOrigenId
            },
            Campos  = campos,
            Filtros = filtros
        });
    }

    /// <summary>Crea un nuevo cubo de análisis.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CuboRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateCuboCommand(
                req.Descripcion,
                req.OrigenDatos,
                req.Observacion,
                req.AmbienteId,
                req.MenuCuboId,
                req.CuboOrigenId,
                req.EsSistema,
                req.FormatoId,
                req.UsuarioAltaId),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    /// <summary>Actualiza un cubo de análisis.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] CuboRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateCuboCommand(
                id,
                req.Descripcion,
                req.OrigenDatos,
                req.Observacion,
                req.AmbienteId,
                req.MenuCuboId,
                req.FormatoId),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id });
    }

    /// <summary>Elimina un cubo y todos sus campos y filtros.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCuboCommand(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    // ── Campos de Cubo ────────────────────────────────────────────────────────

    /// <summary>Retorna los campos de un cubo ordenados por posición.</summary>
    [HttpGet("{id:long}/campos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampos(long id, CancellationToken ct)
    {
        var lista = await db.CubosCampos.AsNoTracking()
            .Where(c => c.CuboId == id)
            .OrderBy(c => c.Orden)
            .Select(c => new {
                c.Id, c.CuboId, c.SourceName, c.Descripcion, c.Ubicacion,
                c.Posicion, c.TipoOrden, c.FuncionAgregado, c.Visible,
                c.Calculado, c.VarName, c.Filtro, c.CampoPadreId, c.Orden
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Agrega un campo a un cubo.</summary>
    [HttpPost("{id:long}/campos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddCampo(long id, [FromBody] CuboCampoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddCuboCampoCommand(
                id,
                req.SourceName,
                req.Descripcion,
                req.Ubicacion,
                req.Posicion,
                req.Visible,
                req.Calculado,
                req.Filtro,
                req.CampoPadreId,
                req.Orden),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetCampos), new { id }, new { Id = result.Value });
    }

    /// <summary>Actualiza un campo de un cubo.</summary>
    [HttpPut("{id:long}/campos/{campoId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCampo(long id, long campoId, [FromBody] CuboCampoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateCuboCampoCommand(
                id,
                campoId,
                req.Descripcion,
                req.Ubicacion,
                req.Posicion,
                req.Visible,
                req.Calculado,
                req.Filtro,
                req.CampoPadreId,
                req.Orden,
                req.TipoOrden,
                req.FuncionAgregado),
            ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = campoId });
    }

    /// <summary>Elimina un campo de un cubo.</summary>
    [HttpDelete("{id:long}/campos/{campoId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCampo(long id, long campoId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCuboCampoCommand(id, campoId), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    // ── Filtros de Cubo ───────────────────────────────────────────────────────

    /// <summary>Retorna los filtros de un cubo.</summary>
    [HttpGet("{id:long}/filtros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFiltros(long id, CancellationToken ct)
    {
        var lista = await db.CubosFiltros.AsNoTracking()
            .Where(f => f.CuboId == id)
            .OrderBy(f => f.Orden)
            .Select(f => new { f.Id, f.CuboId, f.Filtro, f.Operador, f.Orden })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Agrega un filtro a un cubo.</summary>
    [HttpPost("{id:long}/filtros")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddFiltro(long id, [FromBody] CuboFiltroRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddCuboFiltroCommand(id, req.Filtro, req.Operador, req.Orden), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetFiltros), new { id }, new { Id = result.Value });
    }

    /// <summary>Actualiza un filtro de un cubo.</summary>
    [HttpPut("{id:long}/filtros/{filtroId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFiltro(long id, long filtroId, [FromBody] CuboFiltroRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateCuboFiltroCommand(id, filtroId, req.Filtro, req.Operador, req.Orden), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { Id = filtroId });
    }

    /// <summary>Elimina un filtro de un cubo.</summary>
    [HttpDelete("{id:long}/filtros/{filtroId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFiltro(long id, long filtroId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCuboFiltroCommand(id, filtroId), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────

public record CuboRequest(
    string Descripcion,
    string? OrigenDatos       = null,
    string? Observacion       = null,
    int? AmbienteId           = null,
    long? MenuCuboId          = null,
    long? CuboOrigenId        = null,
    bool? EsSistema           = null,
    long? FormatoId           = null,
    long? UsuarioAltaId       = null);

public record CuboCampoRequest(
    string SourceName,
    string? Descripcion        = null,
    int? Ubicacion             = null,
    int? Posicion              = null,
    bool? Visible              = null,
    bool? Calculado            = null,
    string? Filtro             = null,
    long? CampoPadreId         = null,
    int? Orden                 = null,
    int? TipoOrden             = null,
    int? FuncionAgregado       = null);

public record CuboFiltroRequest(
    string Filtro,
    int? Operador = null,
    int? Orden    = null);
