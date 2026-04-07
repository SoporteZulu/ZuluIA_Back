namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class FiscalHardwareDiagnosticDto
{
    public string Marca { get; init; } = string.Empty;
    public bool HabilitadaPorConfiguracion { get; init; }
    public bool AdaptadorRegistrado { get; init; }
    public bool SamplePayloadOk { get; init; }
    public string? Mensaje { get; init; }
    public IReadOnlyList<string> Issues { get; init; } = [];
}
