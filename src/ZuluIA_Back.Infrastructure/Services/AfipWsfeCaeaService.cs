using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Infrastructure.Services;

public sealed class AfipWsfeCaeaService(
    HttpClient httpClient,
    IConfiguration configuration,
    IAfipWsaaAuthService wsaaAuthService,
    ILogger<AfipWsfeCaeaService> logger) : IAfipWsfeCaeaService
{
    private const string SoapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string WsfeNamespace = "http://ar.gov.afip.dif.FEV1/";
    private const string SoapAction = "http://ar.gov.afip.dif.FEV1/FECAEASolicitar";

    public async Task<SolicitarCaeaAfipResponse> SolicitarCaeaAsync(
        SolicitarCaeaAfipRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = AfipWsfeOptions.From(configuration);
        var credentials = await ResolveCredentialsAsync(options, cancellationToken);
        ValidarConfiguracion(options, credentials);

        var envelope = BuildEnvelope(request, options, credentials);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, options.BaseUrl)
        {
            Content = new StringContent(envelope, Encoding.UTF8, "text/xml")
        };

        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        httpRequest.Headers.Add("SOAPAction", $"\"{SoapAction}\"");

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detalle = ExtractErrors(responseContent);
            logger.LogWarning(
                "AFIP WSFE devolvio estado HTTP {StatusCode}. Detalle: {Detalle}",
                (int)response.StatusCode,
                detalle);

            throw new InvalidOperationException(
                $"AFIP WSFE rechazo la solicitud de CAEA con HTTP {(int)response.StatusCode}. {detalle}".Trim());
        }

        return ParseResponse(responseContent);
    }

    private async Task<AfipWsaaCredentials> ResolveCredentialsAsync(
        AfipWsfeOptions options,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.Token) && !string.IsNullOrWhiteSpace(options.Sign))
            return new AfipWsaaCredentials(options.Token, options.Sign, options.TokenExpiration);

        if (options.UseWsaa)
            return await wsaaAuthService.GetWsfeCredentialsAsync(cancellationToken);

        throw new InvalidOperationException(
            "No hay credenciales AFIP WSFE configuradas. Configure Token/Sign o habilite WSAA.");
    }

    private static string BuildEnvelope(
        SolicitarCaeaAfipRequest request,
        AfipWsfeOptions options,
        AfipWsaaCredentials credentials)
    {
        var auth = new XElement(XName.Get("Auth", WsfeNamespace),
            new XElement(XName.Get("Token", WsfeNamespace), credentials.Token),
            new XElement(XName.Get("Sign", WsfeNamespace), credentials.Sign),
            new XElement(XName.Get("Cuit", WsfeNamespace), options.Cuit));

        var body = new XElement(XName.Get("FECAEASolicitar", WsfeNamespace),
            auth,
            new XElement(XName.Get("Periodo", WsfeNamespace), request.Periodo),
            new XElement(XName.Get("Orden", WsfeNamespace), request.Orden));

        var doc = new XDocument(
            new XElement(XName.Get("Envelope", SoapNamespace),
                new XAttribute(XNamespace.Xmlns + "soap", SoapNamespace),
                new XElement(XName.Get("Body", SoapNamespace),
                    body)));

        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static SolicitarCaeaAfipResponse ParseResponse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var caea = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "CAEA")?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(caea))
        {
            var detalle = ExtractErrors(xml);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(detalle)
                    ? "AFIP WSFE no devolvio un CAEA en la respuesta."
                    : $"AFIP WSFE no devolvio un CAEA valido. {detalle}");
        }

        var fechaProceso = ParseAfipDate(doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "FchProceso")?.Value);
        var fechaTope = ParseAfipDate(doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "FchTopeInf")?.Value);

        return new SolicitarCaeaAfipResponse(caea, fechaProceso, fechaTope);
    }

    private static DateOnly? ParseAfipDate(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        var value = rawValue.Trim();
        if (value.Length >= 8)
            value = value[..8];

        return DateOnly.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static string ExtractErrors(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);

            var errores = doc.Descendants()
                .Where(x => x.Name.LocalName is "Err" or "Obs")
                .Select(x =>
                {
                    var code = x.Elements().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value?.Trim();
                    var message = x.Elements().FirstOrDefault(e => e.Name.LocalName is "Msg" or "Message")?.Value?.Trim();

                    return string.IsNullOrWhiteSpace(code)
                        ? message
                        : $"{code}: {message}";
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return errores.Count == 0 ? string.Empty : string.Join(" | ", errores);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void ValidarConfiguracion(AfipWsfeOptions options, AfipWsaaCredentials credentials)
    {
        if (!options.Enabled)
            throw new InvalidOperationException("La integracion AFIP WSFE CAEA no esta habilitada.");

        if (string.IsNullOrWhiteSpace(options.BaseUrl) ||
            string.IsNullOrWhiteSpace(credentials.Token) ||
            string.IsNullOrWhiteSpace(credentials.Sign) ||
            string.IsNullOrWhiteSpace(options.Cuit))
        {
            throw new InvalidOperationException(
                "La configuracion AFIP WSFE CAEA es incompleta. Se requiere BaseUrl, Token, Sign y Cuit.");
        }

        if (credentials.ExpirationTime.HasValue && credentials.ExpirationTime.Value <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Las credenciales AFIP WSFE configuradas estan vencidas.");
    }

    private sealed record AfipWsfeOptions(
        bool Enabled,
        string BaseUrl,
        string Token,
        string Sign,
        string Cuit,
        bool UseWsaa,
        DateTimeOffset? TokenExpiration)
    {
        public static AfipWsfeOptions From(IConfiguration configuration)
        {
            var section = configuration.GetSection("Afip:Wsfe");

            var enabled = bool.TryParse(Environment.GetEnvironmentVariable("AFIP_WSFE_ENABLED"), out var envEnabled)
                ? envEnabled
                : section.GetValue<bool>("Enabled");

            var baseUrl = Environment.GetEnvironmentVariable("AFIP_WSFE_BASE_URL")
                ?? section["BaseUrl"]
                ?? "https://wswhomo.afip.gov.ar/wsfev1/service.asmx";

            var token = Environment.GetEnvironmentVariable("AFIP_WSFE_TOKEN")
                ?? section["Token"]
                ?? string.Empty;

            var sign = Environment.GetEnvironmentVariable("AFIP_WSFE_SIGN")
                ?? section["Sign"]
                ?? string.Empty;

            var cuit = Environment.GetEnvironmentVariable("AFIP_WSFE_CUIT")
                ?? section["Cuit"]
                ?? string.Empty;

            var useWsaa = bool.TryParse(Environment.GetEnvironmentVariable("AFIP_WSFE_USE_WSAA"), out var envUseWsaa)
                ? envUseWsaa
                : section.GetValue<bool>("UseWsaa");

            var tokenExpirationRaw = Environment.GetEnvironmentVariable("AFIP_WSFE_TOKEN_EXPIRATION")
                ?? section["TokenExpiration"];

            DateTimeOffset? tokenExpiration = DateTimeOffset.TryParse(tokenExpirationRaw, out var parsedTokenExpiration)
                ? parsedTokenExpiration
                : null;

            return new AfipWsfeOptions(enabled, baseUrl, token, sign, cuit, useWsaa, tokenExpiration);
        }
    }
}