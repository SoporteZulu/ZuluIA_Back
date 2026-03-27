using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Impresion.Enums;
using ZuluIA_Back.Application.Features.Impresion.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class GoLiveOperativoReadinessService(
    IApplicationDbContext db,
    OperacionesBatchSettingsService settingsService,
    ArchivoTabularParserService parserService,
    IEnumerable<IImpresoraFiscalAdapter> fiscalAdapters)
{
    public async Task<GoLiveOperativoReadinessDto> EvaluateAsync(CancellationToken ct = default)
    {
        var settings = await settingsService.ResolveAsync(ct);
        var ahora = DateTimeOffset.UtcNow;
        var programacionesActivas = await db.BatchProgramaciones.AsNoTracking().CountAsync(x => x.Activa && !x.IsDeleted, ct);
        var programacionesVencidas = await db.BatchProgramaciones.AsNoTracking().CountAsync(x => x.Activa && !x.IsDeleted && x.ProximaEjecucion <= ahora, ct);
        var spoolPendiente = await db.ImpresionSpoolTrabajos.AsNoTracking().CountAsync(x => !x.IsDeleted && x.Estado == EstadoImpresionSpool.Pendiente, ct);
        var spoolConError = await db.ImpresionSpoolTrabajos.AsNoTracking().CountAsync(x => !x.IsDeleted && x.Estado == EstadoImpresionSpool.Error, ct);
        var adapters = fiscalAdapters.Select(x => x.Marca.ToString().ToUpperInvariant()).Distinct().OrderBy(x => x).ToList().AsReadOnly();
        var supportedFormats = parserService.GetSupportedFormats();
        var issues = new List<string>();
        var schedulerReady = settings.SchedulerHabilitado && !string.IsNullOrWhiteSpace(settings.SchedulerQueueMode);
        if (!schedulerReady)
            issues.Add("Scheduler batch no está listo para producción.");

        var spoolReady = settings.SpoolHabilitado && settings.ImpresionFiscalHabilitada && !string.IsNullOrWhiteSpace(settings.SpoolQueueMode);
        if (!spoolReady)
            issues.Add("Spool de impresión no está listo para producción.");

        var parserReady = settings.ParsersHabilitados && supportedFormats.Count > 0 && !string.IsNullOrWhiteSpace(settings.LayoutLegacyProfile);
        if (!parserReady)
            issues.Add("Parsers/layouts legacy no están listos para producción.");

        var pdfReady = !string.IsNullOrWhiteSpace(settings.PdfLayoutProfile);
        if (!pdfReady)
            issues.Add("No hay layout PDF operativo configurado.");

        var hasEpson = adapters.Contains(MarcaImpresoraFiscal.Epson.ToString().ToUpperInvariant());
        var hasHasar = adapters.Contains(MarcaImpresoraFiscal.Hasar.ToString().ToUpperInvariant());
        var fiscalHardwareReady = settings.ImpresionFiscalHabilitada
            && ((!settings.EpsonHabilitada || hasEpson) && (!settings.HasarHabilitada || hasHasar));
        if (!fiscalHardwareReady)
            issues.Add("No están disponibles todos los adaptadores fiscales requeridos por configuración.");

        return new GoLiveOperativoReadinessDto
        {
            SchedulerReadyProduction = schedulerReady,
            SpoolReadyProduction = spoolReady,
            ParserReadyProduction = parserReady,
            PdfReadyProduction = pdfReady,
            FiscalHardwareReadyProduction = fiscalHardwareReady,
            ReadyForGoLive = schedulerReady && spoolReady && parserReady && pdfReady && fiscalHardwareReady,
            SchedulerQueueMode = settings.SchedulerQueueMode,
            SpoolQueueMode = settings.SpoolQueueMode,
            LayoutLegacyProfile = settings.LayoutLegacyProfile,
            PdfLayoutProfile = settings.PdfLayoutProfile,
            SupportedParserFormats = supportedFormats,
            RegisteredFiscalAdapters = adapters,
            ProgramacionesActivas = programacionesActivas,
            ProgramacionesVencidas = programacionesVencidas,
            SpoolPendiente = spoolPendiente,
            SpoolConError = spoolConError,
            Issues = issues.AsReadOnly()
        };
    }
}
