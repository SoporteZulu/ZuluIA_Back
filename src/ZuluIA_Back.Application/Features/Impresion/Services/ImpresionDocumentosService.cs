using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Queries;
using ZuluIA_Back.Application.Features.Impresion.Enums;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;

namespace ZuluIA_Back.Application.Features.Impresion.Services;

public class ImpresionDocumentosService(
    IMediator mediator,
    IApplicationDbContext db,
    ReporteExportacionService exportacionService,
    OperacionesBatchSettingsService settingsService,
    ReportesService reportesService)
{
    public async Task<ExportacionReporteDto> GenerarPdfComprobanteAsync(long comprobanteId, CancellationToken ct)
    {
        var comprobante = await mediator.Send(new GetComprobanteDetalleQuery(comprobanteId), ct)
            ?? throw new InvalidOperationException($"No se encontró el comprobante ID {comprobanteId}.");

        var reporte = new ReporteTabularDto
        {
            Titulo = $"Comprobante {comprobante.NumeroFormateado}",
            Parametros = new Dictionary<string, string>
            {
                ["Sucursal"] = comprobante.SucursalRazonSocial,
                ["Tipo"] = comprobante.TipoComprobanteDescripcion,
                ["Fecha"] = comprobante.Fecha.ToString("yyyy-MM-dd"),
                ["Vencimiento"] = comprobante.FechaVencimiento?.ToString("yyyy-MM-dd") ?? "—",
                ["Cliente"] = comprobante.TerceroRazonSocial,
                ["CUIT"] = comprobante.TerceroCuit,
                ["Condición IVA"] = comprobante.TerceroCondicionIva,
                ["Estado"] = comprobante.Estado,
                ["Subtotal"] = $"{comprobante.MonedaSimbolo}{comprobante.Subtotal:0.00}",
                ["Descuento"] = $"{comprobante.MonedaSimbolo}{comprobante.DescuentoImporte:0.00}",
                ["IVA"] = $"{comprobante.MonedaSimbolo}{(comprobante.IvaRi + comprobante.IvaRni):0.00}",
                ["Percepciones"] = $"{comprobante.MonedaSimbolo}{comprobante.Percepciones:0.00}",
                ["Retenciones"] = $"{comprobante.MonedaSimbolo}{comprobante.Retenciones:0.00}",
                ["Total"] = $"{comprobante.MonedaSimbolo}{comprobante.Total:0.00}",
                ["Saldo"] = $"{comprobante.MonedaSimbolo}{comprobante.Saldo:0.00}",
                ["AFIP"] = comprobante.EstadoAfip,
                ["CAE"] = comprobante.Cae ?? comprobante.Caea ?? "—"
            },
            Columnas = ["Código", "Descripción", "Atributos", "Cantidad", "P. Unitario", "IVA", "Total"],
            Filas = comprobante.Items.Select(x => (IReadOnlyList<string>)
            [
                x.ItemCodigo ?? "—",
                x.Descripcion,
                x.Atributos.Count > 0 ? string.Join("; ", x.Atributos.Select(a => $"{a.AtributoDescripcion}: {a.Valor}")) : "—",
                x.Cantidad.ToString("0.####"),
                x.PrecioUnitario.ToString("0.00"),
                x.IvaImporte.ToString("0.00"),
                x.TotalLinea.ToString("0.00")
            ]).ToList().AsReadOnly()
        };

        var settings = await settingsService.ResolveAsync(ct);
        return exportacionService.Exportar(reporte, FormatoExportacionReporte.Pdf, $"comprobante_{comprobante.Id}", settings.PdfLayoutProfile);
    }

    public async Task<ExportacionReporteDto> GenerarPdfLibroIvaAsync(long sucursalId, DateOnly desde, DateOnly hasta, TipoLibroIva tipo, CancellationToken ct)
    {
        var libro = await mediator.Send(new GetLibroIvaQuery(sucursalId, desde, hasta, tipo), ct);
        var reporte = new ReporteTabularDto
        {
            Titulo = $"Libro IVA {tipo}",
            Parametros = new Dictionary<string, string>
            {
                ["SucursalId"] = sucursalId.ToString(),
                ["Desde"] = desde.ToString("yyyy-MM-dd"),
                ["Hasta"] = hasta.ToString("yyyy-MM-dd")
            },
            Columnas = ["Fecha", "Tipo", "Número", "Razón Social", "CUIT", "Neto", "IVA", "Percepciones", "Total"],
            Filas = libro.Lineas.Select(x => (IReadOnlyList<string>)
            [
                x.Fecha.ToString("yyyy-MM-dd"),
                x.TipoComprobante,
                x.NumeroFormateado,
                x.RazonSocial,
                x.Cuit,
                x.Neto.ToString("0.00"),
                x.Iva.ToString("0.00"),
                x.Percepciones.ToString("0.00"),
                x.Total.ToString("0.00")
            ]).ToList().AsReadOnly()
        };

        var settings = await settingsService.ResolveAsync(ct);
        return exportacionService.Exportar(reporte, FormatoExportacionReporte.Pdf, $"libro_iva_{tipo.ToString().ToLowerInvariant()}_{sucursalId}", settings.PdfLayoutProfile);
    }

    public async Task<ExportacionReporteDto> GenerarPdfReimpresionAsync(CategoriaReimpresionReporte categoria, long? sucursalId, long? ejercicioId, DateOnly desde, DateOnly hasta, long? depositoId, CancellationToken ct)
    {
        var reporte = categoria switch
        {
            CategoriaReimpresionReporte.Ventas => await GenerarReporteComercialAsync(true, sucursalId, desde, hasta, ct),
            CategoriaReimpresionReporte.Compras => await GenerarReporteComercialAsync(false, sucursalId, desde, hasta, ct),
            CategoriaReimpresionReporte.Stock => await reportesService.GetInformeOperativoAsync(sucursalId ?? throw new InvalidOperationException("La sucursal es obligatoria para la reimpresión de stock."), desde, hasta, depositoId, ct),
            _ => await GenerarReporteFinancieroAsync(sucursalId, desde, hasta, ct)
        };

        var settings = await settingsService.ResolveAsync(ct);
        return exportacionService.Exportar(reporte, FormatoExportacionReporte.Pdf, $"reimpresion_{categoria.ToString().ToLowerInvariant()}", settings.PdfLayoutProfile);
    }

    private async Task<ReporteTabularDto> GenerarReporteComercialAsync(bool esVenta, long? sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var tipos = await db.TiposComprobante.AsNoTracking()
            .Where(x => esVenta ? x.EsVenta : x.EsCompra)
            .Select(x => new { x.Id, x.Descripcion })
            .ToListAsync(ct);

        var tipoIds = tipos.Select(x => x.Id).ToList();
        var query = db.Comprobantes.AsNoTracking()
            .Where(x => tipoIds.Contains(x.TipoComprobanteId) && x.Fecha >= desde && x.Fecha <= hasta && !x.IsDeleted);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var comprobantes = await query
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .Select(x => new { x.Id, x.SucursalId, x.TipoComprobanteId, Numero = x.Numero.Formateado, x.Fecha, x.TerceroId, x.Total, x.Saldo, x.Estado })
            .ToListAsync(ct);

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => comprobantes.Select(c => c.TerceroId).Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var tiposLookup = tipos.ToDictionary(x => x.Id);
        return new ReporteTabularDto
        {
            Titulo = esVenta ? "Reimpresión de Ventas" : "Reimpresión de Compras",
            Parametros = new Dictionary<string, string>
            {
                ["Desde"] = desde.ToString("yyyy-MM-dd"),
                ["Hasta"] = hasta.ToString("yyyy-MM-dd"),
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas"
            },
            Columnas = ["Fecha", "Tipo", "Número", "Tercero", "Estado", "Total", "Saldo"],
            Filas = comprobantes.Select(x => (IReadOnlyList<string>)
            [
                x.Fecha.ToString("yyyy-MM-dd"),
                tiposLookup.GetValueOrDefault(x.TipoComprobanteId)?.Descripcion ?? "—",
                x.Numero,
                terceros.GetValueOrDefault(x.TerceroId)?.RazonSocial ?? "—",
                x.Estado.ToString().ToUpperInvariant(),
                x.Total.ToString("0.00"),
                x.Saldo.ToString("0.00")
            ]).ToList().AsReadOnly()
        };
    }

    private async Task<ReporteTabularDto> GenerarReporteFinancieroAsync(long? sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var cobros = db.Cobros.AsNoTracking().Where(x => x.Fecha >= desde && x.Fecha <= hasta && !x.IsDeleted);
        var pagos = db.Pagos.AsNoTracking().Where(x => x.Fecha >= desde && x.Fecha <= hasta && !x.IsDeleted);
        var movimientos = db.TesoreriaMovimientos.AsNoTracking().Where(x => x.Fecha >= desde && x.Fecha <= hasta && !x.Anulado);

        if (sucursalId.HasValue)
        {
            cobros = cobros.Where(x => x.SucursalId == sucursalId.Value);
            pagos = pagos.Where(x => x.SucursalId == sucursalId.Value);
            movimientos = movimientos.Where(x => x.SucursalId == sucursalId.Value);
        }

        var reporte = new ReporteTabularDto
        {
            Titulo = "Reimpresión Financiera",
            Parametros = new Dictionary<string, string>
            {
                ["Desde"] = desde.ToString("yyyy-MM-dd"),
                ["Hasta"] = hasta.ToString("yyyy-MM-dd"),
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas"
            },
            Columnas = ["Tipo", "Cantidad", "Importe Total"],
            Filas = new List<IReadOnlyList<string>>
            {
                new [] { "Cobros", (await cobros.CountAsync(ct)).ToString(), (await cobros.SumAsync(x => (decimal?)x.Total, ct) ?? 0m).ToString("0.00") },
                new [] { "Pagos", (await pagos.CountAsync(ct)).ToString(), (await pagos.SumAsync(x => (decimal?)x.Total, ct) ?? 0m).ToString("0.00") },
                new [] { "Movimientos Tesorería", (await movimientos.CountAsync(ct)).ToString(), (await movimientos.SumAsync(x => (decimal?)x.Importe, ct) ?? 0m).ToString("0.00") }
            }.AsReadOnly()
        };

        return reporte;
    }
}
