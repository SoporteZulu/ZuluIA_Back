namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalIntegrationExecutionResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string ResponsePayload { get; init; } = string.Empty;
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }
    public bool IsFunctionalError { get; init; }
    public int RetryCount { get; init; }
    public bool CircuitOpen { get; init; }
    public string Ambiente { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
}
