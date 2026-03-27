namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ExternalFiscalPrecheckDto
{
    public string Proveedor { get; init; } = string.Empty;
    public string Operacion { get; init; } = string.Empty;
    public string ReferenciaTipo { get; init; } = string.Empty;
    public long ReferenciaId { get; init; }
    public bool Ready { get; init; }
    public string Ambiente { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public IReadOnlyList<string> Issues { get; init; } = [];
}
