using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class FiscalHardwareDiagnosticService(
    OperacionesBatchSettingsService settingsService,
    IEnumerable<IImpresoraFiscalAdapter> adapters)
{
    public async Task<IReadOnlyList<FiscalHardwareDiagnosticDto>> DiagnoseAsync(CancellationToken ct = default)
    {
        var settings = await settingsService.ResolveAsync(ct);
        var registered = adapters.ToDictionary(x => x.Marca, x => x);
        var items = new List<FiscalHardwareDiagnosticDto>
        {
            BuildDiagnostic("EPSON", settings.EpsonHabilitada, registered.TryGetValue(Features.Impresion.Enums.MarcaImpresoraFiscal.Epson, out var epson) ? epson : null),
            BuildDiagnostic("HASAR", settings.HasarHabilitada, registered.TryGetValue(Features.Impresion.Enums.MarcaImpresoraFiscal.Hasar, out var hasar) ? hasar : null)
        };

        return items.AsReadOnly();
    }

    private static FiscalHardwareDiagnosticDto BuildDiagnostic(string marca, bool enabledBySettings, IImpresoraFiscalAdapter? adapter)
    {
        var issues = new List<string>();
        var adapterRegistered = adapter is not null;
        var samplePayloadOk = false;
        string? message = null;

        if (!enabledBySettings)
            issues.Add("La marca está deshabilitada por configuración.");
        if (!adapterRegistered)
            issues.Add("No hay adaptador registrado para la marca.");

        if (enabledBySettings && adapterRegistered)
        {
            try
            {
                var result = adapter!.Imprimir(CreateSampleComprobante());
                samplePayloadOk = !string.IsNullOrWhiteSpace(result.PayloadFiscal);
                message = samplePayloadOk
                    ? "El adaptador generó payload de prueba correctamente."
                    : "El adaptador no generó payload de prueba.";
                if (!samplePayloadOk)
                    issues.Add("El adaptador no generó payload fiscal de prueba.");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                issues.Add($"Falló la generación de payload de prueba: {ex.Message}");
            }
        }

        return new FiscalHardwareDiagnosticDto
        {
            Marca = marca,
            HabilitadaPorConfiguracion = enabledBySettings,
            AdaptadorRegistrado = adapterRegistered,
            SamplePayloadOk = samplePayloadOk,
            Mensaje = message,
            Issues = issues.AsReadOnly()
        };
    }

    private static ComprobanteDetalleDto CreateSampleComprobante()
        => new()
        {
            Id = 1,
            NumeroFormateado = "0001-00000001",
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            TerceroRazonSocial = "CLIENTE PRUEBA",
            TerceroCuit = "20000000001",
            Total = 123.45m,
            Items =
            [
                new ComprobanteItemDto
                {
                    Id = 1,
                    ItemId = 1,
                    ItemCodigo = "ITEM-TEST",
                    Descripcion = "ITEM PRUEBA",
                    Cantidad = 1,
                    PrecioUnitario = 123.45m,
                    TotalLinea = 123.45m
                }
            ]
        };
}
