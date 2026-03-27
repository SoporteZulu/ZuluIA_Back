namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ArchivoLayoutTemplateDto
{
    public string Circuito { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredColumns { get; init; } = [];
    public IReadOnlyDictionary<string, string> Aliases { get; init; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string?> SampleRow { get; init; } = new Dictionary<string, string?>();
}
