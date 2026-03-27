using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using DomainComprobante = ZuluIA_Back.Domain.Entities.Comprobantes.Comprobante;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Services;

public class AfipWsfeService(
    IApplicationDbContext db,
    IRepository<AfipWsfeConfiguracion> configRepo,
    IRepository<AfipWsfeAudit> auditRepo,
    ExternalIntegrationProviderSettingsService providerSettingsService,
    ExternalProviderHttpGateway httpGateway,
    ExternalIntegrationResilienceService resilienceService,
    ICurrentUserService currentUser)
{
    public async Task<AfipWsfeConfiguracion> UpsertConfiguracionAsync(
        long puntoFacturacionId,
        bool habilitado,
        bool produccion,
        bool usaCaeaPorDefecto,
        string cuitEmisor,
        string? certificadoAlias,
        string? observacion,
        CancellationToken ct = default)
    {
        var punto = await db.PuntosFacturacion.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == puntoFacturacionId, ct)
            ?? throw new InvalidOperationException($"No se encontró el punto de facturación con ID {puntoFacturacionId}.");

        var config = await db.AfipWsfeConfiguraciones
            .FirstOrDefaultAsync(x => x.PuntoFacturacionId == puntoFacturacionId, ct);

        if (config is null)
        {
            config = AfipWsfeConfiguracion.Crear(
                punto.SucursalId,
                puntoFacturacionId,
                habilitado,
                produccion,
                usaCaeaPorDefecto,
                cuitEmisor,
                certificadoAlias,
                observacion,
                currentUser.UserId);

            await configRepo.AddAsync(config, ct);
            return config;
        }

        config.Actualizar(
            habilitado,
            produccion,
            usaCaeaPorDefecto,
            cuitEmisor,
            certificadoAlias,
            observacion,
            currentUser.UserId);

        configRepo.Update(config);
        return config;
    }

    public Task<AfipWsfeOperacionDto> SolicitarCaeAsync(long comprobanteId, CancellationToken ct = default) =>
        EjecutarAsync(comprobanteId, TipoOperacionAfipWsfe.SolicitarCae, ct);

    public Task<AfipWsfeOperacionDto> SolicitarCaeaAsync(long comprobanteId, CancellationToken ct = default) =>
        EjecutarAsync(comprobanteId, TipoOperacionAfipWsfe.SolicitarCaea, ct);

    public Task<AfipWsfeOperacionDto> ConsultarAsync(long comprobanteId, CancellationToken ct = default) =>
        EjecutarAsync(comprobanteId, TipoOperacionAfipWsfe.ConsultarComprobante, ct);

    public Task<AfipWsfeOperacionDto> RefreshEstadoAsync(long comprobanteId, CancellationToken ct = default) =>
        EjecutarAsync(comprobanteId, TipoOperacionAfipWsfe.RefreshEstado, ct);

    private async Task<AfipWsfeOperacionDto> EjecutarAsync(long comprobanteId, TipoOperacionAfipWsfe operacion, CancellationToken ct)
    {
        var comprobante = await db.Comprobantes.FirstOrDefaultAsync(x => x.Id == comprobanteId, ct)
            ?? throw new InvalidOperationException($"No se encontró el comprobante con ID {comprobanteId}.");

        if (!comprobante.PuntoFacturacionId.HasValue)
            throw new InvalidOperationException("El comprobante no tiene punto de facturación asociado.");

        var config = await db.AfipWsfeConfiguraciones.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PuntoFacturacionId == comprobante.PuntoFacturacionId.Value, ct)
            ?? throw new InvalidOperationException("No existe configuración AFIP/WSFE para el punto de facturación del comprobante.");

        if (!config.Habilitado)
            throw new InvalidOperationException("La configuración AFIP/WSFE del punto de facturación está deshabilitada.");

        var providerSettings = await providerSettingsService.ResolveAsync(ProveedorIntegracionExterna.AfipWsfe, ct);
        var operationSettings = await providerSettingsService.ResolveOperationAsync(
            ProveedorIntegracionExterna.AfipWsfe,
            operacion.ToString().ToUpperInvariant(),
            operacion switch
            {
                TipoOperacionAfipWsfe.SolicitarCae => "solicitar-cae",
                TipoOperacionAfipWsfe.SolicitarCaea => "solicitar-caea",
                TipoOperacionAfipWsfe.ConsultarComprobante => "consultar",
                TipoOperacionAfipWsfe.RefreshEstado => "refresh",
                _ => string.Empty
            },
            ct);
        var certificadoAlias = string.IsNullOrWhiteSpace(config.CertificadoAlias)
            ? providerSettings.CertificateAlias
            : config.CertificadoAlias;

        if (config.Produccion && string.IsNullOrWhiteSpace(certificadoAlias))
            throw new InvalidOperationException("AFIP/WSFE en producción requiere certificado configurado a nivel punto o proveedor.");

        var requestContract = new AfipWsfeRequestContract(
            comprobante.Id,
            comprobante.SucursalId,
            comprobante.PuntoFacturacionId,
            comprobante.Numero.Prefijo,
            comprobante.TipoComprobanteId,
            comprobante.Total,
            comprobante.Fecha,
            config.CuitEmisor,
            certificadoAlias,
            providerSettings.Ambiente,
            providerSettings.Endpoint,
            operacion.ToString().ToUpperInvariant());

        var requestPayload = JsonSerializer.Serialize(requestContract);

        var execution = await resilienceService.ExecuteAsync(
            ProveedorIntegracionExterna.AfipWsfe,
            operacion.ToString().ToUpperInvariant(),
            "COMPROBANTE",
            comprobante.Id,
            requestPayload,
            token => EjecutarProveedorAfipAsync(providerSettings, operationSettings, comprobante, config, operacion, certificadoAlias, requestPayload, token),
            ct,
            timeoutMs: operationSettings.TimeoutMs,
            retryCount: operationSettings.Reintentos,
            circuitThreshold: operationSettings.CircuitThreshold,
            circuitOpenFor: operationSettings.CircuitOpenFor,
            responseSerializer: resultado => JsonSerializer.Serialize(new
            {
                resultado.Exitoso,
                resultado.EstadoAfip,
                resultado.CodigoError,
                resultado.Cae,
                resultado.Caea,
                resultado.FechaVto,
                resultado.Mensaje
            }));

        if (!execution.IsSuccess || execution.Value is null)
        {
            var responseErrorPayload = string.IsNullOrWhiteSpace(execution.ResponsePayload)
                ? JsonSerializer.Serialize(new { error = execution.Error, execution.CircuitOpen })
                : execution.ResponsePayload;

            var auditError = AfipWsfeAudit.Registrar(
                comprobante.Id,
                comprobante.SucursalId,
                comprobante.PuntoFacturacionId.Value,
                operacion,
                false,
                requestPayload,
                responseErrorPayload,
                execution.Error,
                null,
                null,
                DateOnly.FromDateTime(DateTime.Today),
                currentUser.UserId);

            await auditRepo.AddAsync(auditError, ct);
            comprobante.RegistrarEstadoAfip(EstadoAfipWsfe.Error, execution.Error, DateTimeOffset.UtcNow, currentUser.UserId);
            throw new InvalidOperationException(execution.Error ?? "Falló la operación AFIP/WSFE.");
        }

        var resultado = execution.Value;
        var responsePayload = execution.ResponsePayload;

        var audit = AfipWsfeAudit.Registrar(
            comprobante.Id,
            comprobante.SucursalId,
            comprobante.PuntoFacturacionId.Value,
            operacion,
            resultado.Exitoso,
            requestPayload,
            responsePayload,
            resultado.Exitoso ? null : resultado.Mensaje,
            resultado.Cae,
            resultado.Caea,
            DateOnly.FromDateTime(DateTime.Today),
            currentUser.UserId);

        await auditRepo.AddAsync(audit, ct);

        if (operacion == TipoOperacionAfipWsfe.SolicitarCae && resultado.Exitoso && resultado.Cae is not null && resultado.FechaVto.HasValue)
            comprobante.AsignarCae(resultado.Cae, resultado.FechaVto.Value, resultado.QrData, currentUser.UserId);
        else if (operacion == TipoOperacionAfipWsfe.SolicitarCaea && resultado.Exitoso && resultado.Caea is not null && resultado.FechaVto.HasValue)
            comprobante.AsignarCaea(resultado.Caea, resultado.FechaVto.Value, currentUser.UserId);
        else
        {
            var estadoAfip = Enum.Parse<EstadoAfipWsfe>(resultado.EstadoAfip, true);
            comprobante.RegistrarEstadoAfip(estadoAfip, resultado.Exitoso ? null : resultado.Mensaje, DateTimeOffset.UtcNow, currentUser.UserId);
        }

        return resultado;
    }

    private static AfipWsfeOperacionDto ResolverOperacion(DomainComprobante comprobante, AfipWsfeConfiguracion config, TipoOperacionAfipWsfe operacion, string? certificadoAlias)
    {
        if (config.Produccion && string.IsNullOrWhiteSpace(certificadoAlias))
            throw new ExternalProviderFunctionalException("AFIP/WSFE en producción requiere certificado configurado.", "CERTIFICADO_REQUERIDO");

        if (comprobante.Total <= 0)
            throw new ExternalProviderFunctionalException("AFIP/WSFE no permite autorizar comprobantes con total menor o igual a cero.", "TOTAL_INVALIDO");

        return operacion switch
        {
            TipoOperacionAfipWsfe.SolicitarCae => new AfipWsfeOperacionDto
            {
                ComprobanteId = comprobante.Id,
                Operacion = operacion.ToString().ToUpperInvariant(),
                Exitoso = true,
                EstadoAfip = EstadoAfipWsfe.AutorizadoCae.ToString().ToUpperInvariant(),
                Cae = $"{comprobante.Numero.Prefijo:D4}{comprobante.Id:D10}"[..14],
                FechaVto = comprobante.Fecha.AddDays(10),
                QrData = $"AFIP|CAE|{comprobante.Id}|{comprobante.Total:0.00}|{config.CuitEmisor}",
                Mensaje = "CAE autorizado correctamente."
            },
            TipoOperacionAfipWsfe.SolicitarCaea => new AfipWsfeOperacionDto
            {
                ComprobanteId = comprobante.Id,
                Operacion = operacion.ToString().ToUpperInvariant(),
                Exitoso = true,
                EstadoAfip = EstadoAfipWsfe.AutorizadoCaea.ToString().ToUpperInvariant(),
                Caea = $"{DateTime.Today:yyyyMM}{comprobante.Numero.Prefijo:D4}"[..10],
                FechaVto = comprobante.Fecha.AddDays(15),
                Mensaje = "CAEA autorizado correctamente."
            },
            TipoOperacionAfipWsfe.ConsultarComprobante or TipoOperacionAfipWsfe.RefreshEstado => new AfipWsfeOperacionDto
            {
                ComprobanteId = comprobante.Id,
                Operacion = operacion.ToString().ToUpperInvariant(),
                Exitoso = !string.IsNullOrWhiteSpace(comprobante.Cae) || !string.IsNullOrWhiteSpace(comprobante.Caea),
                EstadoAfip = (!string.IsNullOrWhiteSpace(comprobante.Cae)
                        ? EstadoAfipWsfe.AutorizadoCae
                        : !string.IsNullOrWhiteSpace(comprobante.Caea)
                            ? EstadoAfipWsfe.AutorizadoCaea
                            : EstadoAfipWsfe.Pendiente)
                    .ToString().ToUpperInvariant(),
                Cae = comprobante.Cae,
                Caea = comprobante.Caea,
                FechaVto = comprobante.FechaVtoCae,
                QrData = comprobante.QrData,
                Mensaje = !string.IsNullOrWhiteSpace(comprobante.Cae) || !string.IsNullOrWhiteSpace(comprobante.Caea)
                    ? "Estado AFIP actualizado correctamente."
                    : "El comprobante aún no posee CAE/CAEA."
            },
            _ => throw new InvalidOperationException("Operación AFIP/WSFE no soportada.")
        };
    }

    private async Task<AfipWsfeOperacionDto> EjecutarProveedorAfipAsync(
        ExternalIntegrationProviderSettings providerSettings,
        ExternalIntegrationOperationSettings operationSettings,
        DomainComprobante comprobante,
        AfipWsfeConfiguracion config,
        TipoOperacionAfipWsfe operacion,
        string? certificadoAlias,
        string requestPayload,
        CancellationToken ct)
    {
        if (!httpGateway.ShouldUseRealTransport(providerSettings.Endpoint))
            return ResolverOperacion(comprobante, config, operacion, certificadoAlias);

        var response = await httpGateway.PostJsonAsync<AfipWsfeOperacionDto>(providerSettings, operationSettings.Path, requestPayload, ct);
        return NormalizarRespuestaAfip(response, comprobante.Id, operacion);
    }

    private static AfipWsfeOperacionDto NormalizarRespuestaAfip(AfipWsfeOperacionDto response, long comprobanteId, TipoOperacionAfipWsfe operacion)
    {
        response.ComprobanteId = response.ComprobanteId == 0 ? comprobanteId : response.ComprobanteId;
        response.Operacion = string.IsNullOrWhiteSpace(response.Operacion) ? operacion.ToString().ToUpperInvariant() : response.Operacion.ToUpperInvariant();
        response.EstadoAfip = string.IsNullOrWhiteSpace(response.EstadoAfip) ? EstadoAfipWsfe.Pendiente.ToString().ToUpperInvariant() : response.EstadoAfip.ToUpperInvariant();
        response.CodigoError = string.IsNullOrWhiteSpace(response.CodigoError) ? null : response.CodigoError.ToUpperInvariant();
        response.Mensaje ??= response.Exitoso ? "Operación AFIP/WSFE ejecutada correctamente." : "AFIP/WSFE devolvió un resultado no exitoso.";
        return response;
    }
}
