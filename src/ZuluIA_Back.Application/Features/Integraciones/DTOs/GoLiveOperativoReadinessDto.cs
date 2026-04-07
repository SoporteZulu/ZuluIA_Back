namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class GoLiveOperativoReadinessDto
{
    public bool SchedulerReadyProduction { get; init; }
    public bool SpoolReadyProduction { get; init; }
    public bool ParserReadyProduction { get; init; }
    public bool PdfReadyProduction { get; init; }
    public bool FiscalHardwareReadyProduction { get; init; }
    public bool ReadyForGoLive { get; init; }
    public string SchedulerQueueMode { get; init; } = string.Empty;
    public string SpoolQueueMode { get; init; } = string.Empty;
    public string LayoutLegacyProfile { get; init; } = string.Empty;
    public string PdfLayoutProfile { get; init; } = string.Empty;
    public IReadOnlyList<string> SupportedParserFormats { get; init; } = [];
    public IReadOnlyList<string> RegisteredFiscalAdapters { get; init; } = [];
    public int ProgramacionesActivas { get; init; }
    public int ProgramacionesVencidas { get; init; }
    public int SpoolPendiente { get; init; }
    public int SpoolConError { get; init; }
    public IReadOnlyList<string> Issues { get; init; } = [];
}
