using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.PuntoVenta.Commands;
using ZuluIA_Back.Application.Features.PuntoVenta.Enums;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class PuntoVentaController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpPost("pos/comprobantes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarPos([FromBody] RegistrarComprobantePosCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetComprobanteById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("punto-venta/comprobantes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarPuntoVenta([FromBody] RegistrarComprobantePuntoVentaCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetComprobanteById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("pos/comprobantes/sifen")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarPosSifen([FromBody] RegistrarFiscalAlternativoRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RegistrarComprobanteFiscalAlternativoCommand(
            CanalOperacionPuntoVenta.Pos,
            TipoFlujoFiscalAlternativo.Sifen,
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Fecha,
            request.FechaVencimiento,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            request.Percepciones,
            request.Observacion,
            request.AfectaStock,
            request.ReferenciaExterna,
            request.Items,
            request.TimbradoFiscalId,
            request.CodigoSeguridad,
            request.RequestPayload,
            request.ResponsePayload,
            request.Confirmada), ct);

        return CreatedFromResult(result, "GetComprobanteById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("punto-venta/comprobantes/sifen")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarPuntoVentaSifen([FromBody] RegistrarFiscalAlternativoRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RegistrarComprobanteFiscalAlternativoCommand(
            CanalOperacionPuntoVenta.PuntoVenta,
            TipoFlujoFiscalAlternativo.Sifen,
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Fecha,
            request.FechaVencimiento,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            request.Percepciones,
            request.Observacion,
            request.AfectaStock,
            request.ReferenciaExterna,
            request.Items,
            request.TimbradoFiscalId,
            request.CodigoSeguridad,
            request.RequestPayload,
            request.ResponsePayload,
            request.Confirmada), ct);

        return CreatedFromResult(result, "GetComprobanteById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("pos/comprobantes/deuce")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarPosDeuce([FromBody] RegistrarFiscalAlternativoRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RegistrarComprobanteFiscalAlternativoCommand(
            CanalOperacionPuntoVenta.Pos,
            TipoFlujoFiscalAlternativo.Deuce,
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Fecha,
            request.FechaVencimiento,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            request.Percepciones,
            request.Observacion,
            request.AfectaStock,
            request.ReferenciaExterna,
            request.Items,
            request.TimbradoFiscalId,
            request.CodigoSeguridad,
            request.RequestPayload,
            request.ResponsePayload,
            request.Confirmada), ct);

        return CreatedFromResult(result, "GetComprobanteById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("punto-venta/comprobantes/deuce")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegistrarPuntoVentaDeuce([FromBody] RegistrarFiscalAlternativoRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RegistrarComprobanteFiscalAlternativoCommand(
            CanalOperacionPuntoVenta.PuntoVenta,
            TipoFlujoFiscalAlternativo.Deuce,
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Fecha,
            request.FechaVencimiento,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            request.Percepciones,
            request.Observacion,
            request.AfectaStock,
            request.ReferenciaExterna,
            request.Items,
            request.TimbradoFiscalId,
            request.CodigoSeguridad,
            request.RequestPayload,
            request.ResponsePayload,
            request.Confirmada), ct);

        return CreatedFromResult(result, "GetComprobanteById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpGet("operaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperaciones([FromQuery] long? sucursalId = null, [FromQuery] long? puntoFacturacionId = null, [FromQuery] long? comprobanteId = null, [FromQuery] string? canal = null, CancellationToken ct = default)
    {
        CanalOperacionPuntoVenta? canalEnum = null;
        if (!string.IsNullOrWhiteSpace(canal) && Enum.TryParse<CanalOperacionPuntoVenta>(canal, true, out var parsed))
            canalEnum = parsed;

        var query = db.OperacionesPuntoVenta.AsNoTracking().Where(x => !x.IsDeleted);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (puntoFacturacionId.HasValue)
            query = query.Where(x => x.PuntoFacturacionId == puntoFacturacionId.Value);
        if (comprobanteId.HasValue)
            query = query.Where(x => x.ComprobanteId == comprobanteId.Value);
        if (canalEnum.HasValue)
            query = query.Where(x => x.Canal == canalEnum.Value);

        return Ok(await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct));
    }

    [HttpGet("operaciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOperacionById(long id, CancellationToken ct)
    {
        var item = await db.OperacionesPuntoVenta.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return OkOrNotFound(item);
    }

    [HttpPost("timbrados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarTimbrado([FromBody] RegistrarTimbradoFiscalCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPut("timbrados/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTimbrado(long id, [FromBody] UpdateTimbradoFiscalCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        return FromResult(await Mediator.Send(command, ct));
    }

    [HttpDelete("timbrados/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DesactivarTimbrado(long id, [FromBody] DesactivarTimbradoRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new DesactivarTimbradoFiscalCommand(id, request.Observacion), ct));

    [HttpGet("timbrados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimbrados([FromQuery] long? puntoFacturacionId = null, [FromQuery] long? sucursalId = null, [FromQuery] bool? activo = null, CancellationToken ct = default)
    {
        var query = db.TimbradosFiscales.AsNoTracking().Where(x => !x.IsDeleted);
        if (puntoFacturacionId.HasValue)
            query = query.Where(x => x.PuntoFacturacionId == puntoFacturacionId.Value);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);
        return Ok(await query.OrderByDescending(x => x.VigenciaDesde).ThenByDescending(x => x.Id).ToListAsync(ct));
    }

    [HttpGet("timbrados/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimbradoById(long id, CancellationToken ct)
    {
        var item = await db.TimbradosFiscales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return OkOrNotFound(item);
    }

    [HttpPost("sifen/comprobantes/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcesarSifen(long comprobanteId, [FromBody] ProcesarSifenRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new ProcesarComprobanteSifenCommand(comprobanteId, request.TimbradoFiscalId, request.RequestPayload, request.ResponsePayload, request.CodigoSeguridad, request.Observacion, request.Confirmada), ct));

    [HttpGet("sifen/operaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSifen([FromQuery] long? comprobanteId = null, [FromQuery] string? estado = null, CancellationToken ct = default)
    {
        EstadoIntegracionFiscalAlternativa? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoIntegracionFiscalAlternativa>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.SifenOperaciones.AsNoTracking().Where(x => !x.IsDeleted);
        if (comprobanteId.HasValue)
            query = query.Where(x => x.ComprobanteId == comprobanteId.Value);
        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);

        return Ok(await query.OrderByDescending(x => x.Id).ToListAsync(ct));
    }

    [HttpGet("sifen/operaciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSifenById(long id, CancellationToken ct)
    {
        var item = await db.SifenOperaciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return OkOrNotFound(item);
    }

    [HttpPost("sifen/operaciones/{id:long}/conciliar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConciliarSifen(long id, [FromBody] ConciliarOperacionFiscalRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new ConciliarSifenOperacionCommand(id, request.Confirmar, request.Observacion), ct));

    [HttpPost("deuce/comprobantes/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcesarDeuce(long comprobanteId, [FromBody] ProcesarDeuceRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new ProcesarComprobanteDeuceCommand(comprobanteId, request.ReferenciaExterna, request.RequestPayload, request.ResponsePayload, request.Observacion, request.Confirmada), ct));

    [HttpGet("deuce/operaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeuce([FromQuery] long? comprobanteId = null, [FromQuery] string? estado = null, CancellationToken ct = default)
    {
        EstadoIntegracionFiscalAlternativa? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoIntegracionFiscalAlternativa>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.DeuceOperaciones.AsNoTracking().Where(x => !x.IsDeleted);
        if (comprobanteId.HasValue)
            query = query.Where(x => x.ComprobanteId == comprobanteId.Value);
        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);

        return Ok(await query.OrderByDescending(x => x.Id).ToListAsync(ct));
    }

    [HttpGet("deuce/operaciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeuceById(long id, CancellationToken ct)
    {
        var item = await db.DeuceOperaciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return OkOrNotFound(item);
    }

    [HttpPost("deuce/operaciones/{id:long}/conciliar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConciliarDeuce(long id, [FromBody] ConciliarOperacionFiscalRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new ConciliarDeuceOperacionCommand(id, request.Confirmar, request.Observacion), ct));

    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen(CancellationToken ct)
    {
        var operaciones = await db.OperacionesPuntoVenta.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);
        var sifen = await db.SifenOperaciones.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);
        var deuce = await db.DeuceOperaciones.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);
        var timbrados = await db.TimbradosFiscales.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);

        return Ok(new
        {
            OperacionesPos = operaciones.Count(x => x.Canal == CanalOperacionPuntoVenta.Pos),
            OperacionesPuntoVenta = operaciones.Count(x => x.Canal == CanalOperacionPuntoVenta.PuntoVenta),
            TimbradosActivos = timbrados.Count(x => x.Activo),
            SifenConfirmadas = sifen.Count(x => x.Estado == EstadoIntegracionFiscalAlternativa.Confirmada),
            SifenRechazadas = sifen.Count(x => x.Estado == EstadoIntegracionFiscalAlternativa.Rechazada),
            DeuceConfirmadas = deuce.Count(x => x.Estado == EstadoIntegracionFiscalAlternativa.Confirmada),
            DeuceRechazadas = deuce.Count(x => x.Estado == EstadoIntegracionFiscalAlternativa.Rechazada)
        });
    }
}

public record RegistrarFiscalAlternativoRequest(long SucursalId, long PuntoFacturacionId, long TipoComprobanteId, DateOnly Fecha, DateOnly? FechaVencimiento, long TerceroId, long MonedaId, decimal Cotizacion, decimal Percepciones, string? Observacion, bool AfectaStock, string? ReferenciaExterna, IReadOnlyList<ComprobanteItemInput> Items, long? TimbradoFiscalId = null, string? CodigoSeguridad = null, string? RequestPayload = null, string? ResponsePayload = null, bool Confirmada = true);
public record DesactivarTimbradoRequest(string? Observacion);
public record ConciliarOperacionFiscalRequest(bool Confirmar, string? Observacion);
public record ProcesarSifenRequest(long? TimbradoFiscalId, string? RequestPayload, string? ResponsePayload, string? CodigoSeguridad, string? Observacion, bool Confirmada = true);
public record ProcesarDeuceRequest(string ReferenciaExterna, string? RequestPayload, string? ResponsePayload, string? Observacion, bool Confirmada = true);
