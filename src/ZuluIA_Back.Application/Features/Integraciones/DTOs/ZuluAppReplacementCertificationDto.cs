namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ZuluAppReplacementCertificationDto
{
    public bool SmokeReady { get; init; }
    public bool LegacyCompatibilityReady { get; init; }
    public bool SecurityBaselineReady { get; init; }
    public bool OperationalGoLiveReady { get; init; }
    public bool ReplacementReady { get; init; }
    public IReadOnlyDictionary<string, long> CoreCounts { get; init; } = new Dictionary<string, long>();
    public IReadOnlyList<string> SupportedLegacyCircuits { get; init; } = [];
    public IReadOnlyList<ZuluAppModuleSmokeDto> ModuleSmoke { get; init; } = [];
    public IReadOnlyList<ZuluAppModuleParityDto> ModuleParity { get; init; } = [];
    public IReadOnlyList<ZuluAppReplacementEvidenceDto> EvidenceChecklist { get; init; } = [];
    public IReadOnlyList<ZuluAppReplacementChecklistItemDto> Checklist { get; init; } = [];
    public IReadOnlyList<ZuluAppReplacementGapDto> Gaps { get; init; } = [];
    public IReadOnlyList<string> RecommendedActions { get; init; } = [];
}

public class ZuluAppReplacementEvidenceDto
{
    public string Area { get; init; } = string.Empty;
    public string Evidence { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
}

public class ZuluAppModuleSmokeDto
{
    public string Module { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
}

public class ZuluAppModuleParityDto
{
    public string Module { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
}

public class ZuluAppReplacementChecklistItemDto
{
    public string Area { get; init; } = string.Empty;
    public string Item { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
}

public class ZuluAppReplacementGapDto
{
    public string Severity { get; init; } = string.Empty;
    public string Area { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
