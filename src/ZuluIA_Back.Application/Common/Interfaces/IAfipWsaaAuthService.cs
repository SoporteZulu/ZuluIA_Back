namespace ZuluIA_Back.Application.Common.Interfaces;

public sealed record AfipWsaaCredentials(
    string Token,
    string Sign,
    DateTimeOffset? ExpirationTime = null);

public interface IAfipWsaaAuthService
{
    Task<AfipWsaaCredentials> GetWsfeCredentialsAsync(CancellationToken cancellationToken = default);
}