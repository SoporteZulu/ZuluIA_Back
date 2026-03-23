using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Infrastructure.Services;

public class AfipWsaaAuthService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<AfipWsaaAuthService> logger) : IAfipWsaaAuthService
{
    private const string SoapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

    public virtual async Task<AfipWsaaCredentials> GetWsfeCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var options = AfipWsaaOptions.From(configuration);
        ValidateOptions(options);

        var cms = BuildCms(options);
        var envelope = BuildEnvelope(cms);

        using var request = new HttpRequestMessage(HttpMethod.Post, options.BaseUrl)
        {
            Content = new StringContent(envelope, Encoding.UTF8, "text/xml")
        };

        request.Headers.Add("SOAPAction", "\"\"");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var xml = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("WSAA devolvio HTTP {StatusCode}: {Body}", (int)response.StatusCode, xml);
            throw new InvalidOperationException($"WSAA rechazo la autenticacion con HTTP {(int)response.StatusCode}.");
        }

        return ParseCredentials(xml);
    }

    private static string BuildCms(AfipWsaaOptions options)
    {
        var tra = BuildTra(options.Service, options.GenerationOffsetMinutes, options.ExpirationMinutes);
        var cert = new X509Certificate2(
            options.CertificatePath,
            options.CertificatePassword,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

        var contentInfo = new ContentInfo(Encoding.UTF8.GetBytes(tra));
        var signedCms = new SignedCms(contentInfo);
        var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert)
        {
            IncludeOption = X509IncludeOption.EndCertOnly
        };

        signedCms.ComputeSignature(signer);
        return Convert.ToBase64String(signedCms.Encode());
    }

    private static string BuildEnvelope(string cmsBase64)
    {
        var doc = new XDocument(
            new XElement(XName.Get("Envelope", SoapNamespace),
                new XAttribute(XNamespace.Xmlns + "soapenv", SoapNamespace),
                new XElement(XName.Get("Body", SoapNamespace),
                    new XElement("loginCms",
                        new XElement("in0", cmsBase64)))));

        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static string BuildTra(string service, int generationOffsetMinutes, int expirationMinutes)
    {
        var uniqueId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var generationTime = DateTimeOffset.UtcNow.AddMinutes(-Math.Abs(generationOffsetMinutes));
        var expirationTime = DateTimeOffset.UtcNow.AddMinutes(Math.Abs(expirationMinutes));

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("loginTicketRequest",
                new XAttribute("version", "1.0"),
                new XElement("header",
                    new XElement("uniqueId", uniqueId),
                    new XElement("generationTime", generationTime.ToString("yyyy-MM-ddTHH:mm:ssK")),
                    new XElement("expirationTime", expirationTime.ToString("yyyy-MM-ddTHH:mm:ssK"))),
                new XElement("service", service)));

        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static AfipWsaaCredentials ParseCredentials(string responseXml)
    {
        var soap = XDocument.Parse(responseXml);
        var loginCmsReturn = soap.Descendants().FirstOrDefault(x => x.Name.LocalName == "loginCmsReturn")?.Value;
        if (string.IsNullOrWhiteSpace(loginCmsReturn))
            throw new InvalidOperationException("WSAA no devolvio loginCmsReturn.");

        var ta = XDocument.Parse(loginCmsReturn);
        var token = ta.Descendants().FirstOrDefault(x => x.Name.LocalName == "token")?.Value?.Trim();
        var sign = ta.Descendants().FirstOrDefault(x => x.Name.LocalName == "sign")?.Value?.Trim();
        var expirationRaw = ta.Descendants().FirstOrDefault(x => x.Name.LocalName == "expirationTime")?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(sign))
            throw new InvalidOperationException("WSAA no devolvio token/sign validos.");

        DateTimeOffset? expiration = null;
        if (DateTimeOffset.TryParse(expirationRaw, out var parsed))
            expiration = parsed;

        return new AfipWsaaCredentials(token, sign, expiration);
    }

    private static void ValidateOptions(AfipWsaaOptions options)
    {
        if (!options.Enabled)
            throw new InvalidOperationException("WSAA no esta habilitado.");

        if (string.IsNullOrWhiteSpace(options.BaseUrl) ||
            string.IsNullOrWhiteSpace(options.CertificatePath) ||
            string.IsNullOrWhiteSpace(options.Service))
        {
            throw new InvalidOperationException(
                "La configuracion de WSAA es incompleta. Se requiere BaseUrl, CertificatePath y Service.");
        }
    }

    private sealed record AfipWsaaOptions(
        bool Enabled,
        string BaseUrl,
        string CertificatePath,
        string CertificatePassword,
        string Service,
        int GenerationOffsetMinutes,
        int ExpirationMinutes)
    {
        public static AfipWsaaOptions From(IConfiguration configuration)
        {
            var section = configuration.GetSection("Afip:Wsaa");

            var enabled = bool.TryParse(Environment.GetEnvironmentVariable("AFIP_WSAA_ENABLED"), out var envEnabled)
                ? envEnabled
                : section.GetValue<bool>("Enabled");

            var baseUrl = Environment.GetEnvironmentVariable("AFIP_WSAA_BASE_URL")
                ?? section["BaseUrl"]
                ?? "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";

            var certificatePath = Environment.GetEnvironmentVariable("AFIP_WSAA_CERTIFICATE_PATH")
                ?? section["CertificatePath"]
                ?? string.Empty;

            var certificatePassword = Environment.GetEnvironmentVariable("AFIP_WSAA_CERTIFICATE_PASSWORD")
                ?? section["CertificatePassword"]
                ?? string.Empty;

            var service = Environment.GetEnvironmentVariable("AFIP_WSAA_SERVICE")
                ?? section["Service"]
                ?? "wsfe";

            var generationOffsetMinutes = int.TryParse(Environment.GetEnvironmentVariable("AFIP_WSAA_GENERATION_OFFSET_MINUTES"), out var envGeneration)
                ? envGeneration
                : section.GetValue("GenerationOffsetMinutes", 10);

            var expirationMinutes = int.TryParse(Environment.GetEnvironmentVariable("AFIP_WSAA_EXPIRATION_MINUTES"), out var envExpiration)
                ? envExpiration
                : section.GetValue("ExpirationMinutes", 12 * 60);

            return new AfipWsaaOptions(
                enabled,
                baseUrl,
                certificatePath,
                certificatePassword,
                service,
                generationOffsetMinutes,
                expirationMinutes);
        }
    }
}