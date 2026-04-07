using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Infrastructure.Services;

namespace ZuluIA_Back.UnitTests.Infrastructure;

public class CachedAfipWsaaAuthServiceTests
{
    [Fact]
    public async Task GetWsfeCredentialsAsync_ReutilizaCredencialesCacheadas()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var innerService = Substitute.For<AfipWsaaAuthService>(new HttpClient(), Configuration(), NullLogger<AfipWsaaAuthService>.Instance);
        innerService.GetWsfeCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(new AfipWsaaCredentials("token-1", "sign-1", DateTimeOffset.UtcNow.AddMinutes(30)));

        var sut = new CachedAfipWsaaAuthService(innerService, memoryCache, Configuration());

        var first = await sut.GetWsfeCredentialsAsync();
        var second = await sut.GetWsfeCredentialsAsync();

        first.Token.Should().Be("token-1");
        second.Token.Should().Be("token-1");
        await innerService.Received(1).GetWsfeCredentialsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetWsfeCredentialsAsync_SiLaCredencialExpiraPronto_Renueva()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var innerService = Substitute.For<AfipWsaaAuthService>(new HttpClient(), Configuration(refreshSkewMinutes: 5), NullLogger<AfipWsaaAuthService>.Instance);
        innerService.GetWsfeCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(
                new AfipWsaaCredentials("token-1", "sign-1", DateTimeOffset.UtcNow.AddMinutes(2)),
                new AfipWsaaCredentials("token-2", "sign-2", DateTimeOffset.UtcNow.AddMinutes(30)));

        var sut = new CachedAfipWsaaAuthService(innerService, memoryCache, Configuration(refreshSkewMinutes: 5));

        var first = await sut.GetWsfeCredentialsAsync();
        var second = await sut.GetWsfeCredentialsAsync();

        first.Token.Should().Be("token-1");
        second.Token.Should().Be("token-2");
        await innerService.Received(2).GetWsfeCredentialsAsync(Arg.Any<CancellationToken>());
    }

    private static IConfiguration Configuration(int cacheMinutes = 60, int refreshSkewMinutes = 5) =>
        new ConfigurationBuilder().AddInMemoryCollection([
            new KeyValuePair<string, string?>("Afip:Wsaa:CacheMinutes", cacheMinutes.ToString()),
            new KeyValuePair<string, string?>("Afip:Wsaa:RefreshSkewMinutes", refreshSkewMinutes.ToString())
        ]).Build();
}