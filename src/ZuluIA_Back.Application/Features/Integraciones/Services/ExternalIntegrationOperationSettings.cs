namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalIntegrationOperationSettings
{
    public required string Operation { get; init; }
    public required string Path { get; init; }
    public int TimeoutMs { get; init; }
    public int Reintentos { get; init; }
    public int CircuitThreshold { get; init; }
    public TimeSpan CircuitOpenFor { get; init; }
}
