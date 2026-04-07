using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class OperacionesBatchSettingsService(IApplicationDbContext db)
{
    public async Task<OperacionesBatchSettings> ResolveAsync(CancellationToken ct = default)
    {
        const string prefix = "OPERACIONES";
        var values = await db.Config.AsNoTracking()
            .Where(x => x.Campo.StartsWith(prefix))
            .ToDictionaryAsync(x => x.Campo, x => x.Valor, ct);

        return new OperacionesBatchSettings
        {
            SchedulerHabilitado = GetBool(values, "OPERACIONES.BATCH.SCHEDULER.HABILITADO", true),
            SchedulerPollSeconds = GetInt(values, "OPERACIONES.BATCH.SCHEDULER.POLL_SECONDS", 60),
            SchedulerLote = GetInt(values, "OPERACIONES.BATCH.SCHEDULER.LOTE", 20),
            SchedulerReintentoErrorMinutos = GetInt(values, "OPERACIONES.BATCH.SCHEDULER.REINTENTO_ERROR_MINUTOS", 15),
            SchedulerQueueMode = Get(values, "OPERACIONES.BATCH.SCHEDULER.QUEUE_MODE") ?? "DATABASE",
            SpoolHabilitado = GetBool(values, "OPERACIONES.IMPRESION.SPOOL.HABILITADO", true),
            SpoolPollSeconds = GetInt(values, "OPERACIONES.IMPRESION.SPOOL.POLL_SECONDS", 15),
            SpoolLote = GetInt(values, "OPERACIONES.IMPRESION.SPOOL.LOTE", 10),
            SpoolReintentoMinutos = GetInt(values, "OPERACIONES.IMPRESION.SPOOL.REINTENTO_MINUTOS", 5),
            SpoolMaxIntentos = GetInt(values, "OPERACIONES.IMPRESION.SPOOL.MAX_INTENTOS", 5),
            SpoolBackoffFactor = GetInt(values, "OPERACIONES.IMPRESION.SPOOL.BACKOFF_FACTOR", 2),
            SpoolMaxRetryMinutes = GetInt(values, "OPERACIONES.IMPRESION.SPOOL.MAX_RETRY_MINUTES", 60),
            SpoolQueueMode = Get(values, "OPERACIONES.IMPRESION.SPOOL.QUEUE_MODE") ?? "DATABASE",
            ParsersHabilitados = GetBool(values, "OPERACIONES.IMPORTACION.PARSERS.HABILITADOS", true),
            LayoutLegacyProfile = Get(values, "OPERACIONES.IMPORTACION.LAYOUT_LEGACY_PROFILE") ?? "DEFAULT",
            PdfLayoutProfile = Get(values, "OPERACIONES.IMPRESION.PDF.LAYOUT_PROFILE") ?? "DEFAULT",
            ImpresionFiscalHabilitada = GetBool(values, "OPERACIONES.IMPRESION.FISCAL.HABILITADA", true),
            EpsonHabilitada = GetBool(values, "OPERACIONES.IMPRESION.FISCAL.EPSON.HABILITADA", true),
            HasarHabilitada = GetBool(values, "OPERACIONES.IMPRESION.FISCAL.HASAR.HABILITADA", true)
        };
    }

    private static int GetInt(IReadOnlyDictionary<string, string?> values, string key, int @default)
        => int.TryParse(Get(values, key), out var parsed) ? parsed : @default;

    private static bool GetBool(IReadOnlyDictionary<string, string?> values, string key, bool @default)
        => bool.TryParse(Get(values, key), out var parsed) ? parsed : @default;

    private static string? Get(IReadOnlyDictionary<string, string?> values, string key)
        => values.TryGetValue(key, out var value) ? value?.Trim() : null;
}
