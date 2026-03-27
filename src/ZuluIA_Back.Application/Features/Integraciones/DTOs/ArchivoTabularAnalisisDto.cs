namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ArchivoTabularAnalisisDto
{
    public string FileName { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public string LayoutProfile { get; init; } = string.Empty;
    public string? Separator { get; init; }
    public int TotalRows { get; init; }
    public int PreviewRows { get; init; }
    public IReadOnlyList<string> Columns { get; init; } = [];
    public IReadOnlyList<IReadOnlyDictionary<string, string?>> Rows { get; init; } = [];
    public IReadOnlyList<string> SupportedFormats { get; init; } = [];
}
