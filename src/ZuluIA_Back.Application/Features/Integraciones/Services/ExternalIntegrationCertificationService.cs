using System.Net.Http;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalIntegrationCertificationService(
    IApplicationDbContext db,
    ExternalIntegrationProviderSettingsService providerSettingsService,
    ExternalProviderHttpGateway httpGateway)
{
    public async Task<IReadOnlyList<ExternalIntegrationCertificationDto>> GetCertificationStatusAsync(CancellationToken ct = default)
    {
        var providers = await providerSettingsService.ResolveAllAsync(ct);
        var result = new List<ExternalIntegrationCertificationDto>(providers.Count);
        foreach (var provider in providers)
            result.Add(await BuildCertificationAsync(provider, ct));

        return result.AsReadOnly();
    }

    public async Task<ExternalIntegrationConnectivityDto> TestConnectivityAsync(ProveedorIntegracionExterna proveedor, CancellationToken ct = default)
    {
        var settings = await providerSettingsService.ResolveAsync(proveedor, ct);
        var certification = await BuildCertificationAsync(settings, ct);
        var stopwatch = Stopwatch.StartNew();

        if (!certification.UsaTransporteReal)
        {
            stopwatch.Stop();
            return new ExternalIntegrationConnectivityDto
            {
                Proveedor = proveedor.ToString().ToUpperInvariant(),
                Ambiente = settings.Ambiente,
                Endpoint = settings.Endpoint,
                Habilitada = settings.Habilitada,
                UsaTransporteReal = false,
                ConfiguracionValida = certification.ConfiguracionValida,
                CredencialesCompletas = certification.CredencialesCompletas,
                ConectividadOk = false,
                Mensaje = "El endpoint configurado sigue usando transporte placeholder/local.",
                DuracionMs = stopwatch.ElapsedMilliseconds,
                Issues = certification.Issues
            };
        }

        if (!Uri.TryCreate(settings.Endpoint, UriKind.Absolute, out var endpointUri))
        {
            stopwatch.Stop();
            return new ExternalIntegrationConnectivityDto
            {
                Proveedor = proveedor.ToString().ToUpperInvariant(),
                Ambiente = settings.Ambiente,
                Endpoint = settings.Endpoint,
                Habilitada = settings.Habilitada,
                UsaTransporteReal = false,
                ConfiguracionValida = false,
                CredencialesCompletas = certification.CredencialesCompletas,
                ConectividadOk = false,
                Mensaje = "Endpoint inválido o no absoluto.",
                DuracionMs = stopwatch.ElapsedMilliseconds,
                Issues = certification.Issues
            };
        }

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(settings.TimeoutMs) };
            using var request = new HttpRequestMessage(HttpMethod.Get, endpointUri);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            stopwatch.Stop();

            return new ExternalIntegrationConnectivityDto
            {
                Proveedor = proveedor.ToString().ToUpperInvariant(),
                Ambiente = settings.Ambiente,
                Endpoint = settings.Endpoint,
                Habilitada = settings.Habilitada,
                UsaTransporteReal = true,
                ConfiguracionValida = certification.ConfiguracionValida,
                CredencialesCompletas = certification.CredencialesCompletas,
                ConectividadOk = response.IsSuccessStatusCode,
                HttpStatusCode = (int)response.StatusCode,
                Mensaje = response.IsSuccessStatusCode ? "Conectividad verificada." : $"El endpoint respondió {(int)response.StatusCode}.",
                DuracionMs = stopwatch.ElapsedMilliseconds,
                Issues = certification.Issues
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ExternalIntegrationConnectivityDto
            {
                Proveedor = proveedor.ToString().ToUpperInvariant(),
                Ambiente = settings.Ambiente,
                Endpoint = settings.Endpoint,
                Habilitada = settings.Habilitada,
                UsaTransporteReal = certification.UsaTransporteReal,
                ConfiguracionValida = certification.ConfiguracionValida,
                CredencialesCompletas = certification.CredencialesCompletas,
                ConectividadOk = false,
                Mensaje = ex.Message,
                DuracionMs = stopwatch.ElapsedMilliseconds,
                Issues = certification.Issues
            };
        }
    }

    private async Task<ExternalIntegrationCertificationDto> BuildCertificationAsync(ExternalIntegrationProviderSettings settings, CancellationToken ct)
    {
        var issues = new List<string>();
        var registrosConfigurados = 0;
        var credencialesCompletas = true;
        var usaTransporteReal = httpGateway.ShouldUseRealTransport(settings.Endpoint);

        if (string.IsNullOrWhiteSpace(settings.Endpoint) || !Uri.TryCreate(settings.Endpoint, UriKind.Absolute, out _))
            issues.Add("Endpoint no configurado correctamente.");
        else if (!usaTransporteReal)
            issues.Add("El endpoint configurado sigue apuntando a un transporte placeholder/local.");

        switch (settings.Proveedor)
        {
            case ProveedorIntegracionExterna.AfipWsfe:
                var afipConfigs = await db.AfipWsfeConfiguraciones.AsNoTracking().Where(x => x.Habilitado).ToListAsync(ct);
                registrosConfigurados = afipConfigs.Count;
                if (afipConfigs.Count == 0)
                    issues.Add("No hay configuraciones AFIP/WSFE habilitadas.");
                var hasDefaultCertificate = !string.IsNullOrWhiteSpace(settings.CertificateAlias);
                if (afipConfigs.Any(x => x.Produccion && string.IsNullOrWhiteSpace(x.CertificadoAlias) && !hasDefaultCertificate))
                {
                    issues.Add("Hay puntos AFIP en producción sin certificado alias.");
                    credencialesCompletas = false;
                }
                if (afipConfigs.Any(x => string.IsNullOrWhiteSpace(x.CuitEmisor)))
                {
                    issues.Add("Hay puntos AFIP sin CUIT emisor.");
                    credencialesCompletas = false;
                }
                if (!hasDefaultCertificate && afipConfigs.All(x => string.IsNullOrWhiteSpace(x.CertificadoAlias)))
                {
                    issues.Add("No hay certificado alias configurado ni a nivel proveedor ni a nivel punto de facturación.");
                    credencialesCompletas = false;
                }
                break;

            case ProveedorIntegracionExterna.Ctg:
                registrosConfigurados = await db.CartasPorte.AsNoTracking().CountAsync(ct);
                if (!settings.Habilitada)
                    issues.Add("CTG está deshabilitado por configuración.");
                if (!HasTokenOrCredentialPair(settings))
                {
                    issues.Add("CTG requiere token, api key o usuario/clave configurados.");
                    credencialesCompletas = false;
                }
                break;

            case ProveedorIntegracionExterna.Sifen:
                registrosConfigurados = await db.TimbradosFiscales.AsNoTracking().CountAsync(x => x.Activo && !x.IsDeleted, ct);
                if (registrosConfigurados == 0)
                    issues.Add("No hay timbrados fiscales activos para SIFEN.");
                if (!HasTokenOrCredentialPair(settings))
                {
                    issues.Add("SIFEN requiere token, api key o usuario/clave configurados.");
                    credencialesCompletas = false;
                }
                break;

            case ProveedorIntegracionExterna.Deuce:
                registrosConfigurados = await db.OperacionesPuntoVenta.AsNoTracking().CountAsync(x => !x.IsDeleted, ct);
                if (!settings.Habilitada)
                    issues.Add("Deuce está deshabilitado por configuración.");
                if (!HasTokenOrCredentialPair(settings))
                {
                    issues.Add("Deuce requiere token, api key o usuario/clave configurados.");
                    credencialesCompletas = false;
                }
                break;
        }

        var desde = DateTimeOffset.UtcNow.AddDays(-7);
        var errores = await db.IntegracionesExternasAudit.AsNoTracking()
            .Where(x => x.Proveedor == settings.Proveedor && x.CreatedAt >= desde && !x.Exitoso)
            .ToListAsync(ct);

        return new ExternalIntegrationCertificationDto
        {
            Proveedor = settings.Proveedor.ToString().ToUpperInvariant(),
            Ambiente = settings.Ambiente,
            Endpoint = settings.Endpoint,
            Habilitada = settings.Habilitada,
            UsaTransporteReal = usaTransporteReal,
            ConfiguracionValida = issues.Count == 0,
            CredencialesCompletas = credencialesCompletas,
            ListoParaProduccion = settings.Habilitada && usaTransporteReal && credencialesCompletas && issues.Count == 0,
            RegistrosConfigurados = registrosConfigurados,
            ErroresRecientes = errores.Count,
            ErroresFuncionalesRecientes = errores.Count(x => x.ErrorFuncional),
            Issues = issues.AsReadOnly()
        };
    }

    private static bool HasTokenOrCredentialPair(ExternalIntegrationProviderSettings settings)
        => !string.IsNullOrWhiteSpace(settings.Token)
            || !string.IsNullOrWhiteSpace(settings.ApiKey)
            || (!string.IsNullOrWhiteSpace(settings.UserName) && !string.IsNullOrWhiteSpace(settings.Password));
}
