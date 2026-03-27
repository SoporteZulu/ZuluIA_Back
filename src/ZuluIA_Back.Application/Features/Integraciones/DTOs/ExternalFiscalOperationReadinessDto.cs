namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public class ExternalFiscalOperationReadinessDto
{
    public string Proveedor { get; init; } = string.Empty;
    public string Operacion { get; init; } = string.Empty;
    public string ReferenciaTipo { get; init; } = string.Empty;
    public long ReferenciaId { get; init; }
    public string Ambiente { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public bool Habilitada { get; init; }
    public bool UsaTransporteReal { get; init; }
    public bool CredencialesCompletas { get; init; }
    public bool ConfiguracionValida { get; init; }
    public bool PrecheckOk { get; init; }
    public bool ConectividadOk { get; init; }
    public bool ReadyToExecute { get; init; }
    public bool ReadyForProduction { get; init; }
    public int TimeoutMs { get; init; }
    public int Reintentos { get; init; }
    public int CircuitThreshold { get; init; }
    public int CircuitOpenSeconds { get; init; }
    public int? HttpStatusCode { get; init; }
    public long DuracionMs { get; init; }
    public string? Mensaje { get; init; }
    public IReadOnlyList<string> Issues { get; init; } = [];
}
