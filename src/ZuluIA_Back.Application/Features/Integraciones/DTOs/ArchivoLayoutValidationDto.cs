namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ArchivoLayoutValidationDto
{
    public string Circuito { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string LayoutProfile { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public IReadOnlyList<string> RequiredColumns { get; init; } = [];
    public IReadOnlyList<string> MissingColumns { get; init; } = [];
    public IReadOnlyList<string> ExtraColumns { get; init; } = [];
    public IReadOnlyList<string> Issues { get; init; } = [];
}
