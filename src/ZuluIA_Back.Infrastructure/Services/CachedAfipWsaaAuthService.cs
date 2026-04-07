using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Infrastructure.Services;

public sealed class CachedAfipWsaaAuthService(
    AfipWsaaAuthService innerService,
    IMemoryCache memoryCache,
    IConfiguration configuration) : IAfipWsaaAuthService
{
    private const string CacheKey = "afip:wsaa:wsfe:credentials";

    public async Task<AfipWsaaCredentials> GetWsfeCredentialsAsync(CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue<AfipWsaaCredentials>(CacheKey, out var cachedCredentials) &&
            cachedCredentials is not null &&
            !ShouldRefresh(cachedCredentials))
        {
            return cachedCredentials;
        }

        var freshCredentials = await innerService.GetWsfeCredentialsAsync(cancellationToken);
        var options = CacheOptions.From(configuration);
        var expiration = GetCacheExpiration(freshCredentials, options);

        memoryCache.Set(CacheKey, freshCredentials, expiration);
        return freshCredentials;
    }

    private bool ShouldRefresh(AfipWsaaCredentials credentials)
    {
        if (credentials.ExpirationTime is null)
            return false;

        var options = CacheOptions.From(configuration);
        return credentials.ExpirationTime.Value <= DateTimeOffset.UtcNow.AddMinutes(options.RefreshSkewMinutes);
    }

    private static DateTimeOffset GetCacheExpiration(AfipWsaaCredentials credentials, CacheOptions options)
    {
        var fallbackExpiration = DateTimeOffset.UtcNow.AddMinutes(options.CacheMinutes);

        if (credentials.ExpirationTime is null)
            return fallbackExpiration;

        var adjustedExpiration = credentials.ExpirationTime.Value.AddMinutes(-Math.Abs(options.RefreshSkewMinutes));
        return adjustedExpiration > DateTimeOffset.UtcNow ? adjustedExpiration : DateTimeOffset.UtcNow.AddSeconds(1);
    }

    private sealed record CacheOptions(int CacheMinutes, int RefreshSkewMinutes)
    {
        public static CacheOptions From(IConfiguration configuration)
        {
            var section = configuration.GetSection("Afip:Wsaa");

            var cacheMinutes = int.TryParse(Environment.GetEnvironmentVariable("AFIP_WSAA_CACHE_MINUTES"), out var envCacheMinutes)
                ? envCacheMinutes
                : section.GetValue("CacheMinutes", 60);

            var refreshSkewMinutes = int.TryParse(Environment.GetEnvironmentVariable("AFIP_WSAA_REFRESH_SKEW_MINUTES"), out var envRefreshSkewMinutes)
                ? envRefreshSkewMinutes
                : section.GetValue("RefreshSkewMinutes", 5);

            return new CacheOptions(Math.Max(1, cacheMinutes), Math.Max(0, refreshSkewMinutes));
        }
    }
}