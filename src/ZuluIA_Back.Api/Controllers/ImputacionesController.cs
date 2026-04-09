using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class ImputacionesController(
    IMediator mediator,
    IImputacionRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna una vista agregada de imputaciones de compras para reemplazar el seguimiento local del frontend.
    /// </summary>
    [HttpGet("compras")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompras(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? proveedorId = null,
        [FromQuery] string? estado = null,
        CancellationToken ct = default)
    {
        var tiposCompra = await db.TiposComprobante
            .AsNoTrackingSafe()
            .Where(x => x.Activo && x.EsCompra && x.AfectaCuentaCorriente)
            .Select(x => new
            {
                x.Id,
                x.Descripcion
            })
            .ToListAsync(ct);

        var tipoIds = tiposCompra.Select(x => x.Id).ToList();

        var query = db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => tipoIds.Contains(x.TipoComprobanteId));

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (proveedorId.HasValue)
            query = query.Where(x => x.TerceroId == proveedorId.Value);

        var comprobantes = await query
            .Where(x => x.Estado != Domain.Enums.EstadoComprobante.Anulado && x.Estado != Domain.Enums.EstadoComprobante.Borrador)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        var comprobanteIds = comprobantes.Select(x => x.Id).ToList();
        var proveedoresIds = comprobantes.Select(x => x.TerceroId).Distinct().ToList();
        var monedaIds = comprobantes.Select(x => x.MonedaId).Distinct().ToList();
        var createdByIds = comprobantes.Where(x => x.CreatedBy.HasValue).Select(x => x.CreatedBy!.Value).Distinct().ToList();

        var proveedores = await db.Terceros
            .AsNoTrackingSafe()
            .Where(x => proveedoresIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionarySafeAsync(x => x.Id, x => x.RazonSocial, ct);

        var monedas = await db.Monedas
            .AsNoTrackingSafe()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo })
            .ToDictionarySafeAsync(x => x.Id, x => x.Codigo, ct);

        var usuarios = await db.Usuarios
            .AsNoTrackingSafe()
            .Where(x => createdByIds.Contains(x.Id))
            .Select(x => new { x.Id, Nombre = x.NombreCompleto ?? x.UserName })
            .ToDictionarySafeAsync(x => x.Id, x => x.Nombre, ct);

        var ordenes = await db.OrdenesCompraMeta
            .AsNoTrackingSafe()
            .Where(x => comprobanteIds.Contains(x.ComprobanteId))
            .ToDictionarySafeAsync(x => x.ComprobanteId, ct);

        var recepciones = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => x.ComprobanteOrigenId.HasValue && comprobanteIds.Contains(x.ComprobanteOrigenId.Value))
            .Select(x => new
            {
                x.Id,
                x.ComprobanteOrigenId,
                Numero = x.Numero.Formateado,
                x.Fecha
            })
            .ToListAsync(ct);

        var imputaciones = await db.Imputaciones
            .AsNoTrackingSafe()
            .Where(x => comprobanteIds.Contains(x.ComprobanteDestinoId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var origenIds = imputaciones.Select(x => x.ComprobanteOrigenId).Distinct().ToList();
        var comprobantesOrigen = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.TipoComprobanteId,
                x.TerceroId,
                Numero = x.Numero.Formateado
            })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var tiposOrigenIds = comprobantesOrigen.Values.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var tiposOrigen = await db.TiposComprobante
            .AsNoTrackingSafe()
            .Where(x => tiposOrigenIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionarySafeAsync(x => x.Id, x => x.Descripcion, ct);

        var tercerosOrigenIds = comprobantesOrigen.Values.Select(x => x.TerceroId).Distinct().ToList();
        var tercerosOrigen = await db.Terceros
            .AsNoTrackingSafe()
            .Where(x => tercerosOrigenIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionarySafeAsync(x => x.Id, x => x.RazonSocial, ct);

        var tiposCompraMap = tiposCompra.ToDictionary(x => x.Id, x => x.Descripcion);

        var rows = comprobantes
            .Select(comprobante =>
            {
                var activas = imputaciones
                    .Where(x => x.ComprobanteDestinoId == comprobante.Id && !x.Anulada)
                    .ToList();
                var anuladas = imputaciones
                    .Where(x => x.ComprobanteDestinoId == comprobante.Id && x.Anulada)
                    .ToList();

                var totalImputado = activas.Sum(x => x.Importe);
                var recepcion = recepciones
                    .Where(x => x.ComprobanteOrigenId == comprobante.Id)
                    .OrderByDescending(x => x.Fecha)
                    .FirstOrDefault();

                var tipoDescripcion = tiposCompraMap.GetValueOrDefault(comprobante.TipoComprobanteId, "Compra");
                var tipo = tipoDescripcion.Contains("import", StringComparison.OrdinalIgnoreCase)
                    ? "Importación"
                    : "Compras";

                var estadoLegado = comprobante.Saldo <= 0
                    ? "IMPUTADA"
                    : (anuladas.Count > 0 || activas.Count > 0 ? "OBSERVADA" : "PENDIENTE");

                var distribucion = activas.Count > 0
                    ? activas.Select(x =>
                    {
                        var origen = comprobantesOrigen.GetValueOrDefault(x.ComprobanteOrigenId);
                        var porcentaje = comprobante.Total == 0
                            ? 0m
                            : Math.Round((x.Importe / comprobante.Total) * 100m, 2);

                        return new CompraImputacionDistribucionDto(
                            x.Id.ToString(CultureInfo.InvariantCulture),
                            origen is not null ? tiposOrigen.GetValueOrDefault(origen.TipoComprobanteId, "Aplicación") : "Aplicación",
                            origen is not null ? tercerosOrigen.GetValueOrDefault(origen.TerceroId, "Documento origen") : "Documento origen",
                            porcentaje,
                            x.Importe);
                    }).ToList()
                    :
                    [
                        new CompraImputacionDistribucionDto(
                            $"pendiente-{comprobante.Id}",
                            tipoDescripcion,
                            "Pendiente de imputación",
                            100m,
                            comprobante.Total)
                    ];

                var detalles = new List<string>
                {
                    $"Saldo pendiente: {comprobante.Saldo:0.##} {monedas.GetValueOrDefault(comprobante.MonedaId, "ARS")}",
                    $"Total imputado activo: {totalImputado:0.##} {monedas.GetValueOrDefault(comprobante.MonedaId, "ARS")}",
                    activas.Count > 0
                        ? $"Aplicaciones activas: {activas.Count}"
                        : "Sin aplicaciones activas registradas."
                };

                if (anuladas.Count > 0)
                    detalles.Add($"Desimputaciones detectadas: {anuladas.Count}");

                if (recepcion is not null)
                    detalles.Add($"Última recepción visible: {recepcion.Numero}");

                if (ordenes.TryGetValue(comprobante.Id, out var orden))
                    detalles.Add($"Orden asociada: OC-{orden.Id}");

                return new CompraImputacionResumenDto(
                    comprobante.Id,
                    tipo,
                    proveedores.GetValueOrDefault(comprobante.TerceroId, $"Proveedor #{comprobante.TerceroId}"),
                    comprobante.Numero.Formateado,
                    distribucion[0].Cuenta,
                    distribucion[0].CentroCosto,
                    estadoLegado,
                    comprobante.Fecha,
                    comprobante.Total,
                    ordenes.TryGetValue(comprobante.Id, out var ordenCompra) ? $"OC-{ordenCompra.Id}" : null,
                    recepcion?.Numero,
                    comprobante.CreatedBy.HasValue ? usuarios.GetValueOrDefault(comprobante.CreatedBy.Value, "Sin responsable") : "Sin responsable",
                    monedas.GetValueOrDefault(comprobante.MonedaId, "ARS"),
                    tipoDescripcion,
                    comprobante.Observacion ?? "Sin observaciones registradas.",
                    detalles,
                    distribucion);
            })
            .Where(x => string.IsNullOrWhiteSpace(estado) || string.Equals(x.Estado, estado, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Ok(rows);
    }

    /// <summary>
    /// Retorna las imputaciones de un comprobante como origen
    /// (lo que este comprobante cancela en otros).
    /// </summary>
    [HttpGet("origen/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrigen(
        long comprobanteId,
        CancellationToken ct,
        [FromQuery] long? tipoComprobanteDestinoId = null,
        [FromQuery] bool incluirAnuladas = false)
    {
        var imputaciones = await repo.GetByComprobanteOrigenAsync(comprobanteId, incluirAnuladas, ct);

        var destinoIds = imputaciones
            .Select(x => x.ComprobanteDestinoId)
            .Distinct()
            .ToList();

        var numeros = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => destinoIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.TipoComprobanteId,
                x.Numero.Prefijo,
                x.Numero.Numero
            })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var tipoIds = numeros.Values.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var tipos = await db.TiposComprobante
            .AsNoTrackingSafe()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionarySafeAsync(x => x.Id, x => x.Descripcion, ct);

        if (tipoComprobanteDestinoId.HasValue)
            imputaciones = imputaciones
                .Where(x => numeros.TryGetValue(x.ComprobanteDestinoId, out var n) && n.TipoComprobanteId == tipoComprobanteDestinoId.Value)
                .ToList();

        var origen = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => x.Id == comprobanteId)
            .Select(x => new { x.Numero.Prefijo, x.Numero.Numero, x.TipoComprobanteId })
            .FirstOrDefaultSafeAsync(ct);

        var tipoOrigen = origen is not null
            ? await db.TiposComprobante
                .AsNoTrackingSafe()
                .Where(x => x.Id == origen.TipoComprobanteId)
                .Select(x => x.Descripcion)
                .FirstOrDefaultSafeAsync(ct)
            : null;

        var dtos = imputaciones.Select(i => new ImputacionDto
        {
            Id                   = i.Id,
            ComprobanteOrigenId  = i.ComprobanteOrigenId,
            NumeroOrigen         = origen is not null
                ? $"{origen.Prefijo:D4}-{origen.Numero:D8}"
                : "—",
            TipoComprobanteOrigenId = origen?.TipoComprobanteId,
            TipoComprobanteOrigen = tipoOrigen ?? "—",
            ComprobanteDestinoId = i.ComprobanteDestinoId,
            NumeroDestino        = numeros.ContainsKey(i.ComprobanteDestinoId)
                ? $"{numeros[i.ComprobanteDestinoId].Prefijo:D4}-{numeros[i.ComprobanteDestinoId].Numero:D8}"
                : "—",
            TipoComprobanteDestinoId = numeros.GetValueOrDefault(i.ComprobanteDestinoId)?.TipoComprobanteId,
            TipoComprobanteDestino = numeros.TryGetValue(i.ComprobanteDestinoId, out var destinoInfo)
                ? tipos.GetValueOrDefault(destinoInfo.TipoComprobanteId, "—")
                : "—",
            Importe              = i.Importe,
            Fecha                = i.Fecha,
            CreatedAt            = i.CreatedAt,
            Anulada              = i.Anulada,
            FechaDesimputacion   = i.FechaDesimputacion,
            MotivoDesimputacion  = i.MotivoDesimputacion,
            DesimputadaAt        = i.DesimputadaAt,
            RolComprobante       = "ORIGEN"
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Retorna las imputaciones de un comprobante como destino
    /// (lo que otros comprobantes le cancelan a este).
    /// </summary>
    [HttpGet("destino/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDestino(
        long comprobanteId,
        CancellationToken ct,
        [FromQuery] long? tipoComprobanteOrigenId = null,
        [FromQuery] bool incluirAnuladas = false)
    {
        var imputaciones = await repo.GetByComprobanteDestinoAsync(comprobanteId, incluirAnuladas, ct);

        var origenIds = imputaciones
            .Select(x => x.ComprobanteOrigenId)
            .Distinct()
            .ToList();

        var numeros = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, x.TipoComprobanteId, x.Numero.Prefijo, x.Numero.Numero })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var tipoIds = numeros.Values.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var tipos = await db.TiposComprobante
            .AsNoTrackingSafe()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionarySafeAsync(x => x.Id, x => x.Descripcion, ct);

        if (tipoComprobanteOrigenId.HasValue)
            imputaciones = imputaciones
                .Where(x => numeros.TryGetValue(x.ComprobanteOrigenId, out var n) && n.TipoComprobanteId == tipoComprobanteOrigenId.Value)
                .ToList();

        var destino = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => x.Id == comprobanteId)
            .Select(x => new { x.Numero.Prefijo, x.Numero.Numero, x.TipoComprobanteId })
            .FirstOrDefaultSafeAsync(ct);

        var tipoDestino = destino is not null
            ? await db.TiposComprobante
                .AsNoTrackingSafe()
                .Where(x => x.Id == destino.TipoComprobanteId)
                .Select(x => x.Descripcion)
                .FirstOrDefaultSafeAsync(ct)
            : null;

        var dtos = imputaciones.Select(i => new ImputacionDto
        {
            Id                   = i.Id,
            ComprobanteOrigenId  = i.ComprobanteOrigenId,
            NumeroOrigen         = numeros.ContainsKey(i.ComprobanteOrigenId)
                ? $"{numeros[i.ComprobanteOrigenId].Prefijo:D4}-{numeros[i.ComprobanteOrigenId].Numero:D8}"
                : "—",
            TipoComprobanteOrigenId = numeros.GetValueOrDefault(i.ComprobanteOrigenId)?.TipoComprobanteId,
            TipoComprobanteOrigen = numeros.TryGetValue(i.ComprobanteOrigenId, out var origenInfo)
                ? tipos.GetValueOrDefault(origenInfo.TipoComprobanteId, "—")
                : "—",
            ComprobanteDestinoId = i.ComprobanteDestinoId,
            NumeroDestino        = destino is not null
                ? $"{destino.Prefijo:D4}-{destino.Numero:D8}"
                : "—",
            TipoComprobanteDestinoId = destino?.TipoComprobanteId,
            TipoComprobanteDestino = tipoDestino ?? "—",
            Importe              = i.Importe,
            Fecha                = i.Fecha,
            CreatedAt            = i.CreatedAt,
            Anulada              = i.Anulada,
            FechaDesimputacion   = i.FechaDesimputacion,
            MotivoDesimputacion  = i.MotivoDesimputacion,
            DesimputadaAt        = i.DesimputadaAt,
            RolComprobante       = "DESTINO"
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("historial/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorial(
        long comprobanteId,
        [FromQuery] bool incluirAnuladas = true,
        [FromQuery] long? tipoComprobanteRelacionadoId = null,
        CancellationToken ct = default)
    {
        var historial = await repo.GetHistorialByComprobanteAsync(comprobanteId, incluirAnuladas, ct);
        var relacionadosIds = historial
            .SelectMany(x => new[] { x.ComprobanteOrigenId, x.ComprobanteDestinoId })
            .Distinct()
            .ToList();

        var comprobantes = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => relacionadosIds.Contains(x.Id))
            .Select(x => new { x.Id, x.TipoComprobanteId, x.Numero.Prefijo, x.Numero.Numero })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var tipoIds = comprobantes.Values.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var tipos = await db.TiposComprobante
            .AsNoTrackingSafe()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionarySafeAsync(x => x.Id, x => x.Descripcion, ct);

        var dtos = historial
            .Where(x => !tipoComprobanteRelacionadoId.HasValue ||
                (x.ComprobanteOrigenId != comprobanteId && comprobantes.GetValueOrDefault(x.ComprobanteOrigenId)?.TipoComprobanteId == tipoComprobanteRelacionadoId.Value) ||
                (x.ComprobanteDestinoId != comprobanteId && comprobantes.GetValueOrDefault(x.ComprobanteDestinoId)?.TipoComprobanteId == tipoComprobanteRelacionadoId.Value))
            .Select(i =>
            {
                var rol = i.ComprobanteOrigenId == comprobanteId ? "ORIGEN" : "DESTINO";
                var origenInfo = comprobantes.GetValueOrDefault(i.ComprobanteOrigenId);
                var destinoInfo = comprobantes.GetValueOrDefault(i.ComprobanteDestinoId);

                return new ImputacionDto
                {
                    Id = i.Id,
                    ComprobanteOrigenId = i.ComprobanteOrigenId,
                    NumeroOrigen = origenInfo is not null ? $"{origenInfo.Prefijo:D4}-{origenInfo.Numero:D8}" : "—",
                    TipoComprobanteOrigenId = origenInfo?.TipoComprobanteId,
                    TipoComprobanteOrigen = origenInfo is not null ? tipos.GetValueOrDefault(origenInfo.TipoComprobanteId, "—") : "—",
                    ComprobanteDestinoId = i.ComprobanteDestinoId,
                    NumeroDestino = destinoInfo is not null ? $"{destinoInfo.Prefijo:D4}-{destinoInfo.Numero:D8}" : "—",
                    TipoComprobanteDestinoId = destinoInfo?.TipoComprobanteId,
                    TipoComprobanteDestino = destinoInfo is not null ? tipos.GetValueOrDefault(destinoInfo.TipoComprobanteId, "—") : "—",
                    Importe = i.Importe,
                    Fecha = i.Fecha,
                    CreatedAt = i.CreatedAt,
                    Anulada = i.Anulada,
                    FechaDesimputacion = i.FechaDesimputacion,
                    MotivoDesimputacion = i.MotivoDesimputacion,
                    DesimputadaAt = i.DesimputadaAt,
                    RolComprobante = rol
                };
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Retorna el total imputado de un comprobante destino.
    /// </summary>
    [HttpGet("total-imputado/{comprobanteId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalImputado(
        long comprobanteId,
        CancellationToken ct)
    {
        var total = await repo.GetTotalImputadoAsync(comprobanteId, ct);
        return Ok(new { comprobanteId, totalImputado = total });
    }

    /// <summary>
    /// Imputa un comprobante origen contra un destino.
    /// Actualiza los saldos de ambos comprobantes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Imputar(
        [FromBody] ImputarComprobanteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new
            {
                imputacionId = result.Value,
                mensaje = "Imputación registrada correctamente."
            })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("desimputar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desimputar(
        [FromBody] DesimputarComprobanteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { imputacionId = result.Value, mensaje = "Imputación deshecha correctamente." })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("desimputar-masivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DesimputarMasivo(
        [FromBody] DesimputarComprobantesMasivosCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { desimputaciones = result.Value.Count, ids = result.Value })
            : BadRequest(new { error = result.Error });
    }
}

public sealed record CompraImputacionResumenDto(
    long Id,
    string Tipo,
    string Proveedor,
    string Comprobante,
    string Cuenta,
    string CentroCosto,
    string Estado,
    DateOnly Fecha,
    decimal Importe,
    string? OrdenCompraReferencia,
    string? RecepcionReferencia,
    string Responsable,
    string Moneda,
    string CircuitoOrigen,
    string Observacion,
    IReadOnlyList<string> DetallesClave,
    IReadOnlyList<CompraImputacionDistribucionDto> Distribucion);

public sealed record CompraImputacionDistribucionDto(
    string Id,
    string Cuenta,
    string CentroCosto,
    decimal Porcentaje,
    decimal Importe);