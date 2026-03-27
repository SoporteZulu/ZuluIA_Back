using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalProviderHttpGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<TResponse> PostJsonAsync<TResponse>(ExternalIntegrationProviderSettings settings, string operationPath, string requestPayload, CancellationToken ct)
    {
        var endpoint = BuildUri(settings.Endpoint, operationPath);
        using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(settings.TimeoutMs) };
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(requestPayload, Encoding.UTF8, "application/json")
        };

        ApplyHeaders(request, settings);

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            ThrowFromResponse(response.StatusCode, body);

        if (typeof(TResponse) == typeof(string))
            return (TResponse)(object)body;

        var deserialized = JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
        if (deserialized is null)
            throw new InvalidOperationException("No se pudo deserializar la respuesta del proveedor externo.");

        return deserialized;
    }

    public bool ShouldUseRealTransport(string endpoint)
        => Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            && !uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
            && !uri.Host.Contains("integracion.local", StringComparison.OrdinalIgnoreCase)
            && !uri.Host.Contains("afip.local", StringComparison.OrdinalIgnoreCase)
            && !uri.Host.Contains("sifen.local", StringComparison.OrdinalIgnoreCase)
            && !uri.Host.Contains("deuce.local", StringComparison.OrdinalIgnoreCase);

    private static Uri BuildUri(string endpoint, string operationPath)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri))
            throw new InvalidOperationException("El endpoint configurado del proveedor no es válido.");

        if (string.IsNullOrWhiteSpace(operationPath))
            return baseUri;

        return new Uri(baseUri, operationPath.TrimStart('/'));
    }

    private static void ApplyHeaders(HttpRequestMessage request, ExternalIntegrationProviderSettings settings)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(settings.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token.Trim());

        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            request.Headers.TryAddWithoutValidation("X-Api-Key", settings.ApiKey.Trim());

        if (!string.IsNullOrWhiteSpace(settings.UserName) && !string.IsNullOrWhiteSpace(settings.Password))
        {
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.UserName}:{settings.Password}"));
            request.Headers.Authorization ??= new AuthenticationHeaderValue("Basic", basic);
        }

        if (!string.IsNullOrWhiteSpace(settings.CertificateAlias))
            request.Headers.TryAddWithoutValidation("X-Certificate-Alias", settings.CertificateAlias.Trim());
    }

    private static void ThrowFromResponse(HttpStatusCode statusCode, string body)
    {
        var message = ExtractMessage(body);
        var code = ExtractErrorCode(body) ?? $"HTTP_{(int)statusCode}";

        if (statusCode is HttpStatusCode.BadRequest
            or HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden
            or HttpStatusCode.NotFound
            or HttpStatusCode.Conflict
            or HttpStatusCode.UnprocessableEntity)
        {
            throw new ExternalProviderFunctionalException(message, code);
        }

        throw new InvalidOperationException($"Proveedor externo respondió {(int)statusCode}: {message}");
    }

    private static string ExtractMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "Sin detalle de error.";

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var message))
                    return message.GetString() ?? body;
                if (root.TryGetProperty("error", out var error))
                    return error.GetString() ?? body;
                if (root.TryGetProperty("mensaje", out var mensaje))
                    return mensaje.GetString() ?? body;
            }
        }
        catch
        {
        }

        return body.Length > 500 ? body[..500] : body;
    }

    private static string? ExtractErrorCode(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return null;

            if (root.TryGetProperty("code", out var code))
                return code.GetString()?.Trim().ToUpperInvariant();
            if (root.TryGetProperty("errorCode", out var errorCode))
                return errorCode.GetString()?.Trim().ToUpperInvariant();
            if (root.TryGetProperty("codigo", out var codigo))
                return codigo.GetString()?.Trim().ToUpperInvariant();
        }
        catch
        {
        }

        return null;
    }
}
