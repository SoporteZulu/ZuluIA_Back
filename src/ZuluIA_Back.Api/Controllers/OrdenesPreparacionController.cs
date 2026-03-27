using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Queries;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de órdenes de preparación / picking para despacho de mercaderías.
/// Equivale a frmOrdenDePreparacion / clsOrdenDePreparacion del sistema VB6.
/// </summary>
public class OrdenesPreparacionController(IMediator mediator, IApplicationDbContext db, ReporteExportacionService reporteExportacionService) : BaseController(mediator)
{
    /// <summary>
    /// Retorna las órdenes de preparación paginadas con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] EstadoOrdenPreparacion? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetOrdenesPreparacionPagedQuery(page, pageSize, sucursalId, terceroId, estado, desde, hasta), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de una orden de preparación.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetOrdenPreparacionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrdenPreparacionByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Crea una nueva orden de preparación en estado Pendiente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrdenPreparacionCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtRoute("GetOrdenPreparacionById", new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Confirma una orden de preparación (la marca como Completada).
    /// </summary>
    [HttpPost("{id:long}/confirmar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirmar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ConfirmarOrdenPreparacionCommand(id), ct);
        return FromResult(result);
    }

    [HttpPost("{id:long}/iniciar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Iniciar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new IniciarOrdenPreparacionCommand(id), ct);
        return FromResult(result);
    }

    [HttpPost("{id:long}/picking")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarPicking(long id, [FromBody] RegistrarPickingOrdenPreparacionBody request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RegistrarPickingOrdenPreparacionCommand(id, request.Detalles), ct);
        return FromResult(result);
    }

    [HttpPost("{id:long}/despachar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Despachar(long id, [FromBody] DespacharOrdenPreparacionBody request, CancellationToken ct)
        => FromResult(await Mediator.Send(new DespacharOrdenPreparacionCommand(id, request.DepositoDestinoId, request.Fecha, request.Observacion), ct));

    [HttpGet("{id:long}/eventos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventos(long id, CancellationToken ct)
    {
        var eventos = await db.LogisticaInternaEventos.AsNoTracking()
            .Where(x => x.OrdenPreparacionId == id && !x.IsDeleted)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        return Ok(eventos.Select(x => new
        {
            x.Id,
            Tipo = x.TipoEvento.ToString().ToUpperInvariant(),
            x.Fecha,
            x.Descripcion,
            x.CreatedAt
        }));
    }

    [HttpGet("{id:long}/trazabilidad")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrazabilidad(long id, CancellationToken ct)
    {
        var orden = await db.OrdenesPreparacion.AsNoTracking().Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (orden is null)
            return NotFound(new { error = $"No se encontró la orden de preparación con ID {id}." });

        var eventos = await db.LogisticaInternaEventos.AsNoTracking()
            .Where(x => x.OrdenPreparacionId == id && !x.IsDeleted)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        var transferencias = await db.TransferenciasDeposito.AsNoTracking()
            .Where(x => x.OrdenPreparacionId == id && !x.IsDeleted)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        var transferenciaIds = transferencias.Select(x => x.Id).ToList();
        var eventosTransferencia = transferenciaIds.Count == 0
            ? []
            : await db.LogisticaInternaEventos.AsNoTracking()
                .Where(x => x.TransferenciaDepositoId.HasValue && transferenciaIds.Contains(x.TransferenciaDepositoId.Value) && !x.IsDeleted)
                .OrderBy(x => x.Fecha)
                .ThenBy(x => x.Id)
                .ToListAsync(ct);

        var timeline = eventos
            .Concat(eventosTransferencia)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .ToList();

        return Ok(new
        {
            orden.Id,
            Estado = orden.Estado.ToString().ToUpperInvariant(),
            orden.Fecha,
            orden.FechaConfirmacion,
            CantidadRenglones = orden.Detalles.Count,
            CantidadSolicitada = orden.Detalles.Sum(x => x.Cantidad),
            CantidadEntregada = orden.Detalles.Sum(x => x.CantidadEntregada),
            Transferencias = transferencias.Select(x => new
            {
                x.Id,
                x.DepositoOrigenId,
                x.DepositoDestinoId,
                Estado = x.Estado.ToString().ToUpperInvariant(),
                x.Fecha,
                x.FechaConfirmacion,
                Eventos = eventosTransferencia.Where(e => e.TransferenciaDepositoId == x.Id).Select(e => new
                {
                    e.Id,
                    Tipo = e.TipoEvento.ToString().ToUpperInvariant(),
                    e.Fecha,
                    e.Descripcion,
                    e.CreatedAt
                })
            }),
            Eventos = eventos.Select(x => new
            {
                x.Id,
                Tipo = x.TipoEvento.ToString().ToUpperInvariant(),
                x.Fecha,
                x.Descripcion,
                x.CreatedAt
            }),
            Timeline = timeline.Select(x => new
            {
                x.Id,
                Tipo = x.TipoEvento.ToString().ToUpperInvariant(),
                x.Fecha,
                x.Descripcion,
                x.OrdenPreparacionId,
                x.TransferenciaDepositoId,
                x.CreatedAt
            })
        });
    }

    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen([FromQuery] long? sucursalId = null, CancellationToken ct = default)
    {
        var ordenes = db.OrdenesPreparacion.AsNoTracking().Include(x => x.Detalles).Where(x => !x.IsDeleted);
        if (sucursalId.HasValue)
            ordenes = ordenes.Where(x => x.SucursalId == sucursalId.Value);

        var items = await ordenes.ToListAsync(ct);
        return Ok(new
        {
            Cantidad = items.Count,
            Pendientes = items.Count(x => x.Estado == EstadoOrdenPreparacion.Pendiente),
            EnProceso = items.Count(x => x.Estado == EstadoOrdenPreparacion.EnProceso),
            Completadas = items.Count(x => x.Estado == EstadoOrdenPreparacion.Completada),
            Anuladas = items.Count(x => x.Estado == EstadoOrdenPreparacion.Anulada),
            CantidadSolicitada = items.SelectMany(x => x.Detalles).Sum(x => x.Cantidad),
            CantidadEntregada = items.SelectMany(x => x.Detalles).Sum(x => x.CantidadEntregada)
        });
    }

    [HttpGet("reportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporte(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] EstadoOrdenPreparacion? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv,
        CancellationToken ct = default)
    {
        var query = db.OrdenesPreparacion.AsNoTracking().Include(x => x.Detalles).Where(x => !x.IsDeleted);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);
        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        var items = await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var reporte = new ReporteTabularDto
        {
            Titulo = "Ordenes de Preparacion",
            Parametros = new Dictionary<string, string>
            {
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas",
                ["TerceroId"] = terceroId?.ToString() ?? "Todos",
                ["Estado"] = estado?.ToString().ToUpperInvariant() ?? "Todos",
                ["Desde"] = desde?.ToString("yyyy-MM-dd") ?? "—",
                ["Hasta"] = hasta?.ToString("yyyy-MM-dd") ?? "—"
            },
            Columnas = ["Id", "SucursalId", "TerceroId", "ComprobanteOrigenId", "Fecha", "Estado", "FechaConfirmacion", "Renglones", "CantidadSolicitada", "CantidadEntregada", "Observacion"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.SucursalId.ToString(),
                x.TerceroId?.ToString() ?? "—",
                x.ComprobanteOrigenId?.ToString() ?? "—",
                x.Fecha.ToString("yyyy-MM-dd"),
                x.Estado.ToString().ToUpperInvariant(),
                x.FechaConfirmacion?.ToString("yyyy-MM-dd") ?? "—",
                x.Detalles.Count.ToString(),
                x.Detalles.Sum(d => d.Cantidad).ToString("0.##"),
                x.Detalles.Sum(d => d.CantidadEntregada).ToString("0.##"),
                x.Observacion ?? "—"
            }).ToList().AsReadOnly()
        };

        var archivo = reporteExportacionService.Exportar(reporte, formato, "ordenes_preparacion");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    /// <summary>
    /// Anula una orden de preparación.
    /// </summary>
    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularOrdenPreparacionCommand(id), ct);
        return FromResult(result);
    }
}

public record RegistrarPickingOrdenPreparacionBody(IReadOnlyList<RegistrarPickingDetalleInput> Detalles);
public record DespacharOrdenPreparacionBody(long DepositoDestinoId, DateOnly Fecha, string? Observacion);
