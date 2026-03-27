using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Sucursales.Commands;
using ZuluIA_Back.Application.Features.Sucursales.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Api.Controllers;

public class SucursalesController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna la lista de sucursales. Por defecto solo las activas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool soloActivas = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetSucursalesQuery(soloActivas), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de una sucursal por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetSucursalById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetSucursalByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Crea una nueva sucursal.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSucursalCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetSucursalById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza una sucursal existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateSucursalCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva (soft delete) una sucursal.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteSucursalCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reactiva una sucursal previamente desactivada.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateSucursalCommand(id), ct);
        return FromResult(result);
    }

    // ── Áreas de sucursal ─────────────────────────────────────────────────────

    /// <summary>
    /// Retorna las áreas/departamentos registrados.
    /// </summary>
    [HttpGet("areas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAreas([FromQuery] long? sucursalId, CancellationToken ct)
    {
        var q = db.Areas.AsNoTracking();
        if (sucursalId.HasValue)
            q = q.Where(a => a.SucursalId == sucursalId.Value);

        var lista = await q
            .OrderBy(a => a.Descripcion)
            .Select(a => new { a.Id, a.Descripcion, a.Codigo, a.SucursalId })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Retorna un área por ID.
    /// </summary>
    [HttpGet("areas/{id:long}", Name = "GetAreaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAreaById(long id, CancellationToken ct)
    {
        var area = await db.Areas.FindAsync([id], ct);
        return area is null ? NotFound(new { error = $"Área {id} no encontrada." }) : Ok(area);
    }

    /// <summary>
    /// Crea un área nueva.
    /// </summary>
    [HttpPost("areas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateArea([FromBody] CreateAreaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateAreaCommand(req.Descripcion, req.Codigo, req.SucursalId), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetAreaById", new { id = result.Value }, new { Id = result.Value });
    }

    /// <summary>
    /// Actualiza un área existente.
    /// </summary>
    [HttpPut("areas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateArea(long id, [FromBody] CreateAreaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateAreaCommand(id, req.Descripcion, req.Codigo, req.SucursalId), ct);
        return FromNestedResult(result, new { Id = id });
    }

    /// <summary>
    /// Elimina un área.
    /// </summary>
    [HttpDelete("areas/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteArea(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteAreaCommand(id), ct);
        return FromNestedResult(result);
    }

    // ── Tipos de comprobante por sucursal ─────────────────────────────────────

    /// <summary>
    /// Retorna la configuración de tipos de comprobante para una sucursal.
    /// </summary>
    [HttpGet("{id:long}/tipos-comprobante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposComprobante(long id, CancellationToken ct)
    {
        var lista = await db.TiposComprobantesSucursal
            .Where(t => t.SucursalId == id)
            .Select(t => new
            {
                t.Id,
                t.TipoComprobanteId,
                t.SucursalId,
                t.NumeroComprobanteProximo,
                t.FilasCantidad,
                t.FilasAnchoMaximo,
                t.CantidadCopias,
                t.ImprimirControladorFiscal,
                t.VarianteNroUnico,
                t.PermitirSeleccionMoneda,
                t.MonedaId,
                t.Editable,
                t.VistaPrevia,
                t.ControlIntervalo,
                t.NumeroDesde,
                t.NumeroHasta
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Crea la configuración de un tipo de comprobante en una sucursal.
    /// </summary>
    [HttpPost("{id:long}/tipos-comprobante")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTipoComprobanteSucursal(
        long id,
        [FromBody] CreateTipoComprobanteSucursalRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateTipoComprobanteSucursalCommand(
                id,
                req.TipoComprobanteId,
                req.NumeroProximo,
                req.FilasCantidad,
                req.FilasAnchoMaximo,
                req.CantidadCopias,
                req.ImprimirControladorFiscal,
                req.VarianteNroUnico,
                req.PermitirSeleccionMoneda,
                req.MonedaId,
                req.Editable,
                req.VistaPrevia,
                req.ControlIntervalo,
                req.NumeroDesde,
                req.NumeroHasta),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Created(string.Empty, new { Id = result.Value });
    }

    /// <summary>
    /// Actualiza la configuración de un tipo de comprobante en una sucursal.
    /// </summary>
    [HttpPut("{id:long}/tipos-comprobante/{configId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTipoComprobanteSucursal(
        long id, long configId,
        [FromBody] CreateTipoComprobanteSucursalRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateTipoComprobanteSucursalCommand(
                id,
                configId,
                req.FilasCantidad,
                req.FilasAnchoMaximo,
                req.CantidadCopias,
                req.ImprimirControladorFiscal,
                req.VarianteNroUnico,
                req.PermitirSeleccionMoneda,
                req.MonedaId,
                req.Editable,
                req.VistaPrevia,
                req.ControlIntervalo,
                req.NumeroDesde,
                req.NumeroHasta),
            ct);

        return FromNestedResult(result, new { Id = configId });
    }

    // ─── Domicilios de Sucursal ───────────────────────────────────────────────

    /// <summary>Retorna los domicilios de la sucursal. VB6: clsSucDomicilio / SUC_DOMICILIO.</summary>
    [HttpGet("{id:long}/domicilios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDomicilios(long id, CancellationToken ct)
    {
        var lista = await db.SucursalesDomicilio
            .AsNoTracking()
            .Where(d => d.SucursalId == id)
            .OrderBy(d => d.Orden)
            .Select(d => new { d.Id, d.TipoDomicilioId, d.ProvinciaId, d.LocalidadId,
                               d.Calle, d.Barrio, d.CodigoPostal, d.Observacion, d.Orden, d.EsDefecto })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Agrega un domicilio a la sucursal.</summary>
    [HttpPost("{id:long}/domicilios")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddDomicilio(long id, [FromBody] SucDomicilioRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddSucursalDomicilioCommand(
                id,
                req.TipoDomicilioId,
                req.ProvinciaId,
                req.LocalidadId,
                req.Calle,
                req.Barrio,
                req.CodigoPostal,
                req.Observacion,
                req.Orden,
                req.EsDefecto),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetDomicilios), new { id }, new { Id = result.Value });
    }

    /// <summary>Actualiza un domicilio de la sucursal.</summary>
    [HttpPut("{id:long}/domicilios/{domId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDomicilio(long id, long domId, [FromBody] SucDomicilioRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateSucursalDomicilioCommand(
                id,
                domId,
                req.TipoDomicilioId,
                req.ProvinciaId,
                req.LocalidadId,
                req.Calle,
                req.Barrio,
                req.CodigoPostal,
                req.Observacion,
                req.Orden,
                req.EsDefecto),
            ct);

        return FromNestedResult(result, new { Id = domId });
    }

    /// <summary>Elimina un domicilio de la sucursal.</summary>
    [HttpDelete("{id:long}/domicilios/{domId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDomicilio(long id, long domId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteSucursalDomicilioCommand(id, domId), ct);
        return FromNestedResult(result);
    }

    // ─── Medios de Contacto de Sucursal ──────────────────────────────────────

    /// <summary>Retorna los medios de contacto de la sucursal. VB6: clsSucMedioContacto / SUC_MEDIOCONTACTO.</summary>
    [HttpGet("{id:long}/medios-contacto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMediosContacto(long id, CancellationToken ct)
    {
        var lista = await db.SucursalesMedioContacto
            .AsNoTracking()
            .Where(m => m.SucursalId == id)
            .OrderBy(m => m.Orden)
            .Select(m => new { m.Id, m.TipoMedioContactoId, m.Valor, m.Orden, m.EsDefecto, m.Observacion })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>Agrega un medio de contacto a la sucursal.</summary>
    [HttpPost("{id:long}/medios-contacto")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddMedioContacto(long id, [FromBody] SucMedioContactoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddSucursalMedioContactoCommand(
                id,
                req.Valor,
                req.TipoMedioContactoId,
                req.Orden,
                req.EsDefecto,
                req.Observacion),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetMediosContacto), new { id }, new { Id = result.Value });
    }

    /// <summary>Actualiza un medio de contacto de la sucursal.</summary>
    [HttpPut("{id:long}/medios-contacto/{mconId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMedioContacto(long id, long mconId, [FromBody] SucMedioContactoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateSucursalMedioContactoCommand(
                id,
                mconId,
                req.Valor,
                req.TipoMedioContactoId,
                req.Orden,
                req.EsDefecto,
                req.Observacion),
            ct);

        return FromNestedResult(result, new { Id = mconId });
    }

    /// <summary>Elimina un medio de contacto de la sucursal.</summary>
    [HttpDelete("{id:long}/medios-contacto/{mconId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedioContacto(long id, long mconId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteSucursalMedioContactoCommand(id, mconId), ct);
        return FromNestedResult(result);
    }

    private IActionResult FromNestedResult(Result result, object? okPayload = null)
    {
        if (result.IsSuccess)
            return okPayload is null ? Ok() : Ok(okPayload);

        var isNotFound = result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
            || result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true;

        return isNotFound
            ? NotFound(new { error = result.Error })
            : BadRequest(new { error = result.Error });
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record CreateAreaRequest(string Descripcion, string? Codigo = null, long? SucursalId = null);

public record CreateTipoComprobanteSucursalRequest(
    long TipoComprobanteId,
    long NumeroProximo = 1,
    int FilasCantidad = 0,
    int FilasAnchoMaximo = 0,
    int CantidadCopias = 1,
    bool ImprimirControladorFiscal = false,
    bool VarianteNroUnico = false,
    bool PermitirSeleccionMoneda = false,
    long? MonedaId = null,
    bool Editable = true,
    bool VistaPrevia = false,
    bool ControlIntervalo = false,
    long? NumeroDesde = null,
    long? NumeroHasta = null);

public record SucDomicilioRequest(
    long? TipoDomicilioId, long? ProvinciaId, long? LocalidadId,
    string? Calle, string? Barrio, string? CodigoPostal, string? Observacion,
    int Orden = 0, bool EsDefecto = false);

public record SucMedioContactoRequest(
    string Valor, long? TipoMedioContactoId = null,
    int Orden = 0, bool EsDefecto = false, string? Observacion = null);