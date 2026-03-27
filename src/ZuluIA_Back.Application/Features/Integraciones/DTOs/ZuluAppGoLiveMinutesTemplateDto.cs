namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ZuluAppGoLiveMinutesTemplateDto
{
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<ZuluAppGoLiveMinutesSectionDto> Sections { get; init; } = [];
}

public class ZuluAppGoLiveMinutesSectionDto
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}
