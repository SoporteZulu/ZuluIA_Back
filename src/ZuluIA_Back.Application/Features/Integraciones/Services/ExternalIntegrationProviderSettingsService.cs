using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalIntegrationProviderSettingsService(IApplicationDbContext db)
{
    public async Task<ExternalIntegrationProviderSettings> ResolveAsync(ProveedorIntegracionExterna proveedor, CancellationToken ct = default)
    {
        var prefijo = $"INTEGRACION.{proveedor.ToString().ToUpperInvariant()}";
        var values = await db.Config.AsNoTracking()
            .Where(x => x.Campo.StartsWith(prefijo))
            .ToDictionaryAsync(x => x.Campo, x => x.Valor, ct);

        var ambiente = Get(values, $"{prefijo}.AMBIENTE") ?? "TEST";
        return new ExternalIntegrationProviderSettings
        {
            Proveedor = proveedor,
            Ambiente = ambiente.ToUpperInvariant(),
            Endpoint = Get(values, $"{prefijo}.ENDPOINT") ?? GetDefaultEndpoint(proveedor, ambiente),
            Habilitada = GetBool(values, $"{prefijo}.HABILITADA", true),
            TimeoutMs = GetInt(values, $"{prefijo}.TIMEOUT_MS", 15000),
            Reintentos = GetInt(values, $"{prefijo}.REINTENTOS", 2),
            CircuitThreshold = GetInt(values, $"{prefijo}.CIRCUIT_THRESHOLD", 3),
            CircuitOpenFor = TimeSpan.FromSeconds(GetInt(values, $"{prefijo}.CIRCUIT_OPEN_SECONDS", 60)),
            ApiKey = Get(values, $"{prefijo}.API_KEY"),
            UserName = Get(values, $"{prefijo}.USERNAME"),
            Password = Get(values, $"{prefijo}.PASSWORD"),
            Token = Get(values, $"{prefijo}.TOKEN"),
            CertificateAlias = Get(values, $"{prefijo}.CERTIFICATE_ALIAS")
        };
    }

    public async Task<IReadOnlyList<ExternalIntegrationProviderSettings>> ResolveAllAsync(CancellationToken ct = default)
    {
        var proveedores = Enum.GetValues<ProveedorIntegracionExterna>();
        var items = new List<ExternalIntegrationProviderSettings>(proveedores.Length);
        foreach (var proveedor in proveedores)
            items.Add(await ResolveAsync(proveedor, ct));
        return items.AsReadOnly();
    }

    public async Task<ExternalIntegrationOperationSettings> ResolveOperationAsync(ProveedorIntegracionExterna proveedor, string operation, string defaultPath, CancellationToken ct = default)
    {
        var provider = await ResolveAsync(proveedor, ct);
        var operationKey = operation.Trim().ToUpperInvariant();
        var prefijo = $"INTEGRACION.{proveedor.ToString().ToUpperInvariant()}.OPERACION.{operationKey}";
        var values = await db.Config.AsNoTracking()
            .Where(x => x.Campo.StartsWith(prefijo))
            .ToDictionaryAsync(x => x.Campo, x => x.Valor, ct);

        return new ExternalIntegrationOperationSettings
        {
            Operation = operationKey,
            Path = Get(values, $"{prefijo}.PATH") ?? defaultPath,
            TimeoutMs = GetInt(values, $"{prefijo}.TIMEOUT_MS", provider.TimeoutMs),
            Reintentos = GetInt(values, $"{prefijo}.REINTENTOS", provider.Reintentos),
            CircuitThreshold = GetInt(values, $"{prefijo}.CIRCUIT_THRESHOLD", provider.CircuitThreshold),
            CircuitOpenFor = TimeSpan.FromSeconds(GetInt(values, $"{prefijo}.CIRCUIT_OPEN_SECONDS", (int)provider.CircuitOpenFor.TotalSeconds))
        };
    }

    private static string? Get(IReadOnlyDictionary<string, string?> values, string key)
        => values.TryGetValue(key, out var value) ? value?.Trim() : null;

    private static int GetInt(IReadOnlyDictionary<string, string?> values, string key, int @default)
        => int.TryParse(Get(values, key), out var value) ? value : @default;

    private static bool GetBool(IReadOnlyDictionary<string, string?> values, string key, bool @default)
        => bool.TryParse(Get(values, key), out var value) ? value : @default;

    private static string GetDefaultEndpoint(ProveedorIntegracionExterna proveedor, string ambiente)
    {
        var env = ambiente.Trim().ToUpperInvariant();
        var host = env == "PROD" || env == "PRODUCCION" ? "prod" : "test";
        return proveedor switch
        {
            ProveedorIntegracionExterna.AfipWsfe => $"https://{host}.afip.local/wsfe",
            ProveedorIntegracionExterna.Ctg => $"https://{host}.afip.local/ctg",
            ProveedorIntegracionExterna.Sifen => $"https://{host}.sifen.local/api",
            ProveedorIntegracionExterna.Deuce => $"https://{host}.deuce.local/api",
            _ => $"https://{host}.integracion.local/api"
        };
    }
}
