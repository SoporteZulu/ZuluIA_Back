using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalIntegrationProviderSettings
{
    public required ProveedorIntegracionExterna Proveedor { get; init; }
    public required string Ambiente { get; init; }
    public required string Endpoint { get; init; }
    public bool Habilitada { get; init; }
    public int TimeoutMs { get; init; }
    public int Reintentos { get; init; }
    public int CircuitThreshold { get; init; }
    public TimeSpan CircuitOpenFor { get; init; }
    public string? ApiKey { get; init; }
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string? Token { get; init; }
    public string? CertificateAlias { get; init; }

    public bool HasAnyCredential =>
        !string.IsNullOrWhiteSpace(ApiKey)
        || !string.IsNullOrWhiteSpace(UserName)
        || !string.IsNullOrWhiteSpace(Password)
        || !string.IsNullOrWhiteSpace(Token)
        || !string.IsNullOrWhiteSpace(CertificateAlias);
}
