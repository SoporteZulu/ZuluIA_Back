using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalProviderErrorCatalogService
{
    private static readonly IReadOnlyDictionary<ProveedorIntegracionExterna, IReadOnlyDictionary<string, ExternalProviderErrorDefinition>> Catalog
        = new Dictionary<ProveedorIntegracionExterna, IReadOnlyDictionary<string, ExternalProviderErrorDefinition>>
        {
            [ProveedorIntegracionExterna.AfipWsfe] = new Dictionary<string, ExternalProviderErrorDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["CERTIFICADO_REQUERIDO"] = new(true, "AFIP requiere certificado válido para operar en producción."),
                ["TOTAL_INVALIDO"] = new(true, "AFIP no permite autorizar comprobantes con total menor o igual a cero."),
                ["CAE_RECHAZADO"] = new(true, "AFIP rechazó la solicitud de CAE."),
                ["CAEA_RECHAZADO"] = new(true, "AFIP rechazó la solicitud de CAEA.")
            },
            [ProveedorIntegracionExterna.Ctg] = new Dictionary<string, ExternalProviderErrorDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["CUIT_TRANSPORTISTA_REQUERIDO"] = new(true, "CTG requiere CUIT de transportista."),
                ["CUITS_REQUERIDOS"] = new(true, "CTG requiere CUIT remitente y destinatario válidos."),
                ["CTG_RECHAZADO"] = new(true, "CTG rechazó la solicitud realizada."),
                ["CTG_NO_VIGENTE"] = new(true, "CTG no se encuentra vigente para la operación indicada.")
            },
            [ProveedorIntegracionExterna.Sifen] = new Dictionary<string, ExternalProviderErrorDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["CODIGO_SEGURIDAD_REQUERIDO"] = new(true, "SIFEN requiere código de seguridad."),
                ["SIFEN_RECHAZADO"] = new(true, "SIFEN rechazó el comprobante informado."),
                ["TIMBRADO_NO_VIGENTE"] = new(true, "El timbrado fiscal no está vigente para SIFEN.")
            },
            [ProveedorIntegracionExterna.Deuce] = new Dictionary<string, ExternalProviderErrorDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["REFERENCIA_REQUERIDA"] = new(true, "Deuce requiere referencia externa."),
                ["DEUCE_RECHAZADO"] = new(true, "Deuce rechazó el comprobante informado."),
                ["OPERACION_NO_VIGENTE"] = new(true, "La operación informada no está vigente en Deuce.")
            }
        };

    public ExternalProviderErrorResolution Resolve(ProveedorIntegracionExterna proveedor, string? code, string? message)
    {
        var normalizedCode = code?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedCode)
            && Catalog.TryGetValue(proveedor, out var providerCatalog)
            && providerCatalog.TryGetValue(normalizedCode, out var definition))
        {
            return new ExternalProviderErrorResolution(normalizedCode, definition.ErrorFuncional, definition.Mensaje);
        }

        var text = message?.Trim().ToUpperInvariant() ?? string.Empty;
        if (text.Contains("RECHAZ"))
            return new ExternalProviderErrorResolution(normalizedCode ?? $"{proveedor.ToString().ToUpperInvariant()}_RECHAZADO", true, message ?? "El proveedor rechazó la operación.");
        if (text.Contains("NO VIGENTE"))
            return new ExternalProviderErrorResolution(normalizedCode ?? $"{proveedor.ToString().ToUpperInvariant()}_NO_VIGENTE", true, message ?? "La operación no se encuentra vigente.");
        if (text.Contains("INVALID") || text.Contains("INVALI"))
            return new ExternalProviderErrorResolution(normalizedCode ?? $"{proveedor.ToString().ToUpperInvariant()}_INVALIDO", true, message ?? "El proveedor informó datos inválidos.");

        return new ExternalProviderErrorResolution(normalizedCode ?? "ERROR_TECNICO", false, message ?? "Error técnico de integración externa.");
    }

    public IReadOnlyList<ExternalProviderErrorCatalogDto> GetCatalog()
        => Catalog.Select(x => new ExternalProviderErrorCatalogDto
        {
            Proveedor = x.Key.ToString().ToUpperInvariant(),
            Items = x.Value.Select(i => new ExternalProviderErrorCatalogItemDto
            {
                Codigo = i.Key.ToUpperInvariant(),
                ErrorFuncional = i.Value.ErrorFuncional,
                Mensaje = i.Value.Mensaje
            }).OrderBy(i => i.Codigo).ToList().AsReadOnly()
        }).OrderBy(x => x.Proveedor).ToList().AsReadOnly();

    private sealed record ExternalProviderErrorDefinition(bool ErrorFuncional, string Mensaje);
}

public sealed record ExternalProviderErrorResolution(string Codigo, bool ErrorFuncional, string Mensaje);
