using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class PuntosFacturacionController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna los puntos de facturación activos de una sucursal.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySucursal(
        [FromQuery] long sucursalId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetPuntosFacturacionQuery(sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna los tipos de punto de facturación disponibles.
    /// </summary>
    [HttpGet("tipos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos(CancellationToken ct)
    {
        var tipos = await db.TiposPuntoFacturacion
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.PorDefecto })
            .ToListAsync(ct);

        return Ok(tipos);
    }

    /// <summary>
    /// Retorna el próximo número de comprobante para un punto y tipo dado.
    /// Útil para pre-cargar en el formulario de emisión.
    /// </summary>
    [HttpGet("{id:long}/proximo-numero")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProximoNumero(
        long id,
        [FromQuery] long tipoComprobanteId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetProximoNumeroQuery(id, tipoComprobanteId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo punto de facturación.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePuntoFacturacionCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza la descripción y tipo de un punto de facturación.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdatePuntoFacturacionRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdatePuntoFacturacionCommand(id, request.TipoId, request.Descripcion ?? string.Empty),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Punto de facturación actualizado correctamente." });
    }

    /// <summary>
    /// Desactiva un punto de facturación.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeletePuntoFacturacionCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Punto de facturación desactivado correctamente." });
    }

    /// <summary>
    /// Reactiva un punto de facturación desactivado.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivatePuntoFacturacionCommand(id), ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { mensaje = "Punto de facturación activado correctamente." });
    }

    // ── Configuración Fiscal (FISC_CONFIGURACIONES_GENERALES) ───────────────────────

    /// <summary>
    /// Retorna las configuraciones de impresora fiscal de un punto de facturación.
    /// VB6: frmAjustesPV / FISC_CONFIGURACIONES_GENERALES.
    /// </summary>
    [HttpGet("{id:long}/configuracion-fiscal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguracionFiscal(long id, CancellationToken ct)
    {
        var lista = await db.ConfiguracionesFiscales
            .AsNoTracking()
            .Where(c => c.PuntoFacturacionId == id)
            .Select(c => new {
                c.Id, c.TipoComprobanteId, c.TecnologiaId, c.InterfazFiscalId,
                c.Marco, c.Puerto, c.CopiasDefault, c.DirectorioLocal,
                c.TimerInicial, c.Online
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Agrega una configuración de impresora fiscal al punto de facturación.
    /// VB6: frmAjustesPV / FISC_CONFIGURACIONES_GENERALES (INSERT).
    /// </summary>
    [HttpPost("{id:long}/configuracion-fiscal")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddConfiguracionFiscal(
        long id, [FromBody] ConfiguracionFiscalRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddConfiguracionFiscalCommand(
                id,
                req.TipoComprobanteId,
                req.TecnologiaId,
                req.InterfazFiscalId,
                req.Marco,
                req.Puerto,
                req.CopiasDefault ?? 2,
                req.ClaveActivacion,
                req.DirectorioLocal,
                req.DirectorioLocalBackup,
                req.TimerInicial,
                req.TimerSincronizacion,
                req.Online ?? false),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetConfiguracionFiscal), new { id }, new { Id = result.Value });
    }

    /// <summary>
    /// Actualiza una configuración de impresora fiscal.
    /// </summary>
    [HttpPut("{id:long}/configuracion-fiscal/{cfgId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConfiguracionFiscal(
        long id, long cfgId, [FromBody] ConfiguracionFiscalRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateConfiguracionFiscalCommand(
                id,
                cfgId,
                req.TecnologiaId,
                req.InterfazFiscalId,
                req.Marco,
                req.Puerto,
                req.CopiasDefault ?? 2,
                req.ClaveActivacion,
                req.DirectorioLocal,
                req.DirectorioLocalBackup,
                req.TimerInicial,
                req.TimerSincronizacion,
                req.Online ?? false),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = "Configuracion no encontrada." });

        return Ok(new { Id = cfgId });
    }

    /// <summary>
    /// Elimina una configuración de impresora fiscal.
    /// VB6: frmAjustesPV / FISC_CONFIGURACIONES_GENERALES (DELETE).
    /// </summary>
    [HttpDelete("{id:long}/configuracion-fiscal/{cfgId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConfiguracionFiscal(
        long id, long cfgId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteConfiguracionFiscalCommand(id, cfgId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = "Configuracion no encontrada." });

        return Ok();
    }

    // ── Tipos de comprobante asignados al punto de facturación ───────────────
    // VB6: frmAjustesPV / TIPOSCOMPROBANTESPUNTOFACTURACION

    [HttpGet("{id:long}/tipos-comprobante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTiposComprobante(long id, CancellationToken ct)
    {
        if (!await db.PuntosFacturacion.AnyAsync(x => x.Id == id, ct)) return NotFound();

        var list = await db.TiposComprobantesPuntoFacturacion
            .Where(x => x.PuntoFacturacionId == id)
            .Select(x => new
            {
                x.Id,
                x.PuntoFacturacionId,
                x.TipoComprobanteId,
                x.NumeroComprobanteProximo,
                x.Editable,
                x.CantidadCopias,
                x.VistaPrevia,
                x.ImprimirControladorFiscal,
                x.PermitirSeleccionMoneda,
                x.VarianteNroUnico,
                x.MascaraMoneda
            })
            .OrderBy(x => x.TipoComprobanteId)
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpPost("{id:long}/tipos-comprobante")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddTipoComprobante(
        long id, [FromBody] TipoComprobantePuntoFacturacionRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddTipoComprobantePuntoFacturacionCommand(
                id,
                req.TipoComprobanteId,
                req.NumeroComprobanteProximo ?? 1,
                req.Editable ?? true,
                req.FilasCantidad ?? 0,
                req.FilasAnchoMaximo ?? 0,
                req.ReporteId,
                req.CantidadCopias ?? 1,
                req.VistaPrevia ?? false,
                req.ImprimirControladorFiscal ?? false,
                req.PermitirSeleccionMoneda ?? false,
                req.VarianteNroUnico,
                req.MascaraMoneda),
            ct);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });

            if (result.Error?.Contains("ya esta asignado", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = "El tipo de comprobante ya esta asignado a este punto de facturacion." });

            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetTiposComprobante), new { id }, new { Id = result.Value });
    }

    [HttpPut("{id:long}/tipos-comprobante/{tcId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTipoComprobante(
        long id, long tcId, [FromBody] TipoComprobantePuntoFacturacionRequest req, CancellationToken ct)
    {
        var current = await db.TiposComprobantesPuntoFacturacion
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tcId && x.PuntoFacturacionId == id, ct);
        if (current is null) return NotFound();

        var result = await Mediator.Send(
            new UpdateTipoComprobantePuntoFacturacionCommand(
                id,
                tcId,
                req.NumeroComprobanteProximo ?? current.NumeroComprobanteProximo,
                req.Editable ?? current.Editable,
                req.FilasCantidad ?? current.FilasCantidad,
                req.FilasAnchoMaximo ?? current.FilasAnchoMaximo,
                req.ReporteId,
                req.CantidadCopias ?? current.CantidadCopias,
                req.VistaPrevia ?? current.VistaPrevia,
                req.ImprimirControladorFiscal ?? current.ImprimirControladorFiscal,
                req.PermitirSeleccionMoneda ?? current.PermitirSeleccionMoneda,
                req.VarianteNroUnico,
                req.MascaraMoneda),
            ct);

        if (!result.IsSuccess)
            return NotFound();

        return Ok(new { Id = tcId });
    }

    [HttpDelete("{id:long}/tipos-comprobante/{tcId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTipoComprobante(long id, long tcId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTipoComprobantePuntoFacturacionCommand(id, tcId), ct);
        if (!result.IsSuccess) return NotFound();

        return NoContent();
    }
}

public record UpdatePuntoFacturacionRequest(long TipoId, string? Descripcion);

public record ConfiguracionFiscalRequest(
    long    TipoComprobanteId,
    long?   TecnologiaId          = null,
    long?   InterfazFiscalId      = null,
    int?    Marco                 = null,
    string? Puerto                = null,
    int?    CopiasDefault         = null,
    string? ClaveActivacion       = null,
    string? DirectorioLocal       = null,
    string? DirectorioLocalBackup = null,
    int?    TimerInicial          = null,
    int?    TimerSincronizacion   = null,
    bool?   Online                = null);

public record TipoComprobantePuntoFacturacionRequest(
    long    TipoComprobanteId,
    long?   NumeroComprobanteProximo    = null,
    bool?   Editable                   = null,
    int?    FilasCantidad              = null,
    int?    FilasAnchoMaximo           = null,
    long?   ReporteId                  = null,
    int?    CantidadCopias             = null,
    bool?   VistaPrevia                = null,
    bool?   ImprimirControladorFiscal  = null,
    bool?   PermitirSeleccionMoneda    = null,
    int?    VarianteNroUnico           = null,
    string? MascaraMoneda              = null);