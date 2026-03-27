using FluentAssertions;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Infrastructure.Services;

namespace ZuluIA_Back.UnitTests.Infrastructure;

public class ParaguaySifenServiceTests
{
    [Fact]
    public async Task PrepararEnvioAsync_ConfiguracionCompleta_RetornaPreviewListo()
    {
        var config = Configuration([
            new KeyValuePair<string, string?>("Paraguay:Sifen:Enabled", "true"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:BaseUrl", "https://sifen.test/api"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiKey", "api-key"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiSecret", "api-secret"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:Environment", "test"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:TransportMode", "http")
        ]);

        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ => throw new InvalidOperationException("No deberia invocar HTTP en preview")));
        var sut = new ParaguaySifenService(httpClient, config, NullLogger<ParaguaySifenService>.Instance);

        var result = await sut.PrepararEnvioAsync(new(
            10,
            "80012345-6",
            "Sucursal PY",
            "Av. Paraguay 123",
            "1234567",
            "Cliente Paraguay",
            "Calle Cliente 456",
            "FCR",
            "Factura Contado",
            12,
            1,
            123,
            new DateOnly(2026, 3, 20),
            115,
            100,
            10,
            0,
            5,
            1,
            99,
            "12345678",
            "Observacion"));

        result.IntegracionHabilitada.Should().BeTrue();
        result.ListoParaEnviar.Should().BeTrue();
        result.Endpoint.Should().Be("https://sifen.test/api");
        result.Errores.Should().BeEmpty();
        result.Documento.RucEmisor.Should().Be("80012345-6");
        result.Documento.NroTimbrado.Should().Be("12345678");
    }

    [Fact]
    public async Task PrepararEnvioAsync_ConfiguracionIncompleta_RetornaErrores()
    {
        var config = Configuration([
            new KeyValuePair<string, string?>("Paraguay:Sifen:Enabled", "true"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:Environment", "test")
        ]);

        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ => throw new InvalidOperationException("No deberia invocar HTTP en preview")));
        var sut = new ParaguaySifenService(httpClient, config, NullLogger<ParaguaySifenService>.Instance);

        var result = await sut.PrepararEnvioAsync(new(
            10,
            "80012345-6",
            "Sucursal PY",
            null,
            "1234567",
            "Cliente Paraguay",
            null,
            "FCR",
            "Factura Contado",
            12,
            1,
            123,
            new DateOnly(2026, 3, 20),
            115,
            100,
            10,
            0,
            5,
            1,
            99,
            "12345678",
            null));

        result.ListoParaEnviar.Should().BeFalse();
        result.Errores.Should().Contain(x => x.Contains("BaseUrl", StringComparison.OrdinalIgnoreCase));
        result.Errores.Should().Contain(x => x.Contains("ApiKey", StringComparison.OrdinalIgnoreCase));
        result.Errores.Should().Contain(x => x.Contains("ApiSecret", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EnviarAsync_ModoStub_RetornaAceptadoConTrackingId()
    {
        var config = Configuration([
            new KeyValuePair<string, string?>("Paraguay:Sifen:Enabled", "true"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:BaseUrl", "https://sifen.test/api"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiKey", "api-key"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiSecret", "api-secret"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:Environment", "test"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:TransportMode", "stub")
        ]);

        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            }));

        var sut = new ParaguaySifenService(httpClient, config, NullLogger<ParaguaySifenService>.Instance);

        var result = await sut.EnviarAsync(BuildRequest());

        result.Aceptado.Should().BeTrue();
        result.Estado.Should().Be("stub-accepted");
        result.CodigoRespuesta.Should().BeNull();
        result.MensajeRespuesta.Should().BeNull();
        result.TrackingId.Should().Be("SIFEN-10");
    }

    [Fact]
    public async Task EnviarAsync_HttpOk_ParseaRespuestaJson()
    {
        var config = Configuration([
            new KeyValuePair<string, string?>("Paraguay:Sifen:Enabled", "true"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:BaseUrl", "https://sifen.test/api"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiKey", "api-key"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiSecret", "api-secret"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:Environment", "test"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:TransportMode", "http")
        ]);

        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"accepted\":true,\"status\":\"received\",\"trackingId\":\"abc-123\"}", Encoding.UTF8, "application/json")
            }));

        var sut = new ParaguaySifenService(httpClient, config, NullLogger<ParaguaySifenService>.Instance);

        var result = await sut.EnviarAsync(BuildRequest());

        result.Aceptado.Should().BeTrue();
        result.Estado.Should().Be("received");
        result.CodigoRespuesta.Should().BeNull();
        result.MensajeRespuesta.Should().BeNull();
        result.TrackingId.Should().Be("abc-123");
        result.Cdc.Should().BeNull();
        result.NumeroLote.Should().BeNull();
    }

    [Fact]
    public async Task EnviarAsync_HttpOk_ExtraeCdcYNumeroLoteSinPerderTracking()
    {
        var config = Configuration([
            new KeyValuePair<string, string?>("Paraguay:Sifen:Enabled", "true"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:BaseUrl", "https://sifen.test/api"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiKey", "api-key"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiSecret", "api-secret"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:Environment", "test"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:TransportMode", "http")
        ]);

        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"accepted\":true,\"status\":\"received\",\"trackingId\":\"abc-123\",\"cdc\":\"cdc-456\",\"numeroLote\":\"lote-789\"}", Encoding.UTF8, "application/json")
            }));

        var sut = new ParaguaySifenService(httpClient, config, NullLogger<ParaguaySifenService>.Instance);

        var result = await sut.EnviarAsync(BuildRequest());

        result.TrackingId.Should().Be("abc-123");
        result.Cdc.Should().Be("cdc-456");
        result.NumeroLote.Should().Be("lote-789");
    }

    [Fact]
    public async Task EnviarAsync_HttpRechazado_ExtraeCodigoYMensajeDesdeErrors()
    {
        var config = Configuration([
            new KeyValuePair<string, string?>("Paraguay:Sifen:Enabled", "true"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:BaseUrl", "https://sifen.test/api"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiKey", "api-key"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiSecret", "api-secret"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:Environment", "test"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:TransportMode", "http")
        ]);

        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"accepted\":false,\"status\":\"rejected\",\"errors\":[{\"code\":\"150\",\"message\":\"CDC invalido\"}],\"trackingId\":\"abc-123\"}", Encoding.UTF8, "application/json")
            }));

        var sut = new ParaguaySifenService(httpClient, config, NullLogger<ParaguaySifenService>.Instance);

        var result = await sut.EnviarAsync(BuildRequest());

        result.Aceptado.Should().BeFalse();
        result.CodigoRespuesta.Should().Be("150");
        result.MensajeRespuesta.Should().Be("CDC invalido");
    }

    [Fact]
    public async Task ConsultarEstadoAsync_HttpOk_UsaStatusUrlYParseaEstado()
    {
        var config = Configuration([
            new KeyValuePair<string, string?>("Paraguay:Sifen:Enabled", "true"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:BaseUrl", "https://sifen.test/api/envios"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:StatusUrl", "https://sifen.test/api/estado"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiKey", "api-key"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:ApiSecret", "api-secret"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:Environment", "test"),
            new KeyValuePair<string, string?>("Paraguay:Sifen:TransportMode", "http")
        ]);

        HttpRequestMessage? capturedRequest = null;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"accepted\":true,\"status\":\"approved\",\"trackingId\":\"abc-123\",\"cdc\":\"cdc-456\",\"numeroLote\":\"lote-789\"}", Encoding.UTF8, "application/json")
            };
        }));

        var sut = new ParaguaySifenService(httpClient, config, NullLogger<ParaguaySifenService>.Instance);

        var result = await sut.ConsultarEstadoAsync(new ConsultarEstadoSifenRequest(10, "abc-123", "cdc-456", "lote-789"));

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString().Should().Contain("https://sifen.test/api/estado");
        capturedRequest.RequestUri!.ToString().Should().Contain("trackingId=abc-123");
        result.Aceptado.Should().BeTrue();
        result.Estado.Should().Be("approved");
        result.CodigoRespuesta.Should().BeNull();
        result.MensajeRespuesta.Should().BeNull();
        result.TrackingId.Should().Be("abc-123");
        result.Cdc.Should().Be("cdc-456");
        result.NumeroLote.Should().Be("lote-789");
    }

    private static IConfiguration Configuration(IEnumerable<KeyValuePair<string, string?>> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static PrepararEnvioSifenRequest BuildRequest() => new(
        10,
        "80012345-6",
        "Sucursal PY",
        "Av. Paraguay 123",
        "1234567",
        "Cliente Paraguay",
        "Calle Cliente 456",
        "FCR",
        "Factura Contado",
        12,
        1,
        123,
        new DateOnly(2026, 3, 20),
        115,
        100,
        10,
        0,
        5,
        1,
        99,
        "12345678",
        "Observacion");

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }
}