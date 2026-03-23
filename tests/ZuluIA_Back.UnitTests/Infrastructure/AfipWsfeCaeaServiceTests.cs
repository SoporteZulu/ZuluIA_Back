using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Infrastructure.Services;

namespace ZuluIA_Back.UnitTests.Infrastructure;

public class AfipWsfeCaeaServiceTests
{
    [Fact]
    public async Task SolicitarCaeaAsync_ConWsaa_UsaCredencialesDelServicio()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseSoap("12345678901234"), Encoding.UTF8, "text/xml")
            });

        using var httpClient = new HttpClient(handler);
        var wsaa = Substitute.For<IAfipWsaaAuthService>();
        wsaa.GetWsfeCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(new AfipWsaaCredentials("token-wsaa", "sign-wsaa"));

        var config = Configuration([
            new KeyValuePair<string, string?>("Afip:Wsfe:Enabled", "true"),
            new KeyValuePair<string, string?>("Afip:Wsfe:UseWsaa", "true"),
            new KeyValuePair<string, string?>("Afip:Wsfe:Cuit", "20123456789"),
            new KeyValuePair<string, string?>("Afip:Wsfe:BaseUrl", "https://example.test/wsfe")
        ]);

        var sut = new AfipWsfeCaeaService(httpClient, config, wsaa, NullLogger<AfipWsfeCaeaService>.Instance);

        var result = await sut.SolicitarCaeaAsync(new SolicitarCaeaAfipRequest(202603, 1));

        result.NroCaea.Should().Be("12345678901234");
        handler.LastRequestBody.Should().Contain("token-wsaa");
        handler.LastRequestBody.Should().Contain("sign-wsaa");
        await wsaa.Received(1).GetWsfeCredentialsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SolicitarCaeaAsync_ConCredencialesManuales_NoInvocaWsaa()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseSoap("99999999999999"), Encoding.UTF8, "text/xml")
            });

        using var httpClient = new HttpClient(handler);
        var wsaa = Substitute.For<IAfipWsaaAuthService>();
        var config = Configuration([
            new KeyValuePair<string, string?>("Afip:Wsfe:Enabled", "true"),
            new KeyValuePair<string, string?>("Afip:Wsfe:UseWsaa", "true"),
            new KeyValuePair<string, string?>("Afip:Wsfe:Cuit", "20123456789"),
            new KeyValuePair<string, string?>("Afip:Wsfe:Token", "token-manual"),
            new KeyValuePair<string, string?>("Afip:Wsfe:Sign", "sign-manual"),
            new KeyValuePair<string, string?>("Afip:Wsfe:BaseUrl", "https://example.test/wsfe")
        ]);

        var sut = new AfipWsfeCaeaService(httpClient, config, wsaa, NullLogger<AfipWsfeCaeaService>.Instance);

        var result = await sut.SolicitarCaeaAsync(new SolicitarCaeaAfipRequest(202603, 2));

        result.NroCaea.Should().Be("99999999999999");
        handler.LastRequestBody.Should().Contain("token-manual");
        handler.LastRequestBody.Should().Contain("sign-manual");
        await wsaa.DidNotReceive().GetWsfeCredentialsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SolicitarCaeaAsync_ConCredencialesManualesVencidas_FallaAntesDeInvocarWsaaONet()
    {
        var handler = new StubHttpMessageHandler(_ => throw new InvalidOperationException("No deberia invocar HTTP"));

        using var httpClient = new HttpClient(handler);
        var wsaa = Substitute.For<IAfipWsaaAuthService>();
        var config = Configuration([
            new KeyValuePair<string, string?>("Afip:Wsfe:Enabled", "true"),
            new KeyValuePair<string, string?>("Afip:Wsfe:Cuit", "20123456789"),
            new KeyValuePair<string, string?>("Afip:Wsfe:Token", "token-manual"),
            new KeyValuePair<string, string?>("Afip:Wsfe:Sign", "sign-manual"),
            new KeyValuePair<string, string?>("Afip:Wsfe:TokenExpiration", DateTimeOffset.UtcNow.AddMinutes(-5).ToString("O")),
            new KeyValuePair<string, string?>("Afip:Wsfe:BaseUrl", "https://example.test/wsfe")
        ]);

        var sut = new AfipWsfeCaeaService(httpClient, config, wsaa, NullLogger<AfipWsfeCaeaService>.Instance);

        var action = () => sut.SolicitarCaeaAsync(new SolicitarCaeaAfipRequest(202603, 2));

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*vencidas*");
        await wsaa.DidNotReceive().GetWsfeCredentialsAsync(Arg.Any<CancellationToken>());
        handler.LastRequestBody.Should().BeEmpty();
    }

    private static IConfiguration Configuration(IEnumerable<KeyValuePair<string, string?>> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static string ResponseSoap(string caea) =>
        $$"""
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <FECAEASolicitarResponse xmlns="http://ar.gov.afip.dif.FEV1/">
              <FECAEASolicitarResult>
                <ResultGet>
                  <CAEA>{{caea}}</CAEA>
                  <FchProceso>20260319</FchProceso>
                  <FchTopeInf>20260331</FchTopeInf>
                </ResultGet>
              </FECAEASolicitarResult>
            </FECAEASolicitarResponse>
          </soap:Body>
        </soap:Envelope>
        """;

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public string LastRequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return responder(request);
        }
    }
}