using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class LegacyExportacionService(
    IApplicationDbContext db,
    IntegracionProcesoService integracionProcesoService,
    ReporteExportacionService reporteExportacionService)
{
    public async Task<Result<ExportacionReporteDto>> ExportarCotAsync(Commands.ExportarCotLegacyCommand request, CancellationToken ct)
    {
        var jobExistente = await integracionProcesoService.ObtenerPorClaveIdempotenciaAsync(TipoProcesoIntegracion.ExportacionCot, request.ClaveIdempotencia, ct);
        if (jobExistente is not null)
        {
            var reporteExistente = await ConstruirReporteCotAsync(request, ct);
            return Result.Success(reporteExportacionService.Exportar(reporteExistente, request.Formato, $"export_cot_{request.Desde:yyyyMMdd}_{request.Hasta:yyyyMMdd}"));
        }

        var job = await integracionProcesoService.CrearJobAsync(
            TipoProcesoIntegracion.ExportacionCot,
            "Exportación COT",
            0,
            $"desde={request.Desde:yyyy-MM-dd};hasta={request.Hasta:yyyy-MM-dd};sucursalId={request.SucursalId}",
            ct,
            request.ClaveIdempotencia);

        try
        {
            var reporte = await ConstruirReporteCotAsync(request, ct);
            var itemsCount = reporte.Filas.Count;

            foreach (var _ in reporte.Filas)
                integracionProcesoService.RegistrarExito(job);

            integracionProcesoService.Finalizar(job, $"Exportación COT generada con {itemsCount} registros.");
            await integracionProcesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Informacion, "Archivo COT generado correctamente.", "COT", reporte.Titulo, ct);
            await integracionProcesoService.ActualizarMonitorExportacionAsync("EXPORT_COT", "Monitor exportación COT", job, 0, $"Se exportaron {itemsCount} registros.", ct);

            return Result.Success(reporteExportacionService.Exportar(reporte, request.Formato, $"export_cot_{request.Desde:yyyyMMdd}_{request.Hasta:yyyyMMdd}"));
        }
        catch (Exception ex)
        {
            integracionProcesoService.Fallar(job, ex.Message);
            await integracionProcesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Error, ex.Message, "COT", null, ct);
            await integracionProcesoService.ActualizarMonitorExportacionAsync("EXPORT_COT", "Monitor exportación COT", job, 0, ex.Message, ct);
            return Result.Failure<ExportacionReporteDto>(ex.Message);
        }
    }

    public async Task<Result<ExportacionReporteDto>> ExportarZgcotAsync(Commands.ExportarZgcotLegacyCommand request, CancellationToken ct)
    {
        var jobExistente = await integracionProcesoService.ObtenerPorClaveIdempotenciaAsync(TipoProcesoIntegracion.ExportacionZgcot, request.ClaveIdempotencia, ct);
        if (jobExistente is not null)
        {
            var reporteExistente = await ConstruirReporteZgcotAsync(request, ct);
            return Result.Success(reporteExportacionService.Exportar(reporteExistente, request.Formato, $"export_zgcot_{request.Desde:yyyyMMdd}_{request.Hasta:yyyyMMdd}"));
        }

        var job = await integracionProcesoService.CrearJobAsync(
            TipoProcesoIntegracion.ExportacionZgcot,
            "Exportación ZGCOT",
            0,
            $"desde={request.Desde:yyyy-MM-dd};hasta={request.Hasta:yyyy-MM-dd};sucursalId={request.SucursalId}",
            ct,
            request.ClaveIdempotencia);

        try
        {
            var reporte = await ConstruirReporteZgcotAsync(request, ct);
            var rowsCount = reporte.Filas.Count;

            foreach (var _ in reporte.Filas)
                integracionProcesoService.RegistrarExito(job);

            integracionProcesoService.Finalizar(job, $"Exportación ZGCOT generada con {rowsCount} renglones.");
            await integracionProcesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Informacion, "Archivo ZGCOT generado correctamente.", "ZGCOT", reporte.Titulo, ct);
            await integracionProcesoService.ActualizarMonitorExportacionAsync("EXPORT_ZGCOT", "Monitor exportación ZGCOT", job, 0, $"Se exportaron {rowsCount} renglones.", ct);

            return Result.Success(reporteExportacionService.Exportar(reporte, request.Formato, $"export_zgcot_{request.Desde:yyyyMMdd}_{request.Hasta:yyyyMMdd}"));
        }
        catch (Exception ex)
        {
            integracionProcesoService.Fallar(job, ex.Message);
            await integracionProcesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Error, ex.Message, "ZGCOT", null, ct);
            await integracionProcesoService.ActualizarMonitorExportacionAsync("EXPORT_ZGCOT", "Monitor exportación ZGCOT", job, 0, ex.Message, ct);
            return Result.Failure<ExportacionReporteDto>(ex.Message);
        }
    }

    public async Task<Result<long>> EjecutarSyncLegacyEspecificoAsync(Commands.EjecutarSyncLegacyEspecificoCommand request, CancellationToken ct)
    {
        var existente = await integracionProcesoService.ObtenerPorClaveIdempotenciaAsync(TipoProcesoIntegracion.SincronizacionLegacyEspecifica, request.ClaveIdempotencia, ct);
        if (existente is not null)
            return Result.Success(existente.Id);

        var job = await integracionProcesoService.CrearJobAsync(
            TipoProcesoIntegracion.SincronizacionLegacyEspecifica,
            $"Sync legacy {request.Codigo}",
            request.RegistrosEstimados,
            request.PayloadResumen,
            ct,
            request.ClaveIdempotencia);

        try
        {
            for (var i = 0; i < request.RegistrosEstimados; i++)
                integracionProcesoService.RegistrarExito(job);

            await integracionProcesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Informacion, $"Sincronización legacy '{request.Codigo}' ejecutada.", request.Codigo, request.PayloadResumen, ct);
            integracionProcesoService.Finalizar(job, request.Observacion ?? $"Sync legacy '{request.Codigo}' finalizada.");
            await integracionProcesoService.ActualizarMonitorExportacionAsync($"SYNC_{request.Codigo.Trim().ToUpperInvariant()}", $"Monitor sync legacy {request.Codigo}", job, 0, request.Observacion ?? "Sincronización finalizada.", ct);
            return Result.Success(job.Id);
        }
        catch (Exception ex)
        {
            integracionProcesoService.Fallar(job, ex.Message);
            await integracionProcesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Error, ex.Message, request.Codigo, request.PayloadResumen, ct);
            await integracionProcesoService.ActualizarMonitorExportacionAsync($"SYNC_{request.Codigo.Trim().ToUpperInvariant()}", $"Monitor sync legacy {request.Codigo}", job, request.RegistrosEstimados, ex.Message, ct);
            return Result.Failure<long>(ex.Message);
        }
    }

    private async Task<ReporteTabularDto> ConstruirReporteCotAsync(Commands.ExportarCotLegacyCommand request, CancellationToken ct)
    {
        var query = db.Comprobantes.AsNoTracking()
            .Where(x => x.Fecha >= request.Desde && x.Fecha <= request.Hasta && !x.IsDeleted)
            .Join(db.TiposComprobante.AsNoTracking().Where(t => t.EsVenta),
                c => c.TipoComprobanteId,
                t => t.Id,
                (c, t) => new { Comprobante = c, Tipo = t });

        if (request.SucursalId.HasValue)
            query = query.Where(x => x.Comprobante.SucursalId == request.SucursalId.Value);

        var items = await query
            .OrderBy(x => x.Comprobante.Fecha)
            .ThenBy(x => x.Comprobante.Id)
            .Select(x => new
            {
                x.Comprobante.Id,
                x.Comprobante.SucursalId,
                x.Comprobante.Fecha,
                x.Tipo.Codigo,
                Numero = x.Comprobante.Numero.Formateado,
                x.Comprobante.TerceroId,
                x.Comprobante.Total,
                x.Comprobante.Estado
            })
            .ToListAsync(ct);

        var tercerosIds = items.Select(x => x.TerceroId).Distinct().ToList();
        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => tercerosIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.NroDocumento })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Exportación COT",
            Parametros = new Dictionary<string, string>
            {
                ["Desde"] = request.Desde.ToString("yyyy-MM-dd"),
                ["Hasta"] = request.Hasta.ToString("yyyy-MM-dd"),
                ["SucursalId"] = request.SucursalId?.ToString() ?? "TODAS"
            },
            Columnas = ["Id", "Sucursal", "Fecha", "Tipo", "Número", "Tercero", "CUIT", "Total", "Estado"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.SucursalId.ToString(),
                x.Fecha.ToString("yyyy-MM-dd"),
                x.Codigo,
                x.Numero,
                terceros.GetValueOrDefault(x.TerceroId)?.RazonSocial ?? "—",
                terceros.GetValueOrDefault(x.TerceroId)?.NroDocumento ?? "—",
                x.Total.ToString("0.00"),
                x.Estado.ToString().ToUpperInvariant()
            }).ToList()
        };
    }

    private async Task<ReporteTabularDto> ConstruirReporteZgcotAsync(Commands.ExportarZgcotLegacyCommand request, CancellationToken ct)
    {
        var query = db.ComprobantesItems.AsNoTracking()
            .Join(db.Comprobantes.AsNoTracking().Where(c => c.Fecha >= request.Desde && c.Fecha <= request.Hasta && !c.IsDeleted),
                i => i.ComprobanteId,
                c => c.Id,
                (i, c) => new { Item = i, Comprobante = c })
            .Join(db.TiposComprobante.AsNoTracking().Where(t => t.EsVenta),
                x => x.Comprobante.TipoComprobanteId,
                t => t.Id,
                (x, t) => new { x.Item, x.Comprobante, Tipo = t });

        if (request.SucursalId.HasValue)
            query = query.Where(x => x.Comprobante.SucursalId == request.SucursalId.Value);

        var rows = await query
            .OrderBy(x => x.Comprobante.Fecha)
            .ThenBy(x => x.Comprobante.Id)
            .ThenBy(x => x.Item.Orden)
            .Select(x => new
            {
                x.Comprobante.Id,
                x.Comprobante.SucursalId,
                x.Comprobante.Fecha,
                Numero = x.Comprobante.Numero.Formateado,
                x.Tipo.Codigo,
                x.Item.ItemId,
                x.Item.Descripcion,
                x.Item.Cantidad,
                x.Item.TotalLinea,
                x.Item.DepositoId
            })
            .ToListAsync(ct);

        var depositosIds = rows.Where(x => x.DepositoId.HasValue).Select(x => x.DepositoId!.Value).Distinct().ToList();
        var depositos = await db.Depositos.AsNoTracking()
            .Where(x => depositosIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Exportación ZGCOT",
            Parametros = new Dictionary<string, string>
            {
                ["Desde"] = request.Desde.ToString("yyyy-MM-dd"),
                ["Hasta"] = request.Hasta.ToString("yyyy-MM-dd"),
                ["SucursalId"] = request.SucursalId?.ToString() ?? "TODAS"
            },
            Columnas = ["ComprobanteId", "Sucursal", "Fecha", "Tipo", "Número", "ItemId", "Descripción", "Cantidad", "TotalLinea", "Depósito"],
            Filas = rows.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.SucursalId.ToString(),
                x.Fecha.ToString("yyyy-MM-dd"),
                x.Codigo,
                x.Numero,
                x.ItemId.ToString(),
                x.Descripcion,
                x.Cantidad.ToString("0.####"),
                x.TotalLinea.ToString("0.00"),
                x.DepositoId.HasValue ? depositos.GetValueOrDefault(x.DepositoId.Value)?.Descripcion ?? "—" : "—"
            }).ToList()
        };
    }
}
