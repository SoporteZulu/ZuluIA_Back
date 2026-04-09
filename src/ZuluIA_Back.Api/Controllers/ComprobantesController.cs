using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Ventas.Commands;
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
    /// Retorna una vista de remitos de compras compatible con la pantalla operativa del frontend.
    /// </summary>
    [HttpGet("remitos/compras")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRemitosCompras(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? proveedorId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? tipo = null,
        [FromQuery] bool? esValorizado = null,
        CancellationToken ct = default)
    {
        var estadoNormalizado = NormalizarEstadoRemitoFiltro(estado);
        var tipoNormalizado = NormalizarTipoRemitoFiltro(tipo);
        var tiposRemitoCompra = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => x.Activo && x.EsCompra)
            .Where(x => x.Descripcion.ToUpper().Contains("REMIT") || x.Codigo.ToUpper().Contains("REMIT"))
            .Select(x => new { x.Id })
            .ToListAsync(ct);

        var tipoIds = tiposRemitoCompra.Select(x => x.Id).ToList();
        if (tipoIds.Count == 0)
            return Ok(Array.Empty<CompraRemitoResumenDto>());

        var query = db.Comprobantes
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.TipoComprobanteId));

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (proveedorId.HasValue)
            query = query.Where(x => x.TerceroId == proveedorId.Value);

        if (esValorizado.HasValue)
            query = query.Where(x => x.EsValorizado == esValorizado.Value);

        var remitos = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        var remitoIds = remitos.Select(x => x.Id).ToList();
        var proveedorIds = remitos.Select(x => x.TerceroId).Distinct().ToList();
        var transporteIds = remitos.Where(x => x.TransporteId.HasValue).Select(x => x.TransporteId!.Value).Distinct().ToList();
        var usuarioIds = remitos
            .Where(x => x.CreatedBy.HasValue || x.UpdatedBy.HasValue)
            .SelectMany(x => new[] { x.CreatedBy, x.UpdatedBy })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();
        var origenIds = remitos.Where(x => x.ComprobanteOrigenId.HasValue).Select(x => x.ComprobanteOrigenId!.Value).Distinct().ToList();

        var proveedores = await db.Terceros
            .AsNoTracking()
            .Where(x => proveedorIds.Contains(x.Id) || transporteIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, x => x.RazonSocial, ct);

        var usuarios = await db.Usuarios
            .AsNoTracking()
            .Where(x => usuarioIds.Contains(x.Id))
            .Select(x => new { x.Id, Nombre = x.NombreCompleto ?? x.UserName })
            .ToDictionaryAsync(x => x.Id, x => x.Nombre, ct);

        var ordenes = await db.OrdenesCompraMeta
            .AsNoTracking()
            .Where(x => origenIds.Contains(x.ComprobanteId))
            .ToDictionaryAsync(x => x.ComprobanteId, ct);

        var remitoItems = await db.ComprobantesItems
            .AsNoTracking()
            .Where(x => remitoIds.Contains(x.ComprobanteId))
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var origenItems = await db.ComprobantesItems
            .AsNoTracking()
            .Where(x => origenIds.Contains(x.ComprobanteId))
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var itemIds = remitoItems.Select(x => x.ItemId)
            .Concat(origenItems.Select(x => x.ItemId))
            .Distinct()
            .ToList();
        var unidadIds = remitoItems.Where(x => x.UnidadMedidaId.HasValue).Select(x => x.UnidadMedidaId!.Value)
            .Concat(origenItems.Where(x => x.UnidadMedidaId.HasValue).Select(x => x.UnidadMedidaId!.Value))
            .Distinct()
            .ToList();
        var depositoIds = remitos.Where(x => x.DepositoOrigenId.HasValue).Select(x => x.DepositoOrigenId!.Value)
            .Concat(remitoItems.Where(x => x.DepositoId.HasValue).Select(x => x.DepositoId!.Value))
            .Distinct()
            .ToList();

        var itemsLookup = await db.Items
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.UnidadMedidaId })
            .ToDictionaryAsync(x => x.Id, ct);

        var unidades = await db.UnidadesMedida
            .AsNoTracking()
            .Where(x => unidadIds.Contains(x.Id) || itemsLookup.Values.Select(v => v.UnidadMedidaId).Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion, x.Disminutivo })
            .ToDictionaryAsync(x => x.Id, ct);

        var depositos = await db.Depositos
            .AsNoTracking()
            .Where(x => depositoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct);

        var rows = remitos
            .Select(remito =>
            {
                var itemsActuales = remitoItems.Where(x => x.ComprobanteId == remito.Id).ToList();
                var itemsOrigen = remito.ComprobanteOrigenId.HasValue
                    ? origenItems.Where(x => x.ComprobanteId == remito.ComprobanteOrigenId.Value).ToList()
                    : [];

                var detallesItems = itemsActuales.Select(item =>
                {
                    var origen = itemsOrigen.FirstOrDefault(x => x.ItemId == item.ItemId);
                    var itemInfo = itemsLookup.GetValueOrDefault(item.ItemId);
                    var unidadId = item.UnidadMedidaId ?? itemInfo?.UnidadMedidaId;
                    var unidad = unidadId.HasValue ? unidades.GetValueOrDefault(unidadId.Value) : null;
                    var cantidadEsperada = origen?.Cantidad ?? item.Cantidad;
                    var recibido = item.Cantidad;
                    var diferencia = recibido - cantidadEsperada;

                    return new CompraRemitoItemDto(
                        $"rem-{remito.Id}-{item.Id}",
                        itemInfo?.Codigo ?? item.ItemId.ToString(),
                        item.Descripcion,
                        cantidadEsperada,
                        unidad?.Disminutivo ?? unidad?.Descripcion ?? "unid",
                        recibido,
                        diferencia);
                }).ToList();

                var discrepancias = detallesItems.Where(x => x.Diferencia != 0).ToList();
                var deposito = remito.DepositoOrigenId.HasValue
                    ? depositos.GetValueOrDefault(remito.DepositoOrigenId.Value)
                    : itemsActuales.FirstOrDefault(x => x.DepositoId.HasValue) is { DepositoId: { } depId }
                        ? depositos.GetValueOrDefault(depId)
                        : null;

                var estadoRemito = remito.Estado == EstadoComprobante.Anulado
                    ? "ANULADO"
                    : remito.ComprobanteOrigenId.HasValue && ordenes.TryGetValue(remito.ComprobanteOrigenId.Value, out var orden) && orden.EstadoOc == EstadoOrdenCompra.Recibida
                        ? "RECIBIDO"
                        : "PENDIENTE";

                var diferenciasClave = new List<string>();
                if (discrepancias.Count > 0)
                    diferenciasClave.Add($"Hay {discrepancias.Count} renglón(es) con diferencia respecto al comprobante origen.");
                if (!remito.EsValorizado)
                    diferenciasClave.Add("Remito no valorizado; el total económico permanece en 0.");
                if (string.IsNullOrWhiteSpace(remito.Observacion) && discrepancias.Count == 0)
                    diferenciasClave.Add("Sin observaciones operativas registradas.");
                else if (!string.IsNullOrWhiteSpace(remito.Observacion))
                    diferenciasClave.Add(remito.Observacion!);

                var responsable = !string.IsNullOrWhiteSpace(remito.NombreQuienRecibe)
                    ? remito.NombreQuienRecibe!
                    : remito.UpdatedBy.HasValue
                        ? usuarios.GetValueOrDefault(remito.UpdatedBy.Value, "Sin responsable visible")
                        : remito.CreatedBy.HasValue
                            ? usuarios.GetValueOrDefault(remito.CreatedBy.Value, "Sin responsable visible")
                            : "Sin responsable visible";

                return new CompraRemitoResumenDto(
                    remito.Id,
                    remito.EsValorizado ? "Valorizado" : "No valorizado",
                    proveedores.GetValueOrDefault(remito.TerceroId, $"Proveedor #{remito.TerceroId}"),
                    remito.Numero.Formateado,
                    remito.Fecha,
                    deposito ?? "Sin depósito visible",
                    estadoRemito,
                    remito.ComprobanteOrigenId.HasValue && ordenes.TryGetValue(remito.ComprobanteOrigenId.Value, out var ordenCompra)
                        ? $"OC-{ordenCompra.Id}"
                        : null,
                    $"REC-{remito.Id:D6}",
                    remito.TransporteId.HasValue
                        ? proveedores.GetValueOrDefault(remito.TransporteId.Value, "Sin transportista visible")
                        : "Sin transportista visible",
                    responsable,
                    remito.Observacion ?? "Sin observaciones registradas.",
                    diferenciasClave,
                    remito.Total,
                    detallesItems);
            })
            .Where(x => string.IsNullOrWhiteSpace(estadoNormalizado) || SonFiltrosEquivalentes(x.Estado, estadoNormalizado))
            .Where(x => string.IsNullOrWhiteSpace(tipoNormalizado) || SonFiltrosEquivalentes(x.Tipo, tipoNormalizado))
            .ToList();

        return Ok(rows);
    }

    /// <summary>
    /// Retorna una vista agregada de notas de crédito de compra basada en comprobantes reales.
    /// </summary>
    [HttpGet("notas-credito/compras")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotasCreditoCompras(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? proveedorId = null,
        [FromQuery] string? estado = null,
        CancellationToken ct = default)
    {
        var estadoNormalizado = NormalizarEstadoNotaCreditoFiltro(estado);
        var rows = await BuildAjustesComprasAsync(sucursalId, proveedorId, ct);

        var notas = rows
            .Where(x => x.Tipo == "Crédito")
            .Select(x => new CompraNotaCreditoResumenDto(
                x.Id,
                x.Proveedor,
                x.ComprobanteReferencia,
                x.Motivo,
                x.Estado switch
                {
                    "APLICADO" => "APLICADA",
                    "EMITIDO" => "EMITIDA",
                    _ => x.Estado
                },
                x.Fecha,
                x.Total,
                x.OrdenCompraReferencia,
                x.DevolucionReferencia,
                x.Responsable,
                x.ImpactoCuentaCorriente,
                x.Observacion,
                x.DetallesClave,
                x.Items.Select(i => new CompraNotaCreditoItemDto(i.Id, i.Concepto, i.Importe)).ToList()))
            .Where(x => string.IsNullOrWhiteSpace(estadoNormalizado) || SonFiltrosEquivalentes(x.Estado, estadoNormalizado))
            .ToList();

        return Ok(notas);
    }

    /// <summary>
    /// Retorna una vista agregada de ajustes de compra basada en comprobantes reales.
    /// </summary>
    [HttpGet("ajustes/compras")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAjustesCompras(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? proveedorId = null,
        [FromQuery] string? tipo = null,
        CancellationToken ct = default)
    {
        var tipoNormalizado = NormalizarTextoFiltro(tipo);
        var rows = await BuildAjustesComprasAsync(sucursalId, proveedorId, ct);
        var ajustes = rows
            .Where(x => string.IsNullOrWhiteSpace(tipoNormalizado) || SonFiltrosEquivalentes(x.Tipo, tipoNormalizado))
            .ToList();

        return Ok(ajustes);
    }

    /// <summary>
    /// Retorna una vista agregada de devoluciones de compra basada en comprobantes reales.
    /// </summary>
    [HttpGet("devoluciones/compras")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevolucionesCompras(
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? proveedorId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? tipo = null,
        CancellationToken ct = default)
    {
        var estadoNormalizado = NormalizarEstadoDevolucionFiltro(estado);
        var tipoNormalizado = NormalizarTextoFiltro(tipo);
        var rows = await BuildDevolucionesComprasAsync(sucursalId, proveedorId, ct);
        var filtered = rows
            .Where(x => string.IsNullOrWhiteSpace(estadoNormalizado) || SonFiltrosEquivalentes(x.Estado, estadoNormalizado))
            .Where(x => string.IsNullOrWhiteSpace(tipoNormalizado) || SonFiltrosEquivalentes(x.Tipo, tipoNormalizado))
            .ToList();

        return Ok(filtered);
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
    /// Reemplaza los atributos de cabecera de un remito.
    /// Se usa semántica completa de replace para mantener paridad con formularios legacy.
    /// </summary>
    [HttpPut("{id:long}/remito/atributos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceRemitoAtributos(
        long id,
        [FromBody] ReplaceRemitoAtributosRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ReplaceRemitoAtributosCommand(
                id,
                request.Atributos.Select(x => new RemitoAtributoInput(x.Clave, x.Valor, x.TipoDato)).ToList().AsReadOnly()),
            ct);

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
    /// Retorna los estados logísticos disponibles para remitos.
    /// </summary>
    [HttpGet("remitos/estados-logisticos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEstadosLogisticosRemito()
    {
        var estados = Enum.GetValues<EstadoLogisticoRemito>()
            .Select(e => new
            {
                valor = e.ToString().ToUpperInvariant(),
                descripcion = e.ToString()
            });

        return Ok(estados);
    }

    /// <summary>
    /// Retorna remitos paginados con filtros operativos compatibles con zuluApp.
    /// </summary>
    [HttpGet("remitos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRemitos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] DateOnly? fechaDesde = null,
        [FromQuery] DateOnly? fechaHasta = null,
        [FromQuery] short? prefijo = null,
        [FromQuery] long? numero = null,
        [FromQuery] string? terceroLegajo = null,
        [FromQuery] string? terceroDenominacionSocial = null,
        [FromQuery] string? cotNumero = null,
        [FromQuery] DateOnly? cotFechaDesde = null,
        [FromQuery] DateOnly? cotFechaHasta = null,
        [FromQuery] long? depositoId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? estadoLogistico = null,
        [FromQuery] bool? esValorizado = null,
        CancellationToken ct = default)
    {
        EstadoComprobante? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoComprobante>(estado, true, out var estadoParseado))
            estadoEnum = estadoParseado;

        EstadoLogisticoRemito? estadoLogisticoEnum = null;
        if (!string.IsNullOrWhiteSpace(estadoLogistico) && Enum.TryParse<EstadoLogisticoRemito>(estadoLogistico, true, out var estadoLogisticoParseado))
            estadoLogisticoEnum = estadoLogisticoParseado;

        var result = await Mediator.Send(
            new ZuluIA_Back.Application.Features.Ventas.Queries.GetRemitosPagedQuery(
                page,
                pageSize,
                sucursalId,
                fechaDesde,
                fechaHasta,
                prefijo,
                numero,
                terceroLegajo,
                terceroDenominacionSocial,
                cotNumero,
                cotFechaDesde,
                cotFechaHasta,
                depositoId,
                estadoEnum,
                estadoLogisticoEnum,
                esValorizado),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Retorna las notas de débito de venta con filtros operativos.
    /// </summary>
    [HttpGet("notas-debito")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotasDebito(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] long? motivoDebitoId = null,
        [FromQuery] long? comprobanteOrigenId = null,
        CancellationToken ct = default)
    {
        EstadoComprobante? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado)
            && Enum.TryParse<EstadoComprobante>(estado, true, out var parsed))
        {
            estadoEnum = parsed;
        }

        var result = await Mediator.Send(
            new ZuluIA_Back.Application.Features.Ventas.Queries.GetNotasDebitoPagedQuery(
                page,
                pageSize,
                sucursalId,
                terceroId,
                null,
                estadoEnum,
                desde,
                hasta,
                comprobanteOrigenId,
                motivoDebitoId),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Retorna el catálogo de motivos de débito disponible para ventas.
    /// </summary>
    [HttpGet("notas-debito/motivos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMotivosDebito(
        [FromQuery] bool soloActivos = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new ZuluIA_Back.Application.Features.Ventas.Queries.GetMotivosDebitoQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crea una nota de débito de venta en estado borrador.
    /// </summary>
    [HttpPost("notas-debito")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearNotaDebito(
        [FromBody] RegistrarNotaDebitoVentaCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetComprobanteById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Emite un nuevo comprobante.
    /// Valida período IVA, calcula totales y afecta stock si corresponde.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Emitir(
        [FromBody] EmitirComprobanteCompatRequest request,
        CancellationToken ct)
    {
        var monedaId = request.MonedaId;
        if (!monedaId.HasValue)
        {
            monedaId = await db.Monedas
                .AsNoTracking()
                .Where(x => x.Activa)
                .OrderBy(x => x.Id)
                .Select(x => (long?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (!monedaId.HasValue)
                return BadRequest(new { error = "No existe una moneda activa para emitir el comprobante." });
        }

        var command = new EmitirComprobanteCommand(
            request.Id,
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Fecha,
            request.FechaVencimiento ?? request.FechaVto,
            request.TerceroId,
            monedaId.Value,
            request.Cotizacion ?? 1m,
            request.Percepciones ?? 0m,
            request.Observacion,
            request.Items.Select((item, index) => new ComprobanteItemInput(
                item.ItemId,
                item.Descripcion,
                item.Cantidad,
                item.CantidadBonificada ?? 0,
                item.PrecioUnitario,
                item.DescuentoPct ?? item.Descuento ?? 0m,
                item.AlicuotaIvaId,
                item.DepositoId,
                item.Orden ?? (short)index,
                item.Lote,
                item.Serie,
                item.FechaVencimiento,
                item.UnidadMedidaId,
                item.ObservacionRenglon,
                item.PrecioListaOriginal,
                item.ComisionVendedorRenglon,
                item.ComprobanteItemOrigenId,
                item.CantidadDocumentoOrigen,
                item.PrecioDocumentoOrigen))
            .ToList(),
            request.AfectaStock ?? true,
            request.Cae,
            request.FechaVtoCae);

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

    [HttpPost("{id:long}/afip/cae")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SolicitarCaeAfip(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AutorizarComprobanteAfipWsfeCommand(id, false), ct));

    [HttpPost("{id:long}/afip/caea")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SolicitarCaeaAfip(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AutorizarComprobanteAfipWsfeCommand(id, true), ct));

    [HttpGet("{id:long}/afip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsultarAfip(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new ConsultarComprobanteAfipWsfeCommand(id), ct));

    [HttpPost("{id:long}/afip/refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshAfip(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new RefreshEstadoAfipWsfeCommand(id), ct));

    [HttpGet("{id:long}/afip/auditoria")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditoriaAfip(long id, CancellationToken ct)
    {
        var items = await db.AfipWsfeAudits.AsNoTracking()
            .Where(x => x.ComprobanteId == id)
            .OrderByDescending(x => x.FechaOperacion)
            .ThenByDescending(x => x.Id)
            .Select(x => new AfipWsfeAuditDto
            {
                Id = x.Id,
                ComprobanteId = x.ComprobanteId,
                SucursalId = x.SucursalId,
                PuntoFacturacionId = x.PuntoFacturacionId,
                Operacion = x.Operacion.ToString().ToUpperInvariant(),
                Exitoso = x.Exitoso,
                RequestPayload = x.RequestPayload,
                ResponsePayload = x.ResponsePayload,
                MensajeError = x.MensajeError,
                Cae = x.Cae,
                Caea = x.Caea,
                FechaOperacion = x.FechaOperacion,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy
            })
            .ToListAsync(ct);

        return Ok(items);
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

    private async Task<List<CompraAjusteResumenDto>> BuildAjustesComprasAsync(
        long? sucursalId,
        long? proveedorId,
        CancellationToken ct)
    {
        var query = db.Comprobantes
            .AsNoTrackingSafe();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (proveedorId.HasValue)
            query = query.Where(x => x.TerceroId == proveedorId.Value);

        var comprobantes = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        var tipoIds = comprobantes.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var proveedorIds = comprobantes.Select(x => x.TerceroId).Distinct().ToList();
        var usuarioIds = comprobantes.Where(x => x.CreatedBy.HasValue).Select(x => x.CreatedBy!.Value).Distinct().ToList();
        var origenIds = comprobantes.Where(x => x.ComprobanteOrigenId.HasValue).Select(x => x.ComprobanteOrigenId!.Value).Distinct().ToList();
        var comprobanteIds = comprobantes.Select(x => x.Id).ToList();

        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion, x.Codigo, x.EsCompra })
            .ToDictionaryAsync(x => x.Id, ct);

        var compras = comprobantes
            .Where(x =>
                tipos.TryGetValue(x.TipoComprobanteId, out var tipo)
                && tipo.EsCompra
                && (EsDocumentoEconomicoCompra(tipo.Codigo, tipo.Descripcion)
                    || x.MotivoDevolucion.HasValue
                    || x.AcreditaCuentaCorriente
                    || x.ReingresaStock))
            .ToList();

        var compraIds = compras.Select(x => x.Id).ToList();
        var compraOrigenIds = compras.Where(x => x.ComprobanteOrigenId.HasValue).Select(x => x.ComprobanteOrigenId!.Value).Distinct().ToList();

        var proveedores = await db.Terceros
            .AsNoTracking()
            .Where(x => proveedorIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, x => x.RazonSocial, ct);

        var usuarios = await db.Usuarios
            .AsNoTracking()
            .Where(x => usuarioIds.Contains(x.Id))
            .Select(x => new { x.Id, Nombre = x.NombreCompleto ?? x.UserName })
            .ToDictionaryAsync(x => x.Id, x => x.Nombre, ct);

        var origenes = await db.Comprobantes
            .AsNoTracking()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, Numero = x.Numero.Formateado })
            .ToDictionaryAsync(x => x.Id, x => x.Numero, ct);

        var ordenes = await db.OrdenesCompraMeta
            .AsNoTracking()
            .Where(x => compraOrigenIds.Contains(x.ComprobanteId))
            .ToDictionaryAsync(x => x.ComprobanteId, ct);

        var imputacionesOrigen = await db.Imputaciones
            .AsNoTracking()
            .Where(x => compraIds.Contains(x.ComprobanteOrigenId) && !x.Anulada)
            .ToListAsync(ct);

        var items = await db.ComprobantesItems
            .AsNoTracking()
            .Where(x => compraIds.Contains(x.ComprobanteId))
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        return compras.Select(comp =>
        {
            var tipo = tipos.GetValueOrDefault(comp.TipoComprobanteId);
            var esCredito = comp.AcreditaCuentaCorriente || comp.Total < 0 || comp.MotivoDevolucion.HasValue;
            var tieneAplicaciones = imputacionesOrigen.Any(x => x.ComprobanteOrigenId == comp.Id);
            var estado = comp.Estado == EstadoComprobante.Borrador
                ? "BORRADOR"
                : (tieneAplicaciones || comp.Saldo <= 0 ? "APLICADO" : "EMITIDO");

            var motivo = comp.ObservacionDevolucion
                ?? comp.MotivoDevolucion?.ToString()
                ?? comp.Observacion
                ?? tipo?.Descripcion
                ?? "Ajuste de compra";

            var impacto = esCredito
                ? (tieneAplicaciones || comp.Saldo <= 0
                    ? "Aplicada sobre saldo del comprobante y conciliada con cuenta corriente."
                    : "Disminuye saldo abierto del proveedor y queda pendiente de aplicación.")
                : "Regulariza impacto económico/comercial sobre el comprobante base.";

            var detalles = new List<string>();
            if (comp.MotivoDevolucion.HasValue)
                detalles.Add($"Motivo de devolución: {comp.MotivoDevolucion}");
            if (comp.ReingresaStock)
                detalles.Add("Genera reintegro de stock asociado.");
            if (comp.AcreditaCuentaCorriente)
                detalles.Add("Acredita cuenta corriente del proveedor.");
            if (tieneAplicaciones)
                detalles.Add($"Aplicaciones activas: {imputacionesOrigen.Count(x => x.ComprobanteOrigenId == comp.Id)}");
            if (!string.IsNullOrWhiteSpace(comp.Observacion))
                detalles.Add(comp.Observacion!);
            if (detalles.Count == 0)
                detalles.Add("Sin observaciones adicionales registradas.");

            var lineas = items.Where(x => x.ComprobanteId == comp.Id)
                .Select((item, index) => new CompraAjusteItemDto(
                    $"adj-{comp.Id}-{index}",
                    item.Descripcion,
                    tipo?.Descripcion ?? (esCredito ? "Bonificación" : "Ajuste comercial"),
                    item.TotalLinea))
                .ToList();

            if (lineas.Count == 0)
            {
                lineas.Add(new CompraAjusteItemDto(
                    $"adj-{comp.Id}-0",
                    motivo,
                    tipo?.Descripcion ?? (esCredito ? "Bonificación" : "Ajuste comercial"),
                    comp.Total));
            }

            return new CompraAjusteResumenDto(
                comp.Id,
                esCredito ? "Crédito" : "Débito",
                proveedores.GetValueOrDefault(comp.TerceroId, $"Proveedor #{comp.TerceroId}"),
                comp.ComprobanteOrigenId.HasValue ? origenes.GetValueOrDefault(comp.ComprobanteOrigenId.Value) ?? comp.Numero.Formateado : comp.Numero.Formateado,
                motivo,
                estado,
                comp.Fecha,
                comp.Total,
                comp.ComprobanteOrigenId.HasValue && ordenes.TryGetValue(comp.ComprobanteOrigenId.Value, out var orden)
                    ? $"OC-{orden.Id}"
                    : null,
                $"DEV-{comp.Id}",
                comp.CreatedBy.HasValue ? usuarios.GetValueOrDefault(comp.CreatedBy.Value, "Sin responsable visible") : "Sin responsable visible",
                impacto,
                comp.Observacion ?? comp.ObservacionDevolucion ?? "Sin observaciones registradas.",
                esCredito ? "Regularización por nota de crédito o devolución de compra" : "Regularización comercial/económica posterior a la compra",
                esCredito,
                detalles,
                lineas);
        }).ToList();
    }

    private async Task<List<CompraDevolucionResumenDto>> BuildDevolucionesComprasAsync(
        long? sucursalId,
        long? proveedorId,
        CancellationToken ct)
    {
        var query = db.Comprobantes
            .AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (proveedorId.HasValue)
            query = query.Where(x => x.TerceroId == proveedorId.Value);

        var comprobantes = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        var tipoIds = comprobantes.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var proveedorIds = comprobantes.Select(x => x.TerceroId).Distinct().ToList();
        var origenIds = comprobantes.Where(x => x.ComprobanteOrigenId.HasValue).Select(x => x.ComprobanteOrigenId!.Value).Distinct().ToList();
        var usuarioIds = comprobantes.Where(x => x.CreatedBy.HasValue).Select(x => x.CreatedBy!.Value).Distinct().ToList();

        var tipos = await db.TiposComprobante
            .AsNoTrackingSafe()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.EsCompra })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var devoluciones = comprobantes
            .Where(x => tipos.TryGetValue(x.TipoComprobanteId, out var tipo)
                && tipo.EsCompra
                && (x.MotivoDevolucion.HasValue
                    || x.TipoDevolucion.HasValue
                    || x.ReingresaStock
                    || !string.IsNullOrWhiteSpace(x.ObservacionDevolucion)))
            .ToList();

        var devolucionIds = devoluciones.Select(x => x.Id).ToList();
        var origenes = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, Numero = x.Numero.Formateado })
            .ToDictionarySafeAsync(x => x.Id, x => x.Numero, ct);

        var proveedores = await db.Terceros
            .AsNoTrackingSafe()
            .Where(x => proveedorIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionarySafeAsync(x => x.Id, x => x.RazonSocial, ct);

        var usuarios = await db.Usuarios
            .AsNoTrackingSafe()
            .Where(x => usuarioIds.Contains(x.Id))
            .Select(x => new { x.Id, Nombre = x.NombreCompleto ?? x.UserName })
            .ToDictionarySafeAsync(x => x.Id, x => x.Nombre, ct);

        var ordenes = await db.OrdenesCompraMeta
            .AsNoTrackingSafe()
            .Where(x => origenIds.Contains(x.ComprobanteId))
            .ToDictionarySafeAsync(x => x.ComprobanteId, ct);

        var tiposRemitoCompraIds = await db.TiposComprobante
            .AsNoTrackingSafe()
            .Where(x => x.Activo && x.EsCompra)
            .Where(x => x.Codigo.ToUpper().Contains("REMIT") || x.Descripcion.ToUpper().Contains("REMIT"))
            .Select(x => x.Id)
            .ToListAsync(ct);

        var remitos = await db.Comprobantes
            .AsNoTrackingSafe()
            .Where(x => x.ComprobanteOrigenId.HasValue && origenIds.Contains(x.ComprobanteOrigenId.Value) && tiposRemitoCompraIds.Contains(x.TipoComprobanteId))
            .Select(x => new { x.ComprobanteOrigenId, Numero = x.Numero.Formateado })
            .ToListAsync(ct);

        var items = await db.ComprobantesItems
            .AsNoTrackingSafe()
            .Where(x => devolucionIds.Contains(x.ComprobanteId))
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var depositoIds = items.Where(x => x.DepositoId.HasValue)
            .Select(x => x.DepositoId!.Value)
            .Distinct()
            .ToList();

        var depositos = await db.Depositos
            .AsNoTrackingSafe()
            .Where(x => depositoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionarySafeAsync(x => x.Id, x => x.Descripcion, ct);

        var itemIds = items.Select(x => x.ItemId).Distinct().ToList();
        var itemInfo = await db.Items
            .AsNoTrackingSafe()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo })
            .ToDictionarySafeAsync(x => x.Id, x => x.Codigo, ct);

        return devoluciones.Select(comp =>
        {
            var estado = comp.Estado == EstadoComprobante.Anulado
                ? "ANULADA"
                : comp.Estado == EstadoComprobante.Borrador
                    ? "ABIERTA"
                    : "PROCESADA";

            var tipo = comp.ReingresaStock ? "Stock" : "Económica";
            var impactoStock = comp.ReingresaStock
                ? "Se descargó stock y quedó trazado como devolución de compra."
                : "Sin impacto físico, solo regularización económica del circuito.";
            var motivo = comp.ObservacionDevolucion
                ?? comp.MotivoDevolucion?.ToString()
                ?? comp.Observacion
                ?? "Devolución de compra";
            var resolucion = comp.Estado == EstadoComprobante.Borrador
                ? "Pendiente de confirmación y cierre con proveedor."
                : (comp.AcreditaCuentaCorriente
                    ? "La devolución ya impactó en cuenta corriente del proveedor."
                    : "La devolución quedó procesada sin impacto económico adicional.");

            var detalleItems = items.Where(x => x.ComprobanteId == comp.Id)
                .Select((item, index) => new CompraDevolucionItemDto(
                    $"dev-{comp.Id}-{index}",
                    itemInfo.GetValueOrDefault(item.ItemId, item.ItemId.ToString()),
                    item.Descripcion,
                    item.Cantidad,
                    item.ObservacionRenglon ?? comp.MotivoDevolucion?.ToString() ?? motivo))
                .ToList();

            var diferencias = new List<string>();
            if (comp.MotivoDevolucion.HasValue)
                diferencias.Add($"Motivo registrado: {comp.MotivoDevolucion}");
            if (comp.AcreditaCuentaCorriente)
                diferencias.Add("La devolución requiere o ya generó nota de crédito / impacto económico.");
            if (!string.IsNullOrWhiteSpace(comp.Observacion))
                diferencias.Add(comp.Observacion!);
            if (diferencias.Count == 0)
                diferencias.Add("Sin diferencias adicionales registradas.");

            var remito = comp.ComprobanteOrigenId.HasValue
                ? remitos.FirstOrDefault(x => x.ComprobanteOrigenId == comp.ComprobanteOrigenId.Value)?.Numero
                : null;

            var deposito = items.Where(x => x.ComprobanteId == comp.Id && x.DepositoId.HasValue)
                .Select(x => depositos.GetValueOrDefault(x.DepositoId!.Value))
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                ?? "Sin depósito visible";

            return new CompraDevolucionResumenDto(
                comp.Id,
                tipo,
                proveedores.GetValueOrDefault(comp.TerceroId, $"Proveedor #{comp.TerceroId}"),
                comp.ComprobanteOrigenId.HasValue ? origenes.GetValueOrDefault(comp.ComprobanteOrigenId.Value) ?? comp.Numero.Formateado : comp.Numero.Formateado,
                motivo,
                estado,
                comp.Fecha,
                deposito,
                comp.ComprobanteOrigenId.HasValue && ordenes.TryGetValue(comp.ComprobanteOrigenId.Value, out var orden) ? $"OC-{orden.Id}" : null,
                remito,
                comp.ComprobanteOrigenId.HasValue && ordenes.TryGetValue(comp.ComprobanteOrigenId.Value, out var ordenRecepcion) ? $"REC-{ordenRecepcion.Id:D6}" : null,
                comp.CreatedBy.HasValue ? usuarios.GetValueOrDefault(comp.CreatedBy.Value, "Sin responsable visible") : "Sin responsable visible",
                resolucion,
                impactoStock,
                comp.AcreditaCuentaCorriente,
                diferencias,
                comp.Total,
                detalleItems);
        }).ToList();
    }

    private static bool EsDocumentoEconomicoCompra(string? codigo, string? descripcion)
    {
        var codigoUpper = codigo?.ToUpperInvariant() ?? string.Empty;
        var descripcionUpper = descripcion?.ToUpperInvariant() ?? string.Empty;
        return codigoUpper.Contains("NOTA")
            || codigoUpper.Contains("CRED")
            || codigoUpper.Contains("DEB")
            || codigoUpper.Contains("AJUST")
            || codigoUpper.Contains("DEVOL")
            || descripcionUpper.Contains("NOTA")
            || descripcionUpper.Contains("CRÉDIT")
            || descripcionUpper.Contains("CREDIT")
            || descripcionUpper.Contains("DÉBIT")
            || descripcionUpper.Contains("DEBIT")
            || descripcionUpper.Contains("AJUST")
            || descripcionUpper.Contains("DEVOL");
    }

    private static string? NormalizarEstadoDevolucionFiltro(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            return null;

        return estado.Trim().ToUpperInvariant() switch
        {
            "PENDIENTE" => "ABIERTA",
            _ => estado.Trim().ToUpperInvariant()
        };
    }

    private static string? NormalizarEstadoNotaCreditoFiltro(string? estado)
    {
        var normalizado = NormalizarTextoFiltro(estado);
        if (string.IsNullOrWhiteSpace(normalizado))
            return null;

        return normalizado switch
        {
            "APLICADO" => "APLICADA",
            "EMITIDO" => "EMITIDA",
            _ => normalizado
        };
    }

    private static bool SonFiltrosEquivalentes(string valor, string filtro)
        => string.Equals(NormalizarTextoFiltro(valor), NormalizarTextoFiltro(filtro), StringComparison.Ordinal);

    private static string? NormalizarEstadoRemitoFiltro(string? estado)
    {
        var normalizado = NormalizarTextoFiltro(estado);
        if (string.IsNullOrWhiteSpace(normalizado))
            return null;

        return normalizado switch
        {
            "RECIBIDA" => "RECIBIDO",
            "ANULADA" => "ANULADO",
            _ => normalizado
        };
    }

    private static string? NormalizarTipoRemitoFiltro(string? tipo)
    {
        var normalizado = NormalizarTextoFiltro(tipo);
        if (string.IsNullOrWhiteSpace(normalizado))
            return null;

        return normalizado switch
        {
            "VALORIZADA" => "VALORIZADO",
            "NO VALORIZADA" => "NO VALORIZADO",
            "NO_VALORIZADO" => "NO VALORIZADO",
            "NO_VALORIZADA" => "NO VALORIZADO",
            _ => normalizado.Replace('_', ' ')
        };
    }

    private static string? NormalizarTextoFiltro(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var normalized = valor.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToUpperInvariant(ch));
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
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

public record ReplaceRemitoAtributosRequest(IReadOnlyList<ReplaceRemitoAtributoRequestItem> Atributos);

public record ReplaceRemitoAtributoRequestItem(
    string Clave,
    string? Valor,
    string? TipoDato);

public record EmitirComprobanteCompatRequest(
    long? Id,
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    DateOnly? FechaVto,
    long TerceroId,
    long? MonedaId,
    decimal? Cotizacion,
    decimal? Percepciones,
    string? Observacion,
    IReadOnlyList<EmitirComprobanteCompatItemRequest> Items,
    bool? AfectaStock = null,
    string? Cae = null,
    DateOnly? FechaVtoCae = null);

public record EmitirComprobanteCompatItemRequest(
    long ItemId,
    string? Descripcion,
    decimal Cantidad,
    long? CantidadBonificada,
    long PrecioUnitario,
    decimal? DescuentoPct,
    decimal? Descuento,
    long AlicuotaIvaId,
    long? DepositoId,
    short? Orden,
    string? Lote = null,
    string? Serie = null,
    DateOnly? FechaVencimiento = null,
    long? UnidadMedidaId = null,
    string? ObservacionRenglon = null,
    decimal? PrecioListaOriginal = null,
    decimal? ComisionVendedorRenglon = null,
    long? ComprobanteItemOrigenId = null,
    decimal? CantidadDocumentoOrigen = null,
    decimal? PrecioDocumentoOrigen = null);

public record CompraRemitoResumenDto(
    long Id,
    string Tipo,
    string Proveedor,
    string Numero,
    DateOnly Fecha,
    string Deposito,
    string Estado,
    string? OrdenCompraReferencia,
    string? RecepcionReferencia,
    string Transportista,
    string ResponsableRecepcion,
    string Observacion,
    IReadOnlyList<string> DiferenciasClave,
    decimal Total,
    IReadOnlyList<CompraRemitoItemDto> Items)
{
    public string Comprobante => Numero;
    public string ComprobanteReferencia => Numero;
}

public record CompraRemitoItemDto(
    string Id,
    string Codigo,
    string Descripcion,
    decimal Cantidad,
    string Unidad,
    decimal Recibido,
    decimal Diferencia);

public record CompraNotaCreditoResumenDto(
    long Id,
    string Proveedor,
    string ComprobanteReferencia,
    string Motivo,
    string Estado,
    DateOnly Fecha,
    decimal Total,
    string? OrdenCompraReferencia,
    string? DevolucionReferencia,
    string Responsable,
    string ImpactoCuentaCorriente,
    string Observacion,
    IReadOnlyList<string> DetallesClave,
    IReadOnlyList<CompraNotaCreditoItemDto> Items)
{
    public string Comprobante => ComprobanteReferencia;
}

public record CompraNotaCreditoItemDto(
    string Id,
    string Concepto,
    decimal Importe);

public record CompraAjusteResumenDto(
    long Id,
    string Tipo,
    string Proveedor,
    string ComprobanteReferencia,
    string Motivo,
    string Estado,
    DateOnly Fecha,
    decimal Total,
    string? OrdenCompraReferencia,
    string? DevolucionReferencia,
    string Responsable,
    string ImpactoCuentaCorriente,
    string Observacion,
    string Circuito,
    bool RequiereNotaCredito,
    IReadOnlyList<string> DetallesClave,
    IReadOnlyList<CompraAjusteItemDto> Items)
{
    public string Comprobante => ComprobanteReferencia;
}

public record CompraAjusteItemDto(
    string Id,
    string Concepto,
    string Cuenta,
    decimal Importe);

public record CompraDevolucionResumenDto(
    long Id,
    string Tipo,
    string Proveedor,
    string Comprobante,
    string Motivo,
    string Estado,
    DateOnly Fecha,
    string Deposito,
    string? OrdenCompraReferencia,
    string? RemitoReferencia,
    string? RecepcionReferencia,
    string Responsable,
    string Resolucion,
    string ImpactoStock,
    bool RequiereNotaCredito,
    IReadOnlyList<string> DiferenciasClave,
    decimal Total,
    IReadOnlyList<CompraDevolucionItemDto> Items)
{
    public string ComprobanteReferencia => Comprobante;
}

public record CompraDevolucionItemDto(
    string Id,
    string Codigo,
    string Descripcion,
    decimal Cantidad,
    string Motivo);
