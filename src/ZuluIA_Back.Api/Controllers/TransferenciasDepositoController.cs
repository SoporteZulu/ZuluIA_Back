using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class TransferenciasDepositoController(IMediator mediator, IApplicationDbContext db, IServiceProvider serviceProvider) : BaseController(mediator)
{
    private ReporteExportacionService reporteExportacionService => serviceProvider.GetRequiredService<ReporteExportacionService>();

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] long? sucursalId = null, [FromQuery] long? ordenPreparacionId = null, [FromQuery] string? estado = null, CancellationToken ct = default)
    {
        ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.TransferenciasDeposito.AsNoTracking().Where(x => !x.IsDeleted);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (ordenPreparacionId.HasValue)
            query = query.Where(x => x.OrdenPreparacionId == ordenPreparacionId.Value);
        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);

        var items = await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:long}", Name = "GetTransferenciaDepositoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.TransferenciasDeposito.AsNoTracking().Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return OkOrNotFound(item);
    }

    [HttpGet("{id:long}/trazabilidad")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrazabilidad(long id, CancellationToken ct)
    {
        var transferencia = await db.TransferenciasDeposito.AsNoTracking().Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (transferencia is null)
            return NotFound(new { error = $"No se encontró la transferencia de depósito con ID {id}." });

        var eventos = await db.LogisticaInternaEventos.AsNoTracking()
            .Where(x => x.TransferenciaDepositoId == id && !x.IsDeleted)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        var orden = transferencia.OrdenPreparacionId.HasValue
            ? await db.OrdenesPreparacion.AsNoTracking().FirstOrDefaultAsync(x => x.Id == transferencia.OrdenPreparacionId.Value && !x.IsDeleted, ct)
            : null;

        var eventosOrden = transferencia.OrdenPreparacionId.HasValue
            ? await db.LogisticaInternaEventos.AsNoTracking()
                .Where(x => x.OrdenPreparacionId == transferencia.OrdenPreparacionId.Value && !x.IsDeleted)
                .OrderBy(x => x.Fecha)
                .ThenBy(x => x.Id)
                .ToListAsync(ct)
            : [];

        var timeline = eventos
            .Concat(eventosOrden)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .ToList();

        return Ok(new
        {
            transferencia.Id,
            transferencia.OrdenPreparacionId,
            transferencia.SucursalId,
            transferencia.DepositoOrigenId,
            transferencia.DepositoDestinoId,
            Estado = transferencia.Estado.ToString().ToUpperInvariant(),
            transferencia.Fecha,
            transferencia.FechaDespacho,
            transferencia.FechaConfirmacion,
            CantidadRenglones = transferencia.Detalles.Count,
            CantidadItems = transferencia.Detalles.Sum(x => x.Cantidad),
            OrdenPreparacion = orden is null ? null : new
            {
                orden.Id,
                Estado = orden.Estado.ToString().ToUpperInvariant(),
                orden.Fecha,
                orden.FechaConfirmacion,
                Eventos = eventosOrden.Select(x => new
                {
                    x.Id,
                    Tipo = x.TipoEvento.ToString().ToUpperInvariant(),
                    x.Fecha,
                    x.Descripcion,
                    x.CreatedAt
                })
            },
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTransferenciaDepositoCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetTransferenciaDepositoById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("{id:long}/confirmar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Confirmar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new ConfirmarTransferenciaDepositoCommand(id), ct));

    [HttpPost("{id:long}/despachar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Despachar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DespacharTransferenciaDepositoCommand(id), ct));

    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AnularTransferenciaDepositoCommand(id), ct));

    [HttpGet("{id:long}/eventos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventos(long id, CancellationToken ct)
    {
        var eventos = await db.LogisticaInternaEventos.AsNoTracking()
            .Where(x => x.TransferenciaDepositoId == id && !x.IsDeleted)
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

    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen([FromQuery] long? sucursalId = null, CancellationToken ct = default)
    {
        var transferencias = db.TransferenciasDeposito.AsNoTracking().Include(x => x.Detalles).Where(x => !x.IsDeleted);
        if (sucursalId.HasValue)
            transferencias = transferencias.Where(x => x.SucursalId == sucursalId.Value);

        var items = await transferencias.ToListAsync(ct);
        return Ok(new
        {
            Cantidad = items.Count,
            Borrador = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito.Borrador),
            EnTransito = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito.EnTransito),
            Confirmadas = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito.Confirmada),
            Anuladas = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito.Anulada),
            VinculadasAOrdenPreparacion = items.Count(x => x.OrdenPreparacionId.HasValue),
            CantidadItems = items.SelectMany(x => x.Detalles).Sum(x => x.Cantidad)
        });
    }

    [HttpGet("reportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporte(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? ordenPreparacionId = null,
        [FromQuery] string? estado = null,
        [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv,
        CancellationToken ct = default)
    {
        ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<ZuluIA_Back.Domain.Enums.EstadoTransferenciaDeposito>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.TransferenciasDeposito.AsNoTracking().Include(x => x.Detalles).Where(x => !x.IsDeleted);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (ordenPreparacionId.HasValue)
            query = query.Where(x => x.OrdenPreparacionId == ordenPreparacionId.Value);
        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);

        var items = await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var reporte = new ReporteTabularDto
        {
            Titulo = "Transferencias de Deposito",
            Parametros = new Dictionary<string, string>
            {
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas",
                ["OrdenPreparacionId"] = ordenPreparacionId?.ToString() ?? "Todas",
                ["Estado"] = estadoEnum?.ToString().ToUpperInvariant() ?? "Todos"
            },
            Columnas = ["Id", "SucursalId", "OrdenPreparacionId", "DepositoOrigenId", "DepositoDestinoId", "Fecha", "FechaDespacho", "Estado", "FechaConfirmacion", "Renglones", "CantidadItems", "Observacion"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.SucursalId.ToString(),
                x.OrdenPreparacionId?.ToString() ?? "—",
                x.DepositoOrigenId.ToString(),
                x.DepositoDestinoId.ToString(),
                x.Fecha.ToString("yyyy-MM-dd"),
                x.FechaDespacho?.ToString("yyyy-MM-dd") ?? "—",
                x.Estado.ToString().ToUpperInvariant(),
                x.FechaConfirmacion?.ToString("yyyy-MM-dd") ?? "—",
                x.Detalles.Count.ToString(),
                x.Detalles.Sum(d => d.Cantidad).ToString("0.##"),
                x.Observacion ?? "—"
            }).ToList().AsReadOnly()
        };

        var archivo = reporteExportacionService.Exportar(reporte, formato, "transferencias_deposito");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }
}
