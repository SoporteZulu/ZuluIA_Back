using System.Collections.ObjectModel;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ArchivoImportLayoutProfileService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> RequiredColumnsByCircuit =
        new ReadOnlyDictionary<string, IReadOnlyList<string>>(new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["CLIENTES"] = ["Legajo", "RazonSocial", "NroDocumento"],
            ["VENTAS"] = ["ReferenciaExterna", "SucursalId", "TipoComprobanteId", "Fecha", "TerceroId", "MonedaId", "ItemId", "Cantidad", "AlicuotaIvaId"],
            ["NOTAS_PEDIDO"] = ["ReferenciaExterna", "SucursalId", "Fecha", "TerceroId"],
            ["OPERATIVAS"] = ["ReferenciaExterna", "SucursalId", "Fecha"]
        });

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AliasesByCircuit =
        new ReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>(new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["CLIENTES"] = CreateAliasMap(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["codigo"] = "Legajo",
                ["cliente"] = "RazonSocial",
                ["razon_social"] = "RazonSocial",
                ["razon social"] = "RazonSocial",
                ["documento"] = "NroDocumento",
                ["nro_documento"] = "NroDocumento",
                ["tipo_documento_id"] = "TipoDocumentoId",
                ["condicion_iva_id"] = "CondicionIvaId",
                ["telefono_1"] = "Telefono"
            }),
            ["VENTAS"] = CreateAliasMap(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["referencia"] = "ReferenciaExterna",
                ["referencia_externa"] = "ReferenciaExterna",
                ["sucursal"] = "SucursalId",
                ["tipo_comprobante_id"] = "TipoComprobanteId",
                ["cliente_id"] = "TerceroId",
                ["fecha_vto"] = "FechaVencimiento",
                ["moneda"] = "MonedaId",
                ["item"] = "ItemId",
                ["alicuota_iva_id"] = "AlicuotaIvaId",
                ["deposito"] = "DepositoId",
                ["precio_unit"] = "PrecioUnitario"
            }),
            ["NOTAS_PEDIDO"] = CreateAliasMap(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["referencia"] = "ReferenciaExterna",
                ["sucursal"] = "SucursalId",
                ["cliente_id"] = "TerceroId"
            }),
            ["OPERATIVAS"] = CreateAliasMap(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["referencia"] = "ReferenciaExterna",
                ["sucursal"] = "SucursalId"
            })
        });

    public IReadOnlyDictionary<string, string?> NormalizeRow(string circuito, IReadOnlyDictionary<string, string?> row)
    {
        var key = circuito.Trim().ToUpperInvariant();
        if (!AliasesByCircuit.TryGetValue(key, out var aliases))
            return row;

        var normalized = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in row)
        {
            var normalizedKey = aliases.TryGetValue(NormalizeKey(item.Key), out var canonical)
                ? canonical
                : item.Key.Trim();

            normalized[normalizedKey] = item.Value;
        }

        return normalized;
    }

    public IReadOnlyList<IReadOnlyDictionary<string, string?>> NormalizeRows(string circuito, IReadOnlyList<IReadOnlyDictionary<string, string?>> rows)
        => rows.Select(x => NormalizeRow(circuito, x)).ToList().AsReadOnly();

    public IReadOnlyList<string> GetRequiredColumns(string circuito)
        => RequiredColumnsByCircuit.TryGetValue(circuito.Trim().ToUpperInvariant(), out var required)
            ? required
            : [];

    public IReadOnlyDictionary<string, string> GetAliases(string circuito)
        => AliasesByCircuit.TryGetValue(circuito.Trim().ToUpperInvariant(), out var aliases)
            ? aliases
            : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    public IReadOnlyList<string> GetSupportedCircuits()
        => RequiredColumnsByCircuit.Keys.OrderBy(x => x).ToList().AsReadOnly();

    public ArchivoLayoutTemplateDto GetTemplate(string circuito)
    {
        var key = circuito.Trim().ToUpperInvariant();
        var required = GetRequiredColumns(key);
        if (required.Count == 0)
            throw new InvalidOperationException($"El circuito '{circuito}' no está soportado para plantillas de importación.");

        var sampleRow = required.ToDictionary(x => x, BuildSampleValue, StringComparer.OrdinalIgnoreCase);
        return new ArchivoLayoutTemplateDto
        {
            Circuito = key,
            RequiredColumns = required,
            Aliases = GetAliases(key),
            SampleRow = new ReadOnlyDictionary<string, string?>(sampleRow)
        };
    }

    private static IReadOnlyDictionary<string, string> CreateAliasMap(IDictionary<string, string> map)
        => new ReadOnlyDictionary<string, string>(map.ToDictionary(x => NormalizeKey(x.Key), x => x.Value, StringComparer.OrdinalIgnoreCase));

    private static string NormalizeKey(string key)
        => key.Trim().Replace("_", " ", StringComparison.Ordinal).Replace("  ", " ", StringComparison.Ordinal).ToUpperInvariant();

    private static string? BuildSampleValue(string column)
        => column switch
        {
            "Legajo" => "CLI-0001",
            "RazonSocial" => "CLIENTE EJEMPLO SA",
            "NroDocumento" => "30712345678",
            "ReferenciaExterna" => "REF-0001",
            "SucursalId" => "1",
            "TipoComprobanteId" => "1",
            "Fecha" => "2026-01-15",
            "TerceroId" => "1",
            "MonedaId" => "1",
            "ItemId" => "1001",
            "Cantidad" => "1",
            "AlicuotaIvaId" => "5",
            _ => null
        };
}
