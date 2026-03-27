namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ZuluAppCutoverPlanDto
{
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<ZuluAppCutoverPlanStepDto> Preconditions { get; init; } = [];
    public IReadOnlyList<ZuluAppCutoverPlanStepDto> CutoverSteps { get; init; } = [];
    public IReadOnlyList<ZuluAppCutoverPlanStepDto> FallbackSteps { get; init; } = [];
    public IReadOnlyList<ZuluAppCutoverPlanStepDto> RollbackSteps { get; init; } = [];
}

public class ZuluAppCutoverPlanStepDto
{
    public string Phase { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Step { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}
