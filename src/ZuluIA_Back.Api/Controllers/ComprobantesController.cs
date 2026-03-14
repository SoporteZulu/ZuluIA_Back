using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class ComprobantesController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna comprobantes paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] long? tipoComprobanteId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        EstadoComprobante? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoComprobante>(estado, true, out var parsed))
            estadoEnum = parsed;

        var result = await Mediator.Send(
            new GetComprobantesPagedQuery(
                page, pageSize,
                sucursalId, terceroId, tipoComprobanteId,
                estadoEnum, desde, hasta),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de un comprobante con sus ítems
    /// e imputaciones.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetComprobanteById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetComprobanteDetalleQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna los comprobantes con saldo pendiente de un tercero.
    /// Útil para la pantalla de cobros/pagos y para imputaciones.
    /// </summary>
    [HttpGet("saldo-pendiente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSaldoPendiente(
        [FromQuery] long terceroId,
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetSaldoPendienteTerceroQuery(terceroId, sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna los tipos de comprobante disponibles.
    /// </summary>
    [HttpGet("tipos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos(
        [FromQuery] bool? esVenta = null,
        [FromQuery] bool? esCompra = null,
        CancellationToken ct = default)
    {
        var query = db.TiposComprobante.AsNoTracking().Where(x => x.Activo);

        if (esVenta.HasValue)
            query = query.Where(x => x.EsVenta == esVenta.Value);

        if (esCompra.HasValue)
            query = query.Where(x => x.EsCompra == esCompra.Value);

        var tipos = await query
            .OrderBy(x => x.Descripcion)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.EsVenta,
                x.EsCompra,
                x.EsInterno,
                x.AfectaStock,
                x.AfectaCuentaCorriente,
                x.GeneraAsiento,
                x.TipoAfip,
                x.LetraAfip
            })
            .ToListAsync(ct);

        return Ok(tipos);
    }

    /// <summary>
    /// Retorna los estados de comprobante disponibles.
    /// </summary>
    [HttpGet("estados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEstados()
    {
        var estados = Enum.GetValues<EstadoComprobante>()
            .Select(e => new
            {
                valor = e.ToString().ToUpperInvariant(),
                descripcion = e switch
                {
                    EstadoComprobante.Borrador => "Borrador",
                    EstadoComprobante.Emitido => "Emitido",
                    EstadoComprobante.PagadoParcial => "Pagado Parcial",
                    EstadoComprobante.Pagado => "Pagado",
                    EstadoComprobante.Anulado => "Anulado",
                    _ => e.ToString()
                }
            });

        return Ok(estados);
    }

    /// <summary>
    /// Emite un nuevo comprobante.
    /// Valida período IVA, calcula totales y afecta stock si corresponde.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Emitir(
        [FromBody] EmitirComprobanteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetComprobanteById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Asigna el CAE devuelto por AFIP a un comprobante emitido.
    /// </summary>
    [HttpPost("{id:long}/cae")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarCae(
        long id,
        [FromBody] AsignarCaeRequest request,
        CancellationToken ct)
    {
        var comprobante = await db.Comprobantes
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (comprobante is null)
            return NotFound(new { error = $"No se encontró el comprobante ID {id}." });

        comprobante.AsignarCae(request.Cae, request.FechaVto, request.QrData, null);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "CAE asignado correctamente." });
    }

    /// <summary>
    /// Anula un comprobante y opcionalmente revierte el stock.
    /// </summary>
    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(
        long id,
        [FromBody] AnularComprobanteRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AnularComprobanteCommand(id, request.RevertirStock), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Convierte un presupuesto en un comprobante definitivo (ej. Factura A).
    /// Equivale al flujo "Convertir Presupuesto" de frmPreFacturaVenta del VB6.
    /// </summary>
    [HttpPost("{id:long}/convertir-presupuesto")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConvertirPresupuesto(
        long id,
        [FromBody] ConvertirPresupuestoRequest request,
        CancellationToken ct)
    {
        var command = new ConvertirPresupuestoCommand(
            id,
            request.TipoComprobanteDestinoId,
            request.PuntoFacturacionId,
            request.Fecha,
            request.FechaVencimiento,
            request.Observacion);

        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtRoute("GetComprobanteById", new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Imputa un comprobante origen en uno destino (rebaje de saldo).
    /// </summary>
    [HttpPost("imputar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Imputar(
        [FromBody] ImputarComprobanteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Imputa múltiples pares de comprobantes en una sola operación.
    /// Equivale a frmImputacionesVentasMasivas / frmImputacionesComprasMasivas del VB6.
    /// </summary>
    [HttpPost("imputar-masivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImputarMasivo(
        [FromBody] ImputarComprobantesMasivosCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { imputacionesCreadas = result.Value!.Count, ids = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Retorna estadísticas de comprobantes por período y sucursal.
    /// </summary>
    [HttpGet("estadisticas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadisticas(
        [FromQuery] long sucursalId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        CancellationToken ct)
    {
        var estadosValidos = new[]
        {
            EstadoComprobante.Emitido,
            EstadoComprobante.PagadoParcial,
            EstadoComprobante.Pagado
        };

        var comprobantes = await db.Comprobantes
            .AsNoTracking()
            .Where(x =>
                x.SucursalId == sucursalId &&
                x.Fecha      >= desde      &&
                x.Fecha      <= hasta      &&
                estadosValidos.Contains(x.Estado))
            .GroupBy(x => x.TipoComprobanteId)
            .Select(g => new
            {
                TipoComprobanteId = g.Key,
                Cantidad = g.Count(),
                TotalNeto = g.Sum(x => x.NetoGravado + x.NetoNoGravado),
                TotalIva = g.Sum(x => x.IvaRi + x.IvaRni),
                Total = g.Sum(x => x.Total),
                SaldoPendiente = g.Sum(x => x.Saldo)
            })
            .ToListAsync(ct);

        var tipoIds = comprobantes.Select(x => x.TipoComprobanteId).ToList();
        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var resultado = comprobantes.Select(c => new
        {
            TipoComprobanteId = c.TipoComprobanteId,
            TipoComprobanteDescripcion = tipos.GetValueOrDefault(c.TipoComprobanteId)?.Descripcion ?? "—",
            c.Cantidad,
            c.TotalNeto,
            c.TotalIva,
            c.Total,
            c.SaldoPendiente
        });

        return Ok(new
        {
            sucursalId,
            desde,
            hasta,
            porTipo = resultado
        });
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record AsignarCaeRequest(
    string Cae,
    DateOnly FechaVto,
    string? QrData);

public record AnularComprobanteRequest(bool RevertirStock = true);

public record ConvertirPresupuestoRequest(
    long TipoComprobanteDestinoId,
    long? PuntoFacturacionId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion);
