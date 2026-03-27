using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Infrastructure.Services;

public sealed class ParaguaySifenService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ParaguaySifenService> logger) : IParaguaySifenService
{
    public Task<PreparacionSifenParaguayDto> PrepararEnvioAsync(
        PrepararEnvioSifenRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = ParaguaySifenOptions.From(configuration);
        var errores = ValidarConfiguracion(options);

        if (errores.Count > 0)
        {
            logger.LogInformation(
                "Preparacion SIFEN/SET incompleta para comprobante {ComprobanteId}. Errores: {Errores}",
                request.ComprobanteId,
                string.Join(" | ", errores));
        }

        return Task.FromResult(new PreparacionSifenParaguayDto
        {
            ComprobanteId = request.ComprobanteId,
            IntegracionHabilitada = options.Enabled,
            ListoParaEnviar = errores.Count == 0,
            Ambiente = options.Environment,
            Endpoint = options.BaseUrl,
            ModoTransporte = options.TransportMode,
            Errores = errores,
            Documento = new PreparacionSifenDocumentoDto
            {
                RucEmisor = request.RucEmisor,
                RazonSocialEmisor = request.RazonSocialEmisor,
                DireccionEmisor = request.DireccionEmisor,
                DocumentoReceptor = request.DocumentoReceptor,
                RazonSocialReceptor = request.RazonSocialReceptor,
                DireccionReceptor = request.DireccionReceptor,
                CodigoTipoComprobante = request.CodigoTipoComprobante,
                DescripcionTipoComprobante = request.DescripcionTipoComprobante,
                PuntoExpedicion = request.PuntoExpedicion,
                Prefijo = request.Prefijo,
                NumeroComprobante = request.NumeroComprobante,
                FechaEmision = request.FechaEmision,
                Total = request.Total,
                NetoGravado = request.NetoGravado,
                Iva = request.Iva,
                NetoNoGravado = request.NetoNoGravado,
                Percepciones = request.Percepciones,
                CantidadItems = request.CantidadItems,
                TimbradoId = request.TimbradoId,
                NroTimbrado = request.NroTimbrado,
                Observacion = request.Observacion
            }
        });
    }

    public async Task<ResultadoEnvioSifenParaguayDto> EnviarAsync(
        PrepararEnvioSifenRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = ParaguaySifenOptions.From(configuration);
        var errores = ValidarConfiguracion(options);
        if (errores.Count > 0)
            throw new InvalidOperationException(string.Join(" | ", errores));

        if (string.Equals(options.TransportMode, "stub", StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = request.ComprobanteId,
                Aceptado = true,
                Estado = "stub-accepted",
                CodigoRespuesta = null,
                MensajeRespuesta = null,
                TrackingId = $"SIFEN-{request.ComprobanteId}",
                Cdc = null,
                NumeroLote = null,
                FechaRespuesta = DateTimeOffset.UtcNow,
                RespuestaCruda = JsonSerializer.Serialize(new
                {
                    accepted = true,
                    status = "stub-accepted",
                    trackingId = $"SIFEN-{request.ComprobanteId}"
                })
            };
        }

        var payload = JsonSerializer.Serialize(request);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, options.BaseUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.Add("X-Api-Key", options.ApiKey);
        httpRequest.Headers.Add("X-Api-Secret", options.ApiSecret);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "SIFEN/SET devolvio estado HTTP {StatusCode} para comprobante {ComprobanteId}. Respuesta: {Respuesta}",
                (int)response.StatusCode,
                request.ComprobanteId,
                responseContent);

            throw new InvalidOperationException(
                $"SIFEN/SET rechazo el comprobante con HTTP {(int)response.StatusCode}. {responseContent}".Trim());
        }

        return ParseSendResponse(request.ComprobanteId, responseContent);
    }

    public async Task<ResultadoEnvioSifenParaguayDto> ConsultarEstadoAsync(
        ConsultarEstadoSifenRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = ParaguaySifenOptions.From(configuration);
        var errores = ValidarConfiguracion(options);
        if (errores.Count > 0)
            throw new InvalidOperationException(string.Join(" | ", errores));

        if (string.IsNullOrWhiteSpace(request.TrackingId)
            && string.IsNullOrWhiteSpace(request.Cdc)
            && string.IsNullOrWhiteSpace(request.NumeroLote))
        {
            throw new InvalidOperationException("Se requiere TrackingId, CDC o numero de lote para consultar SIFEN/SET.");
        }

        if (string.Equals(options.TransportMode, "stub", StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = request.ComprobanteId,
                Aceptado = true,
                Estado = "stub-status-accepted",
                CodigoRespuesta = null,
                MensajeRespuesta = null,
                TrackingId = request.TrackingId ?? $"SIFEN-{request.ComprobanteId}",
                Cdc = request.Cdc,
                NumeroLote = request.NumeroLote,
                FechaRespuesta = DateTimeOffset.UtcNow,
                RespuestaCruda = JsonSerializer.Serialize(new
                {
                    accepted = true,
                    status = "stub-status-accepted",
                    trackingId = request.TrackingId,
                    cdc = request.Cdc,
                    numeroLote = request.NumeroLote
                })
            };
        }

        var statusUrl = BuildStatusUrl(options.StatusUrl, request);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, statusUrl);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.Add("X-Api-Key", options.ApiKey);
        httpRequest.Headers.Add("X-Api-Secret", options.ApiSecret);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "SIFEN/SET devolvio estado HTTP {StatusCode} al consultar comprobante {ComprobanteId}. Respuesta: {Respuesta}",
                (int)response.StatusCode,
                request.ComprobanteId,
                responseContent);

            throw new InvalidOperationException(
                $"SIFEN/SET rechazo la consulta de estado con HTTP {(int)response.StatusCode}. {responseContent}".Trim());
        }

        var parsed = ParseSendResponse(request.ComprobanteId, responseContent);
        parsed.TrackingId ??= request.TrackingId;
        parsed.Cdc ??= request.Cdc;
        parsed.NumeroLote ??= request.NumeroLote;
        return parsed;
    }

    private static List<string> ValidarConfiguracion(ParaguaySifenOptions options)
    {
        var errores = new List<string>();

        if (!options.Enabled)
        {
            errores.Add("La integracion Paraguay SIFEN/SET no esta habilitada.");
            return errores;
        }

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            errores.Add("Falta configurar Paraguay:Sifen:BaseUrl.");

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            errores.Add("Falta configurar Paraguay:Sifen:ApiKey.");

        if (string.IsNullOrWhiteSpace(options.ApiSecret))
            errores.Add("Falta configurar Paraguay:Sifen:ApiSecret.");

        if (string.IsNullOrWhiteSpace(options.Environment))
            errores.Add("Falta configurar Paraguay:Sifen:Environment.");

        if (string.IsNullOrWhiteSpace(options.TransportMode))
            errores.Add("Falta configurar Paraguay:Sifen:TransportMode.");

        return errores;
    }

    private static ResultadoEnvioSifenParaguayDto ParseSendResponse(long comprobanteId, string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = comprobanteId,
                Aceptado = true,
                Estado = "accepted",
                FechaRespuesta = DateTimeOffset.UtcNow,
                RespuestaCruda = rawResponse
            };
        }

        try
        {
            using var document = JsonDocument.Parse(rawResponse);
            var root = document.RootElement;

            var accepted = ReadBoolean(root, "accepted")
                ?? ReadBoolean(root, "aceptado")
                ?? ReadBoolean(root, "success")
                ?? true;

            var status = ReadString(root, "status")
                ?? ReadString(root, "estado")
                ?? (accepted ? "accepted" : "rejected");

            var codigoRespuesta = ReadString(root, "errorCode")
                ?? ReadString(root, "codigoError")
                ?? ReadString(root, "codigo_respuesta")
                ?? ReadString(root, "codigoRespuesta")
                ?? ReadString(root, "code")
                ?? ReadNestedErrorValue(root, "code")
                ?? ReadNestedErrorValue(root, "codigo");

            var mensajeRespuesta = ReadString(root, "errorMessage")
                ?? ReadString(root, "mensajeError")
                ?? ReadString(root, "mensaje_respuesta")
                ?? ReadString(root, "mensajeRespuesta")
                ?? ReadString(root, "message")
                ?? ReadString(root, "mensaje")
                ?? ReadNestedErrorValue(root, "message")
                ?? ReadNestedErrorValue(root, "mensaje")
                ?? ReadNestedErrorValue(root, "descripcion");

            var cdc = ReadString(root, "cdc")
                ?? ReadString(root, "CDC");

            var numeroLote = ReadString(root, "numeroLote")
                ?? ReadString(root, "numero_lote")
                ?? ReadString(root, "lote");

            var trackingId = ReadString(root, "trackingId")
                ?? ReadString(root, "id")
                ?? cdc
                ?? numeroLote;

            return new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = comprobanteId,
                Aceptado = accepted,
                Estado = status,
                CodigoRespuesta = codigoRespuesta,
                MensajeRespuesta = mensajeRespuesta,
                TrackingId = trackingId,
                Cdc = cdc,
                NumeroLote = numeroLote,
                FechaRespuesta = DateTimeOffset.UtcNow,
                RespuestaCruda = rawResponse
            };
        }
        catch (JsonException)
        {
            return new ResultadoEnvioSifenParaguayDto
            {
                ComprobanteId = comprobanteId,
                Aceptado = true,
                Estado = "accepted",
                FechaRespuesta = DateTimeOffset.UtcNow,
                RespuestaCruda = rawResponse
            };
        }
    }

    private static bool? ReadBoolean(JsonElement root, string propertyName)
    {
        if (!TryGetProperty(root, propertyName, out var property))
            return null;

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out var value) => value,
            _ => null
        };
    }

    private static string? ReadString(JsonElement root, string propertyName)
        => TryGetProperty(root, propertyName, out var property)
            ? property.ToString()
            : null;

    private static string? ReadNestedErrorValue(JsonElement root, string propertyName)
    {
        if (TryGetProperty(root, "errors", out var errorsElement))
        {
            if (errorsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in errorsElement.EnumerateArray())
                {
                    var value = ReadString(item, propertyName);
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            if (errorsElement.ValueKind == JsonValueKind.Object)
            {
                var value = ReadString(errorsElement, propertyName);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement value)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string BuildStatusUrl(string statusUrl, ConsultarEstadoSifenRequest request)
    {
        var queryParts = new List<string>();

        AppendQuery(queryParts, "trackingId", request.TrackingId);
        AppendQuery(queryParts, "cdc", request.Cdc);
        AppendQuery(queryParts, "numeroLote", request.NumeroLote);

        if (queryParts.Count == 0)
            return statusUrl;

        var separator = statusUrl.Contains('?') ? '&' : '?';
        return $"{statusUrl}{separator}{string.Join("&", queryParts)}";
    }

    private static void AppendQuery(List<string> queryParts, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        queryParts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
    }

    private sealed record ParaguaySifenOptions(
        bool Enabled,
        string BaseUrl,
        string StatusUrl,
        string ApiKey,
        string ApiSecret,
        string Environment,
        string TransportMode)
    {
        public static ParaguaySifenOptions From(IConfiguration configuration)
        {
            var section = configuration.GetSection("Paraguay:Sifen");

            var enabled = bool.TryParse(System.Environment.GetEnvironmentVariable("PARAGUAY_SIFEN_ENABLED"), out var envEnabled)
                ? envEnabled
                : section.GetValue<bool>("Enabled");

            var baseUrl = System.Environment.GetEnvironmentVariable("PARAGUAY_SIFEN_BASE_URL")
                ?? section["BaseUrl"]
                ?? string.Empty;

            var statusUrl = System.Environment.GetEnvironmentVariable("PARAGUAY_SIFEN_STATUS_URL")
                ?? section["StatusUrl"]
                ?? baseUrl;

            var apiKey = System.Environment.GetEnvironmentVariable("PARAGUAY_SIFEN_API_KEY")
                ?? section["ApiKey"]
                ?? string.Empty;

            var apiSecret = System.Environment.GetEnvironmentVariable("PARAGUAY_SIFEN_API_SECRET")
                ?? section["ApiSecret"]
                ?? string.Empty;

            var environment = System.Environment.GetEnvironmentVariable("PARAGUAY_SIFEN_ENVIRONMENT")
                ?? section["Environment"]
                ?? "test";

            var transportMode = System.Environment.GetEnvironmentVariable("PARAGUAY_SIFEN_TRANSPORT_MODE")
                ?? section["TransportMode"]
                ?? "http";

            return new ParaguaySifenOptions(enabled, baseUrl, statusUrl, apiKey, apiSecret, environment, transportMode);
        }
    }
}