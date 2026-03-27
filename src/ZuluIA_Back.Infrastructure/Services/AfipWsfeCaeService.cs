using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Infrastructure.Services;

public sealed class AfipWsfeCaeService(
    HttpClient httpClient,
    IConfiguration configuration,
    IAfipWsaaAuthService wsaaAuthService,
    ILogger<AfipWsfeCaeService> logger) : IAfipWsfeCaeService
{
    private const string SoapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string WsfeNamespace = "http://ar.gov.afip.dif.FEV1/";
    private const string SoapAction = "http://ar.gov.afip.dif.FEV1/FECAESolicitar";

    public async Task<SolicitarCaeAfipResponse> SolicitarCaeAsync(
        SolicitarCaeAfipRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = AfipWsfeOptions.From(configuration);
        var credentials = await ResolveCredentialsAsync(options, cancellationToken);
        ValidateConfiguration(options, credentials);

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
            var detail = ExtractErrors(responseContent);
            logger.LogWarning(
                "AFIP WSFE devolvio HTTP {StatusCode} al solicitar CAE. Detalle: {Detalle}",
                (int)response.StatusCode,
                detail);

            throw new InvalidOperationException(
                $"AFIP WSFE rechazo la solicitud de CAE con HTTP {(int)response.StatusCode}. {detail}".Trim());
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
        SolicitarCaeAfipRequest request,
        AfipWsfeOptions options,
        AfipWsaaCredentials credentials)
    {
        var auth = new XElement(XName.Get("Auth", WsfeNamespace),
            new XElement(XName.Get("Token", WsfeNamespace), credentials.Token),
            new XElement(XName.Get("Sign", WsfeNamespace), credentials.Sign),
            new XElement(XName.Get("Cuit", WsfeNamespace), options.Cuit));

        var detailRequest = new XElement(XName.Get("FECAEDetRequest", WsfeNamespace),
            new XElement(XName.Get("Concepto", WsfeNamespace), request.Concepto),
            new XElement(XName.Get("DocTipo", WsfeNamespace), request.TipoDocumento),
            new XElement(XName.Get("DocNro", WsfeNamespace), request.NumeroDocumento),
            new XElement(XName.Get("CbteDesde", WsfeNamespace), request.NumeroComprobante),
            new XElement(XName.Get("CbteHasta", WsfeNamespace), request.NumeroComprobante),
            new XElement(XName.Get("CbteFch", WsfeNamespace), request.FechaEmision.ToString("yyyyMMdd", CultureInfo.InvariantCulture)),
            new XElement(XName.Get("ImpTotal", WsfeNamespace), FormatDecimal(request.ImporteTotal)),
            new XElement(XName.Get("ImpTotConc", WsfeNamespace), FormatDecimal(request.ImporteNoGravado)),
            new XElement(XName.Get("ImpNeto", WsfeNamespace), FormatDecimal(request.ImporteNeto)),
            new XElement(XName.Get("ImpOpEx", WsfeNamespace), FormatDecimal(request.ImporteExento)),
            new XElement(XName.Get("ImpTrib", WsfeNamespace), FormatDecimal(request.ImporteTributos)),
            new XElement(XName.Get("ImpIVA", WsfeNamespace), FormatDecimal(request.ImporteIva)),
            new XElement(XName.Get("MonId", WsfeNamespace), request.MonedaCodigo),
            new XElement(XName.Get("MonCotiz", WsfeNamespace), FormatDecimal(request.MonedaCotizacion)));

        if (request.Alicuotas.Count > 0)
        {
            detailRequest.Add(new XElement(XName.Get("Iva", WsfeNamespace),
                request.Alicuotas.Select(alicuota =>
                    new XElement(XName.Get("AlicIva", WsfeNamespace),
                        new XElement(XName.Get("Id", WsfeNamespace), alicuota.Id),
                        new XElement(XName.Get("BaseImp", WsfeNamespace), FormatDecimal(alicuota.BaseImponible)),
                        new XElement(XName.Get("Importe", WsfeNamespace), FormatDecimal(alicuota.Importe))))));
        }

        if (request.Tributos.Count > 0)
        {
            detailRequest.Add(new XElement(XName.Get("Tributos", WsfeNamespace),
                request.Tributos.Select(tributo =>
                    new XElement(XName.Get("Tributo", WsfeNamespace),
                        new XElement(XName.Get("Id", WsfeNamespace), tributo.Id),
                        new XElement(XName.Get("Desc", WsfeNamespace), tributo.Descripcion),
                        new XElement(XName.Get("BaseImp", WsfeNamespace), FormatDecimal(tributo.BaseImponible)),
                        new XElement(XName.Get("Alic", WsfeNamespace), FormatDecimal(tributo.Alicuota)),
                        new XElement(XName.Get("Importe", WsfeNamespace), FormatDecimal(tributo.Importe))))));
        }

        var body = new XElement(XName.Get("FECAESolicitar", WsfeNamespace),
            auth,
            new XElement(XName.Get("FeCAEReq", WsfeNamespace),
                new XElement(XName.Get("FeCabReq", WsfeNamespace),
                    new XElement(XName.Get("CantReg", WsfeNamespace), 1),
                    new XElement(XName.Get("PtoVta", WsfeNamespace), request.PuntoVenta),
                    new XElement(XName.Get("CbteTipo", WsfeNamespace), request.TipoComprobante)),
                new XElement(XName.Get("FeDetReq", WsfeNamespace), detailRequest)));

        var doc = new XDocument(
            new XElement(XName.Get("Envelope", SoapNamespace),
                new XAttribute(XNamespace.Xmlns + "soap", SoapNamespace),
                new XElement(XName.Get("Body", SoapNamespace), body)));

        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static SolicitarCaeAfipResponse ParseResponse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var cae = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "CAE")?.Value?.Trim();
        var caeDue = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "CAEFchVto")?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(cae) || string.IsNullOrWhiteSpace(caeDue))
        {
            var detail = ExtractErrors(xml);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(detail)
                    ? "AFIP WSFE no devolvio CAE o fecha de vencimiento."
                    : $"AFIP WSFE no devolvio un CAE valido. {detail}");
        }

        if (!DateOnly.TryParseExact(caeDue[..8], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
            throw new InvalidOperationException("AFIP WSFE devolvio una fecha de vencimiento de CAE invalida.");

        return new SolicitarCaeAfipResponse(cae, dueDate);
    }

    private static string FormatDecimal(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string ExtractErrors(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);

            var errors = doc.Descendants()
                .Where(x => x.Name.LocalName is "Err" or "Obs")
                .Select(x =>
                {
                    var code = x.Elements().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value?.Trim();
                    var message = x.Elements().FirstOrDefault(e => e.Name.LocalName is "Msg" or "Message")?.Value?.Trim();
                    return string.IsNullOrWhiteSpace(code) ? message : $"{code}: {message}";
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return errors.Count == 0 ? string.Empty : string.Join(" | ", errors);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void ValidateConfiguration(AfipWsfeOptions options, AfipWsaaCredentials credentials)
    {
        if (!options.Enabled)
            throw new InvalidOperationException("La integracion AFIP WSFE CAE no esta habilitada.");

        if (string.IsNullOrWhiteSpace(options.BaseUrl) ||
            string.IsNullOrWhiteSpace(options.Cuit) ||
            string.IsNullOrWhiteSpace(credentials.Token) ||
            string.IsNullOrWhiteSpace(credentials.Sign))
        {
            throw new InvalidOperationException(
                "La configuracion AFIP WSFE CAE es incompleta. Se requiere BaseUrl, Cuit, Token y Sign.");
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