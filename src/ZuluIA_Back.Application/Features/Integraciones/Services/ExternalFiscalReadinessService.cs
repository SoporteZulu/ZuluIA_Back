using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalFiscalReadinessService(
    ExternalIntegrationProviderSettingsService providerSettingsService,
    ExternalIntegrationCertificationService certificationService,
    ExternalFiscalPrecheckService fiscalPrecheckService)
{
    public async Task<ExternalFiscalOperationReadinessDto> EvaluateAsync(
        ProveedorIntegracionExterna proveedor,
        string operacion,
        string defaultPath,
        long referenciaId,
        long? timbradoFiscalId = null,
        string? codigoSeguridad = null,
        string? referenciaExterna = null,
        bool? usarCaea = null,
        bool testConectividad = true,
        CancellationToken ct = default)
    {
        var provider = await providerSettingsService.ResolveAsync(proveedor, ct);
        var operation = await providerSettingsService.ResolveOperationAsync(proveedor, operacion, defaultPath, ct);
        var precheck = await fiscalPrecheckService.PrecheckAsync(proveedor, operacion, referenciaId, timbradoFiscalId, codigoSeguridad, referenciaExterna, usarCaea, ct);
        var certification = (await certificationService.GetCertificationStatusAsync(ct))
            .First(x => x.Proveedor == proveedor.ToString().ToUpperInvariant());

        ExternalIntegrationConnectivityDto? connectivity = null;
        if (testConectividad)
            connectivity = await certificationService.TestConnectivityAsync(proveedor, ct);

        var issues = certification.Issues
            .Concat(precheck.Issues)
            .Concat(connectivity?.Issues ?? [])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();

        var conectividadOk = connectivity?.ConectividadOk ?? true;
        return new ExternalFiscalOperationReadinessDto
        {
            Proveedor = proveedor.ToString().ToUpperInvariant(),
            Operacion = operation.Operation,
            ReferenciaTipo = precheck.ReferenciaTipo,
            ReferenciaId = referenciaId,
            Ambiente = provider.Ambiente,
            Endpoint = provider.Endpoint,
            Path = operation.Path,
            Habilitada = provider.Habilitada,
            UsaTransporteReal = certification.UsaTransporteReal,
            CredencialesCompletas = certification.CredencialesCompletas,
            ConfiguracionValida = certification.ConfiguracionValida,
            PrecheckOk = precheck.Ready,
            ConectividadOk = conectividadOk,
            ReadyToExecute = provider.Habilitada && certification.ConfiguracionValida && precheck.Ready && conectividadOk,
            ReadyForProduction = provider.Habilitada && certification.UsaTransporteReal && certification.ConfiguracionValida && precheck.Ready && conectividadOk,
            TimeoutMs = operation.TimeoutMs,
            Reintentos = operation.Reintentos,
            CircuitThreshold = operation.CircuitThreshold,
            CircuitOpenSeconds = (int)operation.CircuitOpenFor.TotalSeconds,
            HttpStatusCode = connectivity?.HttpStatusCode,
            DuracionMs = connectivity?.DuracionMs ?? 0,
            Mensaje = connectivity?.Mensaje,
            Issues = issues
        };
    }
}
