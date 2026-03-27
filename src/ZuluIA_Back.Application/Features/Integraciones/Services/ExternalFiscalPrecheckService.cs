using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalFiscalPrecheckService(
    IApplicationDbContext db,
    ExternalIntegrationProviderSettingsService providerSettingsService)
{
    public async Task<ExternalFiscalPrecheckDto> PrecheckAsync(
        ProveedorIntegracionExterna proveedor,
        string operacion,
        long referenciaId,
        long? timbradoFiscalId = null,
        string? codigoSeguridad = null,
        string? referenciaExterna = null,
        bool? usarCaea = null,
        CancellationToken ct = default)
    {
        var settings = await providerSettingsService.ResolveAsync(proveedor, ct);
        var issues = proveedor switch
        {
            ProveedorIntegracionExterna.AfipWsfe => await PrecheckAfipAsync(referenciaId, usarCaea, settings, ct),
            ProveedorIntegracionExterna.Ctg => await PrecheckCtgAsync(referenciaId, operacion, settings, ct),
            ProveedorIntegracionExterna.Sifen => await PrecheckSifenAsync(referenciaId, timbradoFiscalId, codigoSeguridad, settings, ct),
            ProveedorIntegracionExterna.Deuce => await PrecheckDeuceAsync(referenciaId, referenciaExterna, settings, ct),
            _ => ["Proveedor no soportado para precheck."]
        };

        return new ExternalFiscalPrecheckDto
        {
            Proveedor = proveedor.ToString().ToUpperInvariant(),
            Operacion = operacion.Trim().ToUpperInvariant(),
            ReferenciaTipo = proveedor == ProveedorIntegracionExterna.Ctg ? "CARTA_PORTE" : "COMPROBANTE",
            ReferenciaId = referenciaId,
            Ready = issues.Count == 0,
            Ambiente = settings.Ambiente,
            Endpoint = settings.Endpoint,
            Issues = issues.AsReadOnly()
        };
    }

    private async Task<List<string>> PrecheckAfipAsync(long comprobanteId, bool? usarCaea, ExternalIntegrationProviderSettings settings, CancellationToken ct)
    {
        var issues = ValidateProviderCredentials(settings, requiresCertificateAlias: true);
        var comprobante = await db.Comprobantes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == comprobanteId && !x.IsDeleted, ct);
        if (comprobante is null)
        {
            issues.Add($"No se encontró el comprobante ID {comprobanteId}.");
            return issues;
        }

        if (!comprobante.PuntoFacturacionId.HasValue)
            issues.Add("El comprobante no tiene punto de facturación asociado.");

        if (comprobante.Total <= 0)
            issues.Add("El comprobante debe tener total mayor a cero.");

        if (comprobante.Estado != EstadoComprobante.Emitido)
            issues.Add("El comprobante debe estar emitido para operar con AFIP.");

        if (comprobante.PuntoFacturacionId.HasValue)
        {
            var config = await db.AfipWsfeConfiguraciones.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PuntoFacturacionId == comprobante.PuntoFacturacionId.Value, ct);

            if (config is null)
                issues.Add("No existe configuración AFIP/WSFE para el punto de facturación.");
            else
            {
                if (!config.Habilitado)
                    issues.Add("La configuración AFIP/WSFE del punto está deshabilitada.");
                if (string.IsNullOrWhiteSpace(config.CuitEmisor))
                    issues.Add("La configuración AFIP/WSFE no tiene CUIT emisor.");
                if (config.Produccion && string.IsNullOrWhiteSpace(config.CertificadoAlias) && string.IsNullOrWhiteSpace(settings.CertificateAlias))
                    issues.Add("AFIP producción requiere certificado alias a nivel punto o proveedor.");
                if (usarCaea == true && !config.UsaCaeaPorDefecto)
                    issues.Add("La configuración AFIP/WSFE no está preparada para usar CAEA por defecto.");
            }
        }

        return issues;
    }

    private async Task<List<string>> PrecheckCtgAsync(long cartaPorteId, string operacion, ExternalIntegrationProviderSettings settings, CancellationToken ct)
    {
        var issues = ValidateProviderCredentials(settings);
        var carta = await db.CartasPorte.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cartaPorteId && !x.IsDeleted, ct);
        if (carta is null)
        {
            issues.Add($"No se encontró la carta de porte ID {cartaPorteId}.");
            return issues;
        }

        if (string.IsNullOrWhiteSpace(carta.CuitRemitente) || string.IsNullOrWhiteSpace(carta.CuitDestinatario))
            issues.Add("La carta de porte requiere CUIT remitente y destinatario.");

        var op = operacion.Trim().ToUpperInvariant();
        if (op.Contains("SOLICITAR"))
        {
            if (string.IsNullOrWhiteSpace(carta.CuitTransportista))
                issues.Add("La carta de porte requiere CUIT transportista para solicitar CTG.");
            if (carta.Estado is not (EstadoCartaPorte.OrdenCargaAsignada or EstadoCartaPorte.CtgError))
                issues.Add("La carta de porte debe tener orden de carga asignada o estar en error CTG para solicitar.");
        }

        if (op.Contains("CONSULTAR") && string.IsNullOrWhiteSpace(carta.NroCtg) && carta.Estado != EstadoCartaPorte.CtgSolicitado)
            issues.Add("Para consultar CTG la carta debe tener nro CTG o encontrarse en estado CTG solicitado.");

        return issues;
    }

    private async Task<List<string>> PrecheckSifenAsync(long comprobanteId, long? timbradoFiscalId, string? codigoSeguridad, ExternalIntegrationProviderSettings settings, CancellationToken ct)
    {
        var issues = ValidateProviderCredentials(settings);
        var comprobante = await db.Comprobantes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == comprobanteId && !x.IsDeleted, ct);
        if (comprobante is null)
        {
            issues.Add($"No se encontró el comprobante ID {comprobanteId}.");
            return issues;
        }

        if (!comprobante.PuntoFacturacionId.HasValue)
            issues.Add("El comprobante no posee punto de facturación para SIFEN.");

        if (string.IsNullOrWhiteSpace(codigoSeguridad))
            issues.Add("Debe informarse código de seguridad para SIFEN.");

        if (timbradoFiscalId.HasValue)
        {
            var timbrado = await db.TimbradosFiscales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == timbradoFiscalId.Value && !x.IsDeleted, ct);
            if (timbrado is null)
                issues.Add($"No se encontró el timbrado ID {timbradoFiscalId.Value}.");
            else if (!timbrado.VigentePara(comprobante.Fecha))
                issues.Add("El timbrado indicado no está vigente para la fecha del comprobante.");
        }
        else
        {
            var tieneTimbradoActivo = await db.TimbradosFiscales.AsNoTracking()
                .AnyAsync(x => x.Activo && !x.IsDeleted && x.PuntoFacturacionId == comprobante.PuntoFacturacionId, ct);
            if (!tieneTimbradoActivo)
                issues.Add("No hay timbrado fiscal activo para el punto de facturación del comprobante.");
        }

        return issues;
    }

    private async Task<List<string>> PrecheckDeuceAsync(long comprobanteId, string? referenciaExterna, ExternalIntegrationProviderSettings settings, CancellationToken ct)
    {
        var issues = ValidateProviderCredentials(settings);
        var comprobante = await db.Comprobantes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == comprobanteId && !x.IsDeleted, ct);
        if (comprobante is null)
        {
            issues.Add($"No se encontró el comprobante ID {comprobanteId}.");
            return issues;
        }

        if (!comprobante.PuntoFacturacionId.HasValue)
            issues.Add("El comprobante no posee punto de facturación para Deuce.");

        if (string.IsNullOrWhiteSpace(referenciaExterna))
            issues.Add("Debe informarse referencia externa para Deuce.");

        return issues;
    }

    private static List<string> ValidateProviderCredentials(ExternalIntegrationProviderSettings settings, bool requiresCertificateAlias = false)
    {
        var issues = new List<string>();

        if (!settings.Habilitada)
            issues.Add("El proveedor está deshabilitado por configuración.");

        if (string.IsNullOrWhiteSpace(settings.Endpoint) || !Uri.TryCreate(settings.Endpoint, UriKind.Absolute, out _))
            issues.Add("El endpoint del proveedor no es válido.");

        var hasCredentials = !string.IsNullOrWhiteSpace(settings.Token)
            || !string.IsNullOrWhiteSpace(settings.ApiKey)
            || (!string.IsNullOrWhiteSpace(settings.UserName) && !string.IsNullOrWhiteSpace(settings.Password));

        if (!hasCredentials)
            issues.Add("No hay credenciales configuradas para el proveedor.");

        if (requiresCertificateAlias && string.IsNullOrWhiteSpace(settings.CertificateAlias))
            issues.Add("No hay certificate alias configurado a nivel proveedor.");

        return issues;
    }
}
