namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalProviderFunctionalException(string message, string? code = null) : InvalidOperationException(message)
{
    public string? Code { get; } = code?.Trim().ToUpperInvariant();
}
