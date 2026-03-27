using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.Services;

public class AfipCaeComprobanteService(
    IApplicationDbContext db,
    IAfipWsfeCaeService afipWsfeCaeService,
    ICurrentUserService currentUser) : IAfipCaeComprobanteService
{
    public async Task<Result> SolicitarYAsignarAsync(Comprobante comprobante, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(comprobante.Cae))
            return Result.Failure("El comprobante ya tiene un CAE asignado.");

        if (comprobante.Estado == Domain.Enums.EstadoComprobante.Anulado)
            return Result.Failure("No se puede solicitar CAE para un comprobante anulado.");

        var tipoComprobante = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TipoComprobanteId, ct);

        if (tipoComprobante is null)
            return Result.Failure($"No se encontro el tipo de comprobante ID {comprobante.TipoComprobanteId}.");

        if (!tipoComprobante.EsVenta)
            return Result.Failure("La emision AFIP de CAE solo esta implementada para comprobantes de venta.");

        if (!comprobante.PuntoFacturacionId.HasValue)
            return Result.Failure("El comprobante no tiene punto de facturacion asociado.");

        if (!tipoComprobante.TipoAfip.HasValue)
            return Result.Failure("El tipo de comprobante no tiene codigo AFIP configurado.");

        var puntoFacturacion = await db.PuntosFacturacion
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.PuntoFacturacionId.Value, ct);

        if (puntoFacturacion is null)
            return Result.Failure($"No se encontro el punto de facturacion ID {comprobante.PuntoFacturacionId.Value}.");

        var sucursal = await db.Sucursales
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.SucursalId, ct);

        if (sucursal is null)
            return Result.Failure($"No se encontro la sucursal ID {comprobante.SucursalId}.");

        var tercero = await db.Terceros
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TerceroId, ct);

        if (tercero is null)
            return Result.Failure($"No se encontro el tercero ID {comprobante.TerceroId}.");

        var tipoDocumento = await db.TiposDocumento
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tercero.TipoDocumentoId, ct);

        if (tipoDocumento is null)
            return Result.Failure($"No se encontro el tipo de documento ID {tercero.TipoDocumentoId}.");

        var moneda = await db.Monedas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.MonedaId, ct);

        if (moneda is null)
            return Result.Failure($"No se encontro la moneda ID {comprobante.MonedaId}.");

        if (!long.TryParse(tercero.NroDocumento, out var numeroDocumento))
            return Result.Failure("El numero de documento del tercero debe ser numerico para emitir CAE en AFIP.");

        var alicuotaIds = comprobante.Items.Select(x => x.AlicuotaIvaId).Distinct().ToList();
        var alicuotas = await db.AlicuotasIva
            .AsNoTracking()
            .Where(x => alicuotaIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var ivaLines = comprobante.Items
            .Where(x => x.IvaImporte > 0)
            .GroupBy(x => x.AlicuotaIvaId)
            .Select(group =>
            {
                if (!alicuotas.TryGetValue(group.Key, out var alicuota))
                    throw new InvalidOperationException($"No se encontro la alicuota IVA ID {group.Key}.");

                return new AfipWsfeCaeAlicuotaRequest(
                    alicuota.Codigo,
                    Math.Round(group.Sum(x => x.SubtotalNeto), 2),
                    Math.Round(group.Sum(x => x.IvaImporte), 2));
            })
            .ToList();

        var tributoLines = (await ComprobanteTributoResolver.ResolveAsync(db, comprobante, ct))
            .Select(x => new AfipWsfeCaeTributoRequest(
                short.TryParse(x.Codigo, out var tributoId) ? tributoId : (short)99,
                x.Descripcion,
                x.BaseImponible,
                x.Alicuota,
                x.Importe))
            .ToList();

        var afipRequest = new SolicitarCaeAfipRequest(
            puntoFacturacion.Numero,
            tipoComprobante.TipoAfip.Value,
            comprobante.Numero.Numero,
            comprobante.Fecha,
            1,
            tipoDocumento.Codigo,
            numeroDocumento,
            Math.Round(comprobante.Total, 2),
            Math.Round(comprobante.NetoGravado, 2),
            Math.Round(comprobante.NetoNoGravado, 2),
            0,
            Math.Round(comprobante.Percepciones, 2),
            Math.Round(comprobante.IvaRi, 2),
            moneda.Codigo,
            comprobante.Cotizacion,
            ivaLines,
            tributoLines);

        RegistrarAuditoria(
            comprobante,
            AccionAuditoria.AfipSolicitud,
            $"Solicitud CAE AFIP enviada. {BuildRequestSummary(afipRequest)}");

        try
        {
            var afipResponse = await afipWsfeCaeService.SolicitarCaeAsync(afipRequest, ct);

            comprobante.AsignarCae(
                afipResponse.Cae,
                afipResponse.FechaVencimiento,
                BuildQrData(
                    sucursal.Cuit,
                    comprobante.Fecha,
                    puntoFacturacion.Numero,
                    tipoComprobante.TipoAfip.Value,
                    comprobante.Numero.Numero,
                    comprobante.Total,
                    moneda.Codigo,
                    comprobante.Cotizacion,
                    tipoDocumento.Codigo,
                    numeroDocumento,
                    afipResponse.Cae),
                currentUser.UserId);

            RegistrarAuditoria(
                comprobante,
                AccionAuditoria.CaeAsignado,
                $"CAE AFIP asignado correctamente. {BuildRequestSummary(afipRequest)} {BuildResponseSummary(afipResponse)}");

            return Result.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await TryPersistFailureAuditAsync(
                comprobante,
                $"Error al solicitar CAE AFIP. {BuildRequestSummary(afipRequest)} error={ex.Message}",
                ct);

            return Result.Failure(ex.Message);
        }
    }

    private void RegistrarAuditoria(
        Comprobante comprobante,
        AccionAuditoria accion,
        string detalle)
    {
        if (comprobante.Id <= 0)
            return;

        db.AuditoriaComprobantes.Add(
            AuditoriaComprobante.Registrar(
                comprobante.Id,
                currentUser.UserId,
                accion,
                TruncateAuditDetail(detalle),
                null));
    }

    private async Task TryPersistFailureAuditAsync(
        Comprobante comprobante,
        string detalle,
        CancellationToken ct)
    {
        if (comprobante.Id <= 0)
            return;

        try
        {
            RegistrarAuditoria(comprobante, AccionAuditoria.AfipError, detalle);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
        }
    }

    private static string BuildRequestSummary(SolicitarCaeAfipRequest request)
    {
        var summary = JsonSerializer.Serialize(new
        {
            request.PuntoVenta,
            request.TipoComprobante,
            request.NumeroComprobante,
            FechaEmision = request.FechaEmision.ToString("yyyy-MM-dd"),
            request.TipoDocumento,
            request.NumeroDocumento,
            request.ImporteTotal,
            request.ImporteNeto,
            request.ImporteTributos,
            request.ImporteIva,
            request.MonedaCodigo,
            request.MonedaCotizacion,
            Alicuotas = request.Alicuotas.Count,
            Tributos = request.Tributos.Count
        });

        return $"request={summary}";
    }

    private static string BuildResponseSummary(SolicitarCaeAfipResponse response)
    {
        var summary = JsonSerializer.Serialize(new
        {
            response.Cae,
            FechaVencimiento = response.FechaVencimiento.ToString("yyyy-MM-dd")
        });

        return $"response={summary}";
    }

    private static string TruncateAuditDetail(string detail)
        => detail.Length <= 2000 ? detail : detail[..2000];

    private static string BuildQrData(
        string cuitEmisor,
        DateOnly fechaEmision,
        short puntoVenta,
        short tipoComprobante,
        long numeroComprobante,
        decimal importeTotal,
        string monedaCodigo,
        decimal cotizacion,
        short tipoDocumento,
        long numeroDocumento,
        string cae)
    {
        var payload = JsonSerializer.Serialize(new
        {
            ver = 1,
            fecha = fechaEmision.ToString("yyyy-MM-dd"),
            cuit = long.Parse(cuitEmisor),
            ptoVta = puntoVenta,
            tipoCmp = tipoComprobante,
            nroCmp = numeroComprobante,
            importe = importeTotal,
            moneda = monedaCodigo,
            ctz = cotizacion,
            tipoDocRec = tipoDocumento,
            nroDocRec = numeroDocumento,
            tipoCodAut = "E",
            codAut = long.Parse(cae)
        });

        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        return $"https://www.afip.gob.ar/fe/qr/?p={Uri.EscapeDataString(base64)}";
    }

}