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

public class AfipWsfeCaeServiceTests
{
    [Fact]
    public async Task SolicitarCaeAsync_ConWsaa_UsaCredencialesYParseaRespuesta()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseSoap("12345678901234", "20260430"), Encoding.UTF8, "text/xml")
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

        var sut = new AfipWsfeCaeService(httpClient, config, wsaa, NullLogger<AfipWsfeCaeService>.Instance);

        var result = await sut.SolicitarCaeAsync(new SolicitarCaeAfipRequest(
            1, 1, 25, new DateOnly(2026, 4, 1), 1, 80, 20123456789, 1210m, 1000m, 0m, 0m, 0m, 210m, "PES", 1m,
          [new AfipWsfeCaeAlicuotaRequest(5, 1000m, 210m)],
          [new AfipWsfeCaeTributoRequest(99, "Percepciones", 1000m, 3.5m, 35m)]));

        result.Cae.Should().Be("12345678901234");
        result.FechaVencimiento.Should().Be(new DateOnly(2026, 4, 30));
        handler.LastRequestBody.Should().Contain("FECAESolicitar");
        handler.LastRequestBody.Should().Contain("token-wsaa");
        handler.LastRequestBody.Should().Contain("Tributos");
        handler.LastRequestBody.Should().Contain("Percepciones");
        handler.LastRequestBody.Should().NotContain("12345678901234");
    }

      [Fact]
      public async Task SolicitarCaeAsync_ConCredencialesManualesVencidas_FallaAntesDeInvocarWsaaONet()
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

        var sut = new AfipWsfeCaeService(httpClient, config, wsaa, NullLogger<AfipWsfeCaeService>.Instance);

        var action = () => sut.SolicitarCaeAsync(new SolicitarCaeAfipRequest(
          1, 1, 25, new DateOnly(2026, 4, 1), 1, 80, 20123456789, 1210m, 1000m, 0m, 0m, 0m, 210m, "PES", 1m,
          [new AfipWsfeCaeAlicuotaRequest(5, 1000m, 210m)],
          []));

        await action.Should().ThrowAsync<InvalidOperationException>()
          .WithMessage("*vencidas*");
        await wsaa.DidNotReceive().GetWsfeCredentialsAsync(Arg.Any<CancellationToken>());
        handler.LastRequestBody.Should().BeEmpty();
      }

    private static IConfiguration Configuration(IEnumerable<KeyValuePair<string, string?>> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static string ResponseSoap(string cae, string fechaVto) =>
        $$"""
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <FECAESolicitarResponse xmlns="http://ar.gov.afip.dif.FEV1/">
              <FECAESolicitarResult>
                <FeDetResp>
                  <FECAEDetResponse>
                    <CAE>{{cae}}</CAE>
                    <CAEFchVto>{{fechaVto}}</CAEFchVto>
                  </FECAEDetResponse>
                </FeDetResp>
              </FECAESolicitarResult>
            </FECAESolicitarResponse>
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