using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Auditoria.Queries;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Queries;
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
    /// Retorna el comprobante por ID en formato resumido.
    /// </summary>
    [HttpGet("{id:long}/resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResumenById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetComprobanteByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna un payload dedicado para reimpresion del comprobante ya emitido.
    /// Reutiliza el detalle completo para que frontend o tooling documental generen una copia imprimible.
    /// </summary>
    [HttpGet("{id:long}/reimpresion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReimpresion(long id, CancellationToken ct)
    {
        var detalle = await Mediator.Send(new GetComprobanteDetalleQuery(id), ct);
        if (detalle is null)
            return NotFound();

        return Ok(new ComprobanteReimpresionResponse(true, DateTimeOffset.UtcNow, detalle));
    }

    /// <summary>
    /// Prepara y valida la informacion necesaria para enviar un comprobante Paraguay a SIFEN/SET.
    /// No transmite el documento; solo devuelve el paquete listo para integracion.
    /// </summary>
    [HttpGet("{id:long}/paraguay/sifen-preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSifenPreview(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new PrepararSifenParaguayQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna el ultimo estado SIFEN persistido para un comprobante Paraguay.
    /// </summary>
    [HttpGet("{id:long}/paraguay/sifen-estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSifenEstado(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetComprobanteSifenEstadoQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna el historial de intentos y respuestas SIFEN persistidos para un comprobante Paraguay.
    /// </summary>
    [HttpGet("{id:long}/paraguay/sifen-historial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSifenHistorial(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetComprobanteSifenHistorialQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Envia un comprobante Paraguay al integrador SIFEN/SET configurado y registra auditoria del intento.
    /// </summary>
    [HttpPost("{id:long}/paraguay/sifen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SolicitarSifenParaguay(long id, CancellationToken ct)
    {
        var preview = await Mediator.Send(new PrepararSifenParaguayQuery(id), ct);
        if (preview is null)
            return NotFound();

        var result = await Mediator.Send(new SolicitarSifenParaguayComprobanteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reintenta el envio SIFEN/SET de un comprobante Paraguay cuando el ultimo estado fue rechazo o error.
    /// </summary>
    [HttpPost("{id:long}/paraguay/sifen/reintentar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReintentarSifenParaguay(long id, CancellationToken ct)
    {
        var estado = await Mediator.Send(new GetComprobanteSifenEstadoQuery(id), ct);
        if (estado is null)
            return NotFound();

        var result = await Mediator.Send(new ReintentarSifenParaguayComprobanteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reintenta en lote comprobantes Paraguay con ultimo estado rechazado o error.
    /// Usa filtros operativos compatibles con la bandeja para recortar el lote antes del reenvio.
    /// </summary>
    [HttpGet("paraguay/sifen/reintentar-pendientes/preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PreviewReintentarSifenParaguayPendientes(
        [FromQuery] int maxItems = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estadoSifen = null,
        [FromQuery] string? codigoRespuesta = null,
        [FromQuery] DateOnly? fechaDesde = null,
        [FromQuery] DateOnly? fechaHasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new PreviewReintentarSifenParaguayPendientesQuery(
                maxItems,
                sucursalId,
                estadoSifen,
                codigoRespuesta,
                fechaDesde,
                fechaHasta),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Reintenta en lote comprobantes Paraguay con ultimo estado rechazado o error.
    /// Usa filtros operativos compatibles con la bandeja para recortar el lote antes del reenvio.
    /// </summary>
    [HttpPost("paraguay/sifen/reintentar-pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReintentarSifenParaguayPendientes(
        [FromBody] ReintentarSifenParaguayPendientesRequest? request,
        CancellationToken ct)
    {
        var command = new ReintentarSifenParaguayPendientesCommand(
            request?.MaxItems ?? 20,
            request?.SucursalId,
            request?.EstadoSifen,
            request?.CodigoRespuesta,
            request?.FechaDesde,
            request?.FechaHasta);

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Consulta el estado actual SIFEN/SET del comprobante usando TrackingId, CDC o numero de lote persistidos.
    /// </summary>
    [HttpPost("{id:long}/paraguay/sifen/conciliar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConciliarSifenParaguay(long id, CancellationToken ct)
    {
        var estado = await Mediator.Send(new GetComprobanteSifenEstadoQuery(id), ct);
        if (estado is null)
            return NotFound();

        var result = await Mediator.Send(new ConciliarSifenParaguayComprobanteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Lista comprobantes Paraguay emitidos que aun no fueron aceptados por SIFEN/SET.
    /// Permite filtrar por sucursal, estado SIFEN, codigo de respuesta, reintento e identificadores persistidos.
    /// </summary>
    [HttpGet("paraguay/sifen/pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSifenPendientes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estadoSifen = null,
        [FromQuery] string? codigoRespuesta = null,
        [FromQuery] bool? puedeReintentar = null,
        [FromQuery] DateOnly? fechaDesde = null,
        [FromQuery] DateOnly? fechaHasta = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool soloConIdentificadores = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetComprobantesSifenPendientesQuery(
                page,
                pageSize,
                sucursalId,
                estadoSifen,
                codigoRespuesta,
                puedeReintentar,
                soloConIdentificadores,
                fechaDesde,
                fechaHasta,
                sortBy),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Exporta la bandeja de comprobantes Paraguay aun no aceptados por SIFEN/SET a CSV.
    /// Usa los mismos filtros operativos que la bandeja interactiva.
    /// </summary>
    [HttpGet("paraguay/sifen/pendientes/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportSifenPendientes(
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estadoSifen = null,
        [FromQuery] string? codigoRespuesta = null,
        [FromQuery] bool? puedeReintentar = null,
        [FromQuery] DateOnly? fechaDesde = null,
        [FromQuery] DateOnly? fechaHasta = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool soloConIdentificadores = false,
        CancellationToken ct = default)
    {
        ExportacionArchivoResultDto result = await Mediator.Send(
            new ExportarComprobantesSifenPendientesCsvQuery(
                sucursalId,
                estadoSifen,
                codigoRespuesta,
                puedeReintentar,
                soloConIdentificadores,
                fechaDesde,
                fechaHasta,
                sortBy),
            ct);

        return File(Encoding.Latin1.GetBytes(result.Contenido), "text/csv", result.NombreArchivo);
    }

    /// <summary>
    /// Retorna un resumen agregado de comprobantes Paraguay aun no aceptados por SIFEN/SET.
    /// Expone totales, reintentables, casos con identificadores, distribucion por estado y top de codigos y mensajes de respuesta.
    /// </summary>
    [HttpGet("paraguay/sifen/pendientes/resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSifenPendientesResumen(
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estadoSifen = null,
        [FromQuery] string? codigoRespuesta = null,
        [FromQuery] bool? puedeReintentar = null,
        [FromQuery] DateOnly? fechaDesde = null,
        [FromQuery] DateOnly? fechaHasta = null,
        [FromQuery] int topCodigos = 10,
        [FromQuery] int topMensajes = 10,
        [FromQuery] bool soloConIdentificadores = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetComprobantesSifenPendientesResumenQuery(
                sucursalId,
                estadoSifen,
                codigoRespuesta,
                puedeReintentar,
                soloConIdentificadores,
                fechaDesde,
                fechaHasta,
                topCodigos,
                topMensajes),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Concilia en lote comprobantes Paraguay emitidos con identificadores SIFEN persistidos y aun no aceptados.
    /// </summary>
    [HttpGet("paraguay/sifen/conciliar-pendientes/preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PreviewConciliarSifenParaguayPendientes(
        [FromQuery] int maxItems = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estadoSifen = null,
        [FromQuery] string? codigoRespuesta = null,
        [FromQuery] bool? puedeReintentar = null,
        [FromQuery] DateOnly? fechaDesde = null,
        [FromQuery] DateOnly? fechaHasta = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new PreviewConciliarSifenParaguayPendientesQuery(
                maxItems,
                sucursalId,
                estadoSifen,
                codigoRespuesta,
                puedeReintentar,
                fechaDesde,
                fechaHasta),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Concilia en lote comprobantes Paraguay emitidos con identificadores SIFEN persistidos y aun no aceptados.
    /// </summary>
    [HttpPost("paraguay/sifen/conciliar-pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConciliarSifenParaguayPendientes(
        [FromBody] ConciliarSifenParaguayPendientesRequest? request,
        CancellationToken ct)
    {
        var command = new ConciliarSifenParaguayPendientesCommand(
            request?.MaxItems ?? 20,
            request?.SucursalId,
            request?.EstadoSifen,
            request?.CodigoRespuesta,
            request?.PuedeReintentar,
            request?.FechaDesde,
            request?.FechaHasta);

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
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
    /// Crea un comprobante en estado borrador sin ejecutar el flujo completo de emisión.
    /// </summary>
    [HttpPost("borradores")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBorrador(
        [FromBody] CreateComprobanteCommand command,
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
        var result = await Mediator.Send(
            new AsignarCaeComprobanteCommand(id, request.Cae, request.FechaVto, request.QrData),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = $"No se encontró el comprobante ID {id}." });

        return Ok(new { mensaje = "CAE asignado correctamente." });
    }

    /// <summary>
    /// Solicita a AFIP un CAE real para un comprobante ya emitido y lo asigna en el sistema.
    /// </summary>
    [HttpPost("{id:long}/cae/afip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SolicitarCaeAfip(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new SolicitarCaeAfipComprobanteCommand(id), ct);
        return FromResult(result);
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

    // ── TipoEntrega ───────────────────────────────────────────────────────────
    // VB6: clsComprobantesTipoEntrega / COMPROBANTESTIPOENTREGA

    [HttpGet("tipos-entrega")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposEntrega(CancellationToken ct)
    {
        var list = await db.TiposEntrega
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.TipoComprobanteId, x.Prefijo })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost("tipos-entrega")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTipoEntrega(
        [FromBody] CreateTipoEntregaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateTipoEntregaCommand(req.Codigo, req.Descripcion, req.TipoComprobanteId, req.Prefijo),
            ct);

        if (!result.IsSuccess)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = "Ya existe un tipo de entrega con ese código." })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetTiposEntrega), null, new { Id = result.Value });
    }

    [HttpPut("tipos-entrega/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTipoEntrega(
        long id, [FromBody] UpdateTipoEntregaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateTipoEntregaCommand(id, req.Descripcion, req.TipoComprobanteId, req.Prefijo),
            ct);

        if (!result.IsSuccess) return NotFound();

        return Ok(new { Id = id, req.Descripcion });
    }

    // ── ComprobanteEntrega ────────────────────────────────────────────────────
    // VB6: clsComprobantesEntrega / COMPROBANTESENTREGA

    [HttpGet("{id:long}/entrega")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEntrega(long id, CancellationToken ct)
    {
        var entrega = await db.ComprobantesEntregas
            .FirstOrDefaultAsync(x => x.ComprobanteId == id, ct);
        if (entrega is null) return NotFound();
        return Ok(entrega);
    }

    [HttpPost("{id:long}/entrega")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEntrega(
        long id, [FromBody] CreateEntregaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateComprobanteEntregaCommand(
                id,
                req.Fecha,
                req.RazonSocial,
                req.Domicilio,
                req.LocalidadId,
                req.ProvinciaId,
                req.PaisId,
                req.CodigoPostal,
                req.Telefono1,
                req.Telefono2,
                req.Celular,
                req.Email,
                req.Observacion,
                req.TipoEntregaId,
                req.TransportistaId,
                req.ZonaId),
            ct);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("Comprobante no encontrado", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound();

            if (result.Error?.Contains("ya tiene datos de entrega", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = "El comprobante ya tiene datos de entrega. Use PUT para actualizar." });

            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetEntrega), new { id }, new { Id = result.Value });
    }

    [HttpPut("{id:long}/entrega")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEntrega(
        long id, [FromBody] CreateEntregaRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateComprobanteEntregaCommand(
                id,
                req.RazonSocial,
                req.Domicilio,
                req.LocalidadId,
                req.ProvinciaId,
                req.PaisId,
                req.CodigoPostal,
                req.Telefono1,
                req.Telefono2,
                req.Celular,
                req.Email,
                req.Observacion,
                req.TipoEntregaId,
                req.TransportistaId,
                req.ZonaId),
            ct);

        if (!result.IsSuccess) return NotFound();

        var entrega = await db.ComprobantesEntregas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ComprobanteId == id, ct);

        return Ok(new { entrega?.Id });
    }

    // ── ComprobanteDetalleCosto ───────────────────────────────────────────────
    // VB6: clsComprobantesDetalleCostos / COMPROBANTESDETALLESCOSTOS

    [HttpGet("{id:long}/detalle-costos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetalleCostos(long id, CancellationToken ct)
    {
        var list = await db.ComprobantesDetallesCostos
            .Where(x => db.ComprobantesItems
                .Where(ci => ci.ComprobanteId == id)
                .Select(ci => ci.Id)
                .Contains(x.ComprobanteItemId))
            .Select(x => new { x.Id, x.ComprobanteItemId, x.CentroCostoId, x.Procesado })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost("{id:long}/detalle-costos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDetalleCosto(
        long id, [FromBody] AddDetalleCostoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddComprobanteDetalleCostoCommand(id, req.ComprobanteItemId, req.CentroCostoId),
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = "El ítem no pertenece al comprobante indicado." });

        return CreatedAtAction(nameof(GetDetalleCostos), new { id }, new { Id = result.Value });
    }

    [HttpPatch("{id:long}/detalle-costos/{dcId:long}/procesar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcesarDetalleCosto(
        long id, long dcId, [FromQuery] bool procesar = true, CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new ProcesarComprobanteDetalleCostoCommand(id, dcId, procesar),
            ct);

        if (!result.IsSuccess) return NotFound();

        return Ok(new { Id = dcId, Procesado = result.Value });
    }

    // ── Formas de pago de comprobante ─────────────────────────────────────────

    /// <summary>
    /// Retorna las formas de pago asociadas a un comprobante.
    /// </summary>
    [HttpGet("{id:long}/formas-pago")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFormasPago(long id, CancellationToken ct)
    {
        var lista = await db.ComprobantesFormasPago
            .Where(fp => fp.ComprobanteId == id)
            .Select(fp => new
            {
                fp.Id,
                fp.FormaPagoId,
                fp.Fecha,
                fp.Importe,
                fp.Descripcion,
                fp.Observacion,
                fp.Valido,
                fp.MonedaId,
                fp.Cotizacion
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Agrega una forma de pago a un comprobante.
    /// </summary>
    [HttpPost("{id:long}/formas-pago")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddFormaPago(
        long id,
        [FromBody] AddFormaPagoRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AddComprobanteFormaPagoCommand(
                id,
                req.FormaPagoId,
                req.Fecha,
                req.Importe,
                req.Descripcion,
                req.Observacion,
                req.MonedaId,
                req.Cotizacion),
            ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Created(string.Empty, new { Id = result.Value });
    }

    /// <summary>
    /// Anula una forma de pago de un comprobante.
    /// </summary>
    [HttpPatch("{id:long}/formas-pago/{fpId:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnularFormaPago(long id, long fpId, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularComprobanteFormaPagoCommand(id, fpId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = $"Forma de pago {fpId} no encontrada en comprobante {id}." });

        return Ok();
    }

    /// <summary>Retorna el historial de auditoría de un comprobante.</summary>
    [HttpGet("{id:long}/auditoria")]
    public async Task<IActionResult> GetAuditoria(long id, CancellationToken ct)
        => Ok(await Mediator.Send(new GetAuditoriaComprobanteQuery(id), ct));
}

// ── Request bodies ────────────────────────────────────────────────────────────
public record AsignarCaeRequest(
    string Cae,
    DateOnly FechaVto,
    string? QrData);

public record ConciliarSifenParaguayPendientesRequest(
    int MaxItems = 20,
    long? SucursalId = null,
    string? EstadoSifen = null,
    string? CodigoRespuesta = null,
    bool? PuedeReintentar = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null);

public record ComprobanteReimpresionResponse(
    bool EsReimpresion,
    DateTimeOffset GeneradoEn,
    object Documento);

public record ReintentarSifenParaguayPendientesRequest(
    int MaxItems = 20,
    long? SucursalId = null,
    string? EstadoSifen = null,
    string? CodigoRespuesta = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null);

public record AnularComprobanteRequest(bool RevertirStock = true);

public record ConvertirPresupuestoRequest(
    long TipoComprobanteDestinoId,
    long? PuntoFacturacionId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion);

public record CreateTipoEntregaRequest(
    string Codigo,
    string Descripcion,
    long? TipoComprobanteId = null,
    string? Prefijo = null);

public record UpdateTipoEntregaRequest(
    string Descripcion,
    long? TipoComprobanteId = null,
    string? Prefijo = null);

public record CreateEntregaRequest(
    DateOnly Fecha,
    string? RazonSocial = null,
    string? Domicilio = null,
    long? LocalidadId = null,
    long? ProvinciaId = null,
    long? PaisId = null,
    string? CodigoPostal = null,
    string? Telefono1 = null,
    string? Telefono2 = null,
    string? Celular = null,
    string? Email = null,
    string? Observacion = null,
    long? TipoEntregaId = null,
    long? TransportistaId = null,
    long? ZonaId = null);

public record AddDetalleCostoRequest(long ComprobanteItemId, long CentroCostoId);
public record AddFormaPagoRequest(
    long FormaPagoId,
    DateOnly Fecha,
    decimal Importe,
    string? Descripcion = null,
    string? Observacion = null,
    long? MonedaId = null,
    decimal Cotizacion = 1);

