using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.Services;

public class ParaguaySifenComprobanteService(
    IApplicationDbContext db,
    IParaguaySifenService sifenService,
    ICurrentUserService currentUser) : IParaguaySifenComprobanteService
{
    public async Task<PreparacionSifenParaguayDto?> PrepararAsync(long comprobanteId, CancellationToken cancellationToken = default)
    {
        var context = await LoadContextAsync(comprobanteId, cancellationToken);
        if (context is null)
            return null;

        return await BuildPreviewAsync(context, cancellationToken);
    }

    public async Task<Result<ResultadoEnvioSifenParaguayDto>> EnviarAsync(
        Comprobante comprobante,
        CancellationToken cancellationToken = default)
    {
        var context = await LoadContextAsync(comprobante.Id, cancellationToken, comprobante);
        if (context is null)
            return Result.Failure<ResultadoEnvioSifenParaguayDto>($"No se encontro el comprobante ID {comprobante.Id}.");

        var preview = await BuildPreviewAsync(context, cancellationToken);
        if (!preview.ListoParaEnviar)
            return Result.Failure<ResultadoEnvioSifenParaguayDto>(string.Join(" | ", preview.Errores));

        RegistrarAuditoria(
            comprobante.Id,
            AccionAuditoria.SifenSolicitud,
            $"Solicitud SIFEN/SET enviada. {BuildPreviewSummary(preview)}");

        try
        {
            var response = await sifenService.EnviarAsync(context.Request, cancellationToken);

            comprobante.RegistrarResultadoSifen(
                response.Aceptado ? EstadoSifenParaguay.Aceptado : EstadoSifenParaguay.Rechazado,
                response.CodigoRespuesta,
                response.MensajeRespuesta,
                response.TrackingId,
                response.Cdc,
                response.NumeroLote,
                response.FechaRespuesta,
                currentUser.UserId);

            RegistrarHistorial(
                comprobante.Id,
                response.Aceptado ? EstadoSifenParaguay.Aceptado : EstadoSifenParaguay.Rechazado,
                response.Aceptado,
                response.Estado,
                response.CodigoRespuesta,
                response.MensajeRespuesta,
                response.TrackingId,
                response.Cdc,
                response.NumeroLote,
                response.FechaRespuesta,
                BuildResponseSummary(response),
                response.RespuestaCruda);

            RegistrarAuditoria(
                comprobante.Id,
                response.Aceptado ? AccionAuditoria.SifenAprobado : AccionAuditoria.SifenError,
                $"Respuesta SIFEN/SET recibida. {BuildResponseSummary(response)}");

            return Result.Success(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            comprobante.RegistrarResultadoSifen(
                EstadoSifenParaguay.Error,
                null,
                ex.Message,
                null,
                null,
                null,
                DateTimeOffset.UtcNow,
                currentUser.UserId);

            RegistrarHistorial(
                comprobante.Id,
                EstadoSifenParaguay.Error,
                false,
                "error",
                null,
                ex.Message,
                null,
                null,
                null,
                comprobante.SifenFechaRespuesta,
                ex.Message,
                null);

            await TryPersistFailureAuditAsync(
                comprobante.Id,
                $"Error al enviar comprobante a SIFEN/SET. comprobanteId={comprobante.Id}; error={ex.Message}",
                cancellationToken);

            return Result.Failure<ResultadoEnvioSifenParaguayDto>(ex.Message);
        }
    }

    public async Task<Result<ResultadoEnvioSifenParaguayDto>> ConciliarEstadoAsync(
        Comprobante comprobante,
        CancellationToken cancellationToken = default)
    {
        if (comprobante.Id <= 0)
            return Result.Failure<ResultadoEnvioSifenParaguayDto>("El comprobante es invalido para consultar SIFEN/SET.");

        var trackingId = comprobante.SifenTrackingId;
        var cdc = comprobante.SifenCdc;
        var numeroLote = comprobante.SifenNumeroLote;

        if (string.IsNullOrWhiteSpace(trackingId)
            && string.IsNullOrWhiteSpace(cdc)
            && string.IsNullOrWhiteSpace(numeroLote))
        {
            return Result.Failure<ResultadoEnvioSifenParaguayDto>(
                "El comprobante no tiene TrackingId, CDC ni numero de lote para consultar SIFEN/SET.");
        }

        RegistrarAuditoria(
            comprobante.Id,
            AccionAuditoria.SifenConsulta,
            $"Consulta SIFEN/SET enviada. trackingId={trackingId}; cdc={cdc}; numeroLote={numeroLote}");

        try
        {
            var response = await sifenService.ConsultarEstadoAsync(
                new ConsultarEstadoSifenRequest(comprobante.Id, trackingId, cdc, numeroLote),
                cancellationToken);

            var resolvedTrackingId = response.TrackingId ?? trackingId;
            var resolvedCdc = response.Cdc ?? cdc;
            var resolvedNumeroLote = response.NumeroLote ?? numeroLote;
            var resolvedState = response.Aceptado ? EstadoSifenParaguay.Aceptado : EstadoSifenParaguay.Rechazado;

            comprobante.RegistrarResultadoSifen(
                resolvedState,
                response.CodigoRespuesta,
                response.MensajeRespuesta,
                resolvedTrackingId,
                resolvedCdc,
                resolvedNumeroLote,
                response.FechaRespuesta,
                currentUser.UserId);

            RegistrarHistorial(
                comprobante.Id,
                resolvedState,
                response.Aceptado,
                response.Estado,
                response.CodigoRespuesta,
                response.MensajeRespuesta,
                resolvedTrackingId,
                resolvedCdc,
                resolvedNumeroLote,
                response.FechaRespuesta,
                BuildResponseSummary(response),
                response.RespuestaCruda);

            RegistrarAuditoria(
                comprobante.Id,
                response.Aceptado ? AccionAuditoria.SifenAprobado : AccionAuditoria.SifenError,
                $"Respuesta consulta SIFEN/SET recibida. {BuildResponseSummary(response)}");

            return Result.Success(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            comprobante.RegistrarResultadoSifen(
                EstadoSifenParaguay.Error,
                null,
                ex.Message,
                trackingId,
                cdc,
                numeroLote,
                DateTimeOffset.UtcNow,
                currentUser.UserId);

            RegistrarHistorial(
                comprobante.Id,
                EstadoSifenParaguay.Error,
                false,
                "error",
                null,
                ex.Message,
                trackingId,
                cdc,
                numeroLote,
                comprobante.SifenFechaRespuesta,
                ex.Message,
                null);

            await TryPersistFailureAuditAsync(
                comprobante.Id,
                $"Error al consultar estado SIFEN/SET. comprobanteId={comprobante.Id}; error={ex.Message}",
                cancellationToken);

            return Result.Failure<ResultadoEnvioSifenParaguayDto>(ex.Message);
        }
    }

    private async Task<PreparacionSifenParaguayDto> BuildPreviewAsync(
        ParaguaySifenComprobanteContext context,
        CancellationToken cancellationToken)
    {
        var comprobante = context.Comprobante;
        var sucursal = context.Sucursal;
        var tercero = context.Tercero;
        var tipoComprobante = context.TipoComprobante;
        var puntoFacturacion = context.PuntoFacturacion;

        var esSucursalParaguay = EsPaisParaguay(context.CodigoPais);
        var errores = new List<string>();

        if (sucursal is null)
            errores.Add("La sucursal del comprobante no existe.");

        if (tercero is null)
            errores.Add("El tercero del comprobante no existe.");

        if (tipoComprobante is null)
            errores.Add("El tipo de comprobante no existe.");

        if (!esSucursalParaguay)
            errores.Add("La sucursal del comprobante no pertenece a Paraguay.");

        if (comprobante.Estado != EstadoComprobante.Emitido)
            errores.Add("El comprobante debe estar emitido para preparar SIFEN/SET.");

        if (!comprobante.TimbradoId.HasValue || string.IsNullOrWhiteSpace(comprobante.NroTimbrado))
            errores.Add("El comprobante no tiene timbrado Paraguay asignado.");

        if (string.IsNullOrWhiteSpace(sucursal?.Cuit))
            errores.Add("La sucursal no tiene RUC/CUIT configurado.");

        if (string.IsNullOrWhiteSpace(sucursal?.RazonSocial))
            errores.Add("La sucursal no tiene razon social configurada.");

        if (string.IsNullOrWhiteSpace(tercero?.NroDocumento))
            errores.Add("El tercero no tiene documento configurado.");

        if (string.IsNullOrWhiteSpace(tercero?.RazonSocial))
            errores.Add("El tercero no tiene razon social configurada.");

        if (comprobante.PuntoFacturacionId.HasValue && puntoFacturacion is null)
            errores.Add("El punto de facturacion asociado no existe.");

        if (context.CantidadItems <= 0)
            errores.Add("El comprobante no tiene items para preparar SIFEN/SET.");

        var prepared = await sifenService.PrepararEnvioAsync(context.Request, cancellationToken);

        var mergedErrors = prepared.Errores
            .Concat(errores)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        prepared.EsSucursalParaguay = esSucursalParaguay;
        prepared.Errores = mergedErrors;
        prepared.ListoParaEnviar = esSucursalParaguay
            && prepared.IntegracionHabilitada
            && mergedErrors.Count == 0;

        return prepared;
    }

    private async Task<ParaguaySifenComprobanteContext?> LoadContextAsync(
        long comprobanteId,
        CancellationToken cancellationToken,
        Comprobante? comprobante = null)
    {
        comprobante ??= await db.Comprobantes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobanteId, cancellationToken);

        if (comprobante is null)
            return null;

        var sucursal = await db.Sucursales
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.SucursalId, cancellationToken);

        var tercero = await db.Terceros
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TerceroId, cancellationToken);

        var tipoComprobante = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TipoComprobanteId, cancellationToken);

        var puntoFacturacion = comprobante.PuntoFacturacionId.HasValue
            ? await db.PuntosFacturacion
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == comprobante.PuntoFacturacionId.Value, cancellationToken)
            : null;

        string? codigoPais = null;
        if (sucursal is not null)
        {
            codigoPais = await db.Paises
                .AsNoTracking()
                .Where(x => x.Id == sucursal.PaisId)
                .Select(x => x.Codigo)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var cantidadItems = await db.ComprobantesItems
            .AsNoTracking()
            .CountAsync(x => x.ComprobanteId == comprobante.Id, cancellationToken);

        var request = new PrepararEnvioSifenRequest(
            comprobante.Id,
            sucursal?.Cuit?.Trim() ?? string.Empty,
            sucursal?.RazonSocial?.Trim() ?? string.Empty,
            NormalizarTexto(sucursal?.Domicilio.Completo),
            tercero?.NroDocumento?.Trim() ?? string.Empty,
            tercero?.RazonSocial?.Trim() ?? string.Empty,
            NormalizarTexto(tercero?.Domicilio.Completo),
            tipoComprobante?.Codigo?.Trim() ?? string.Empty,
            tipoComprobante?.Descripcion?.Trim() ?? string.Empty,
            puntoFacturacion?.Numero,
            comprobante.Numero.Prefijo,
            comprobante.Numero.Numero,
            comprobante.Fecha,
            comprobante.Total,
            comprobante.NetoGravado,
            comprobante.IvaRi,
            comprobante.NetoNoGravado,
            comprobante.Percepciones,
            cantidadItems,
            comprobante.TimbradoId,
            comprobante.NroTimbrado,
            NormalizarTexto(comprobante.Observacion));

        return new ParaguaySifenComprobanteContext(
            comprobante,
            sucursal,
            tercero,
            tipoComprobante,
            puntoFacturacion,
            codigoPais,
            cantidadItems,
            request);
    }

    private void RegistrarAuditoria(long comprobanteId, AccionAuditoria accion, string detalle)
    {
        if (comprobanteId <= 0)
            return;

        db.AuditoriaComprobantes.Add(
            AuditoriaComprobante.Registrar(
                comprobanteId,
                currentUser.UserId,
                accion,
                TruncateAuditDetail(detalle),
                null));
    }

    private void RegistrarHistorial(
        long comprobanteId,
        EstadoSifenParaguay estadoSifen,
        bool aceptado,
        string? estadoRespuesta,
        string? codigoRespuesta,
        string? mensajeRespuesta,
        string? trackingId,
        string? cdc,
        string? numeroLote,
        DateTimeOffset? fechaRespuesta,
        string? detalle,
        string? respuestaCruda)
    {
        if (comprobanteId <= 0)
            return;

        db.HistorialSifenComprobantes.Add(
            HistorialSifenComprobante.Registrar(
                comprobanteId,
                currentUser.UserId,
                estadoSifen,
                aceptado,
                Truncate(estadoRespuesta, 100),
                Truncate(codigoRespuesta, 100),
                Truncate(mensajeRespuesta, 500),
                Truncate(trackingId, 100),
                Truncate(cdc, 100),
                Truncate(numeroLote, 100),
                fechaRespuesta,
                Truncate(detalle, 1000),
                Truncate(respuestaCruda, 4000)));
    }

    private async Task TryPersistFailureAuditAsync(long comprobanteId, string detalle, CancellationToken cancellationToken)
    {
        if (comprobanteId <= 0)
            return;

        try
        {
            RegistrarAuditoria(comprobanteId, AccionAuditoria.SifenError, detalle);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
        }
    }

    private static string BuildPreviewSummary(PreparacionSifenParaguayDto preview)
    {
        var summary = JsonSerializer.Serialize(new
        {
            preview.ComprobanteId,
            preview.Documento.RucEmisor,
            preview.Documento.DocumentoReceptor,
            preview.Documento.CodigoTipoComprobante,
            preview.Documento.PuntoExpedicion,
            preview.Documento.NumeroComprobante,
            preview.Documento.Total,
            preview.Documento.NroTimbrado
        });

        return $"preview={summary}";
    }

    private static string BuildResponseSummary(ResultadoEnvioSifenParaguayDto response)
    {
        var summary = JsonSerializer.Serialize(new
        {
            response.ComprobanteId,
            response.Aceptado,
            response.Estado,
            response.CodigoRespuesta,
            response.MensajeRespuesta,
            response.TrackingId,
            response.Cdc,
            response.NumeroLote,
            response.FechaRespuesta
        });

        return $"response={summary}";
    }

    private static bool EsPaisParaguay(string? codigoPais) =>
        string.Equals(codigoPais, "PY", StringComparison.OrdinalIgnoreCase)
        || string.Equals(codigoPais, "PRY", StringComparison.OrdinalIgnoreCase);

    private static string? NormalizarTexto(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string TruncateAuditDetail(string detail)
        => detail.Length <= 2000 ? detail : detail[..2000];

    private static string? Truncate(string? detail, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(detail))
            return null;

        var trimmed = detail.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private sealed record ParaguaySifenComprobanteContext(
        Comprobante Comprobante,
        Domain.Entities.Sucursales.Sucursal? Sucursal,
        Domain.Entities.Terceros.Tercero? Tercero,
        Domain.Entities.Referencia.TipoComprobante? TipoComprobante,
        Domain.Entities.Facturacion.PuntoFacturacion? PuntoFacturacion,
        string? CodigoPais,
        int CantidadItems,
        PrepararEnvioSifenRequest Request);
}