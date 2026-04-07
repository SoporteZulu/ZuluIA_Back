namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class OperacionesBatchSettings
{
    public bool SchedulerHabilitado { get; init; }
    public int SchedulerPollSeconds { get; init; }
    public int SchedulerLote { get; init; }
    public int SchedulerReintentoErrorMinutos { get; init; }
    public string SchedulerQueueMode { get; init; } = "DATABASE";
    public bool SpoolHabilitado { get; init; }
    public int SpoolPollSeconds { get; init; }
    public int SpoolLote { get; init; }
    public int SpoolReintentoMinutos { get; init; }
    public int SpoolMaxIntentos { get; init; }
    public int SpoolBackoffFactor { get; init; }
    public int SpoolMaxRetryMinutes { get; init; }
    public string SpoolQueueMode { get; init; } = "DATABASE";
    public bool ParsersHabilitados { get; init; }
    public string LayoutLegacyProfile { get; init; } = "DEFAULT";
    public string PdfLayoutProfile { get; init; } = "DEFAULT";
    public bool ImpresionFiscalHabilitada { get; init; }
    public bool EpsonHabilitada { get; init; }
    public bool HasarHabilitada { get; init; }
}
