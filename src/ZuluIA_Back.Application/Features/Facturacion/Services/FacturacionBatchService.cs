using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Facturacion.Services;

public class FacturacionBatchService(
    IApplicationDbContext db,
    IMediator mediator,
    IntegracionProcesoService procesoService)
{
    public async Task<ProcesoIntegracionJob> FacturarMasivoAsync(
        IReadOnlyList<long> comprobanteOrigenIds,
        long tipoComprobanteDestinoId,
        long? puntoFacturacionId,
        DateOnly fecha,
        DateOnly? fechaVencimiento,
        string? observacion,
        OperacionStockVenta operacionStock,
        OperacionCuentaCorrienteVenta operacionCuentaCorriente,
        bool autorizarAfip,
        bool usarCaea,
        string? claveIdempotencia,
        CancellationToken ct)
    {
        var origenIds = comprobanteOrigenIds.Distinct().ToList();
        return await ProcesarAsync(
            TipoProcesoIntegracion.FacturacionMasiva,
            "Facturación masiva",
            origenIds,
            tipoComprobanteDestinoId,
            puntoFacturacionId,
            fecha,
            fechaVencimiento,
            observacion,
            operacionStock,
            operacionCuentaCorriente,
            autorizarAfip,
            usarCaea,
            claveIdempotencia,
            "FACTURACION_MASIVA",
            "Facturación masiva",
            ct);
    }

    public async Task<ProcesoIntegracionJob> FacturarAutomaticoAsync(
        long sucursalId,
        long tipoComprobanteOrigenId,
        long tipoComprobanteDestinoId,
        DateOnly desde,
        DateOnly hasta,
        long? terceroId,
        bool soloEmitidos,
        long? puntoFacturacionId,
        DateOnly fecha,
        DateOnly? fechaVencimiento,
        string? observacion,
        OperacionStockVenta operacionStock,
        OperacionCuentaCorrienteVenta operacionCuentaCorriente,
        bool autorizarAfip,
        bool usarCaea,
        string? claveIdempotencia,
        CancellationToken ct)
    {
        var origenes = db.Comprobantes.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId
                && x.TipoComprobanteId == tipoComprobanteOrigenId
                && x.Fecha >= desde
                && x.Fecha <= hasta
                && x.Estado != EstadoComprobante.Anulado
                && x.Estado != EstadoComprobante.Convertido
                && !x.IsDeleted);

        if (terceroId.HasValue)
            origenes = origenes.Where(x => x.TerceroId == terceroId.Value);

        if (soloEmitidos)
            origenes = origenes.Where(x => x.Estado == EstadoComprobante.Emitido);

        var origenIds = await origenes
            .Where(x => !db.Comprobantes.Any(h => h.ComprobanteOrigenId == x.Id && h.TipoComprobanteId == tipoComprobanteDestinoId && h.Estado != EstadoComprobante.Anulado && !h.IsDeleted))
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(ct);

        return await ProcesarAsync(
            TipoProcesoIntegracion.FacturacionAutomatica,
            "Facturación automática",
            origenIds,
            tipoComprobanteDestinoId,
            puntoFacturacionId,
            fecha,
            fechaVencimiento,
            observacion,
            operacionStock,
            operacionCuentaCorriente,
            autorizarAfip,
            usarCaea,
            claveIdempotencia,
            "FACTURACION_AUTOMATICA",
            "Facturación automática",
            ct);
    }

    private async Task<ProcesoIntegracionJob> ProcesarAsync(
        TipoProcesoIntegracion tipoProceso,
        string nombre,
        IReadOnlyList<long> origenIds,
        long tipoComprobanteDestinoId,
        long? puntoFacturacionId,
        DateOnly fecha,
        DateOnly? fechaVencimiento,
        string? observacion,
        OperacionStockVenta operacionStock,
        OperacionCuentaCorrienteVenta operacionCuentaCorriente,
        bool autorizarAfip,
        bool usarCaea,
        string? claveIdempotencia,
        string codigoMonitor,
        string descripcionMonitor,
        CancellationToken ct)
    {
        var existente = await procesoService.ObtenerPorClaveIdempotenciaAsync(tipoProceso, claveIdempotencia, ct);
        if (existente is not null)
            return existente;

        var payload = $"tipoDestino={tipoComprobanteDestinoId};cantidad={origenIds.Count}";
        var job = await procesoService.CrearJobAsync(tipoProceso, nombre, origenIds.Count, payload, ct, claveIdempotencia);

        foreach (var origenId in origenIds)
        {
            var origen = await db.Comprobantes.AsNoTracking()
                .Where(x => x.Id == origenId)
                .Select(x => new { x.Id, x.Estado, x.Numero.Formateado, x.TipoComprobanteId })
                .FirstOrDefaultAsync(ct);

            if (origen is null)
            {
                procesoService.RegistrarError(job, $"No se encontró el comprobante origen {origenId}.");
                await procesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Error, "Comprobante origen inexistente.", origenId.ToString(), null, ct);
                continue;
            }

            var duplicado = await db.Comprobantes.AsNoTracking()
                .AnyAsync(x => x.ComprobanteOrigenId == origenId
                    && x.TipoComprobanteId == tipoComprobanteDestinoId
                    && x.Estado != EstadoComprobante.Anulado
                    && !x.IsDeleted, ct);

            if (duplicado)
            {
                procesoService.RegistrarError(job, $"Ya existe una factura generada para el origen {origen.Formateado}.");
                await procesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Advertencia, "Documento duplicado detectado.", origenId.ToString(), origen.Formateado, ct);
                continue;
            }

            var conversion = await mediator.Send(new ConvertirDocumentoVentaCommand(
                origenId,
                tipoComprobanteDestinoId,
                puntoFacturacionId,
                fecha,
                fechaVencimiento,
                observacion,
                operacionStock,
                operacionCuentaCorriente), ct);

            if (!conversion.IsSuccess)
            {
                procesoService.RegistrarError(job, conversion.Error);
                await procesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Error, conversion.Error ?? "No se pudo facturar el documento.", origenId.ToString(), origen.Formateado, ct);
                continue;
            }

            if (autorizarAfip)
            {
                var afip = await mediator.Send(new Commands.AutorizarComprobanteAfipWsfeCommand(conversion.Value, usarCaea), ct);
                if (!afip.IsSuccess)
                {
                    procesoService.RegistrarError(job, afip.Error);
                    await procesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Error, afip.Error ?? "No se pudo autorizar el comprobante en AFIP.", conversion.Value.ToString(), origen.Formateado, ct);
                    continue;
                }
            }

            procesoService.RegistrarExito(job);
            await procesoService.RegistrarLogAsync(job.Id, NivelLogIntegracion.Informacion, "Documento facturado correctamente.", origenId.ToString(), conversion.Value.ToString(), ct);
        }

        procesoService.Finalizar(job, observacion);
        await procesoService.ActualizarMonitorExportacionAsync(codigoMonitor, descripcionMonitor, job, origenIds.Count - job.RegistrosExitosos, observacion, ct);
        return job;
    }
}
