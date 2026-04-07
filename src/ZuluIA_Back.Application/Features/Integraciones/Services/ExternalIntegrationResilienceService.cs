using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ExternalIntegrationResilienceService(
    ExternalIntegrationProviderSettingsService settingsService,
    ExternalProviderErrorCatalogService errorCatalogService,
    IRepository<IntegracionExternaAudit> auditRepo,
    ICurrentUserService currentUser)
{
    private static readonly ConcurrentDictionary<string, CircuitState> Circuits = new();
    private const int DefaultTimeoutMs = 15000;
    private const int DefaultRetryCount = 2;
    private const int DefaultCircuitThreshold = 3;
    private static readonly TimeSpan DefaultCircuitOpenFor = TimeSpan.FromMinutes(1);

    public async Task<ExternalIntegrationExecutionResult<T>> ExecuteAsync<T>(
        ProveedorIntegracionExterna proveedor,
        string operacion,
        string? referenciaTipo,
        long? referenciaId,
        string requestPayload,
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct,
        int timeoutMs = DefaultTimeoutMs,
        int retryCount = DefaultRetryCount,
        int circuitThreshold = DefaultCircuitThreshold,
        TimeSpan? circuitOpenFor = null,
        Func<T, string>? responseSerializer = null)
    {
        var settings = await settingsService.ResolveAsync(proveedor, ct);
        if (!settings.Habilitada)
        {
            await auditRepo.AddAsync(IntegracionExternaAudit.Registrar(
                proveedor,
                operacion,
                referenciaTipo,
                referenciaId,
                false,
                0,
                settings.TimeoutMs,
                false,
                0,
                settings.Ambiente,
                settings.Endpoint,
                "PROVEEDOR_DESHABILITADO",
                true,
                requestPayload,
                string.Empty,
                $"La integración {proveedor} está deshabilitada para el ambiente configurado.",
                currentUser.UserId), ct);

            return new ExternalIntegrationExecutionResult<T>
            {
                IsSuccess = false,
                Error = $"La integración {proveedor} está deshabilitada para el ambiente configurado.",
                ErrorCode = "PROVEEDOR_DESHABILITADO",
                IsFunctionalError = true,
                CircuitOpen = false,
                RetryCount = 0,
                Ambiente = settings.Ambiente,
                Endpoint = settings.Endpoint
            };
        }

        var key = $"{proveedor}:{operacion}".ToUpperInvariant();
        var state = Circuits.GetOrAdd(key, _ => new CircuitState());
        var now = DateTimeOffset.UtcNow;
        var effectiveTimeoutMs = timeoutMs == DefaultTimeoutMs ? settings.TimeoutMs : timeoutMs;
        var effectiveRetryCount = retryCount == DefaultRetryCount ? settings.Reintentos : retryCount;
        var effectiveCircuitThreshold = circuitThreshold == DefaultCircuitThreshold ? settings.CircuitThreshold : circuitThreshold;
        var openFor = circuitOpenFor ?? settings.CircuitOpenFor;

        if (state.IsOpen(now))
        {
            await auditRepo.AddAsync(IntegracionExternaAudit.Registrar(
                proveedor,
                operacion,
                referenciaTipo,
                referenciaId,
                false,
                0,
                effectiveTimeoutMs,
                true,
                0,
                settings.Ambiente,
                settings.Endpoint,
                "CIRCUIT_OPEN",
                false,
                requestPayload,
                string.Empty,
                "Circuit breaker abierto.",
                currentUser.UserId), ct);

            return new ExternalIntegrationExecutionResult<T>
            {
                IsSuccess = false,
                Error = "Circuit breaker abierto.",
                ErrorCode = "CIRCUIT_OPEN",
                IsFunctionalError = false,
                CircuitOpen = true,
                RetryCount = 0,
                Ambiente = settings.Ambiente,
                Endpoint = settings.Endpoint
            };
        }

        Exception? lastException = null;
        var attempts = 0;
        var stopwatch = Stopwatch.StartNew();

        while (attempts <= effectiveRetryCount)
        {
            attempts++;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linkedCts.CancelAfter(effectiveTimeoutMs);

            try
            {
                var value = await action(linkedCts.Token);
                state.Reset();
                stopwatch.Stop();

                var responsePayload = responseSerializer is not null
                    ? responseSerializer(value)
                    : JsonSerializer.Serialize(value);

                await auditRepo.AddAsync(IntegracionExternaAudit.Registrar(
                    proveedor,
                    operacion,
                    referenciaTipo,
                    referenciaId,
                    true,
                    attempts - 1,
                    effectiveTimeoutMs,
                    false,
                    stopwatch.ElapsedMilliseconds,
                    settings.Ambiente,
                    settings.Endpoint,
                    null,
                    false,
                    requestPayload,
                    responsePayload,
                    null,
                    currentUser.UserId), ct);

                return new ExternalIntegrationExecutionResult<T>
                {
                    IsSuccess = true,
                    Value = value,
                    ResponsePayload = responsePayload,
                    RetryCount = attempts - 1,
                    CircuitOpen = false,
                    Ambiente = settings.Ambiente,
                    Endpoint = settings.Endpoint
                };
            }
            catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
            {
                lastException = new TimeoutException($"Timeout excedido en la operación externa '{operacion}'.", ex);
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        stopwatch.Stop();
        state.RegisterFailure(now, effectiveCircuitThreshold, openFor);
        var circuitOpen = state.IsOpen(DateTimeOffset.UtcNow);
        var error = lastException?.Message ?? "Error desconocido en integración externa.";
        var (errorCode, functionalError, normalizedMessage) = ClassifyError(proveedor, lastException, errorCatalogService, error);

        await auditRepo.AddAsync(IntegracionExternaAudit.Registrar(
            proveedor,
            operacion,
            referenciaTipo,
            referenciaId,
            false,
            attempts - 1,
            effectiveTimeoutMs,
            circuitOpen,
            stopwatch.ElapsedMilliseconds,
            settings.Ambiente,
            settings.Endpoint,
            errorCode,
            functionalError,
            requestPayload,
            JsonSerializer.Serialize(new { error = normalizedMessage, errorCode, functionalError, circuitOpen, settings.Ambiente, settings.Endpoint }),
            normalizedMessage,
            currentUser.UserId), ct);

        return new ExternalIntegrationExecutionResult<T>
        {
            IsSuccess = false,
            Error = normalizedMessage,
            ErrorCode = errorCode,
            IsFunctionalError = functionalError,
            RetryCount = attempts - 1,
            CircuitOpen = circuitOpen,
            Ambiente = settings.Ambiente,
            Endpoint = settings.Endpoint,
            ResponsePayload = JsonSerializer.Serialize(new { error = normalizedMessage, errorCode, functionalError, circuitOpen, settings.Ambiente, settings.Endpoint })
        };
    }

    private static (string errorCode, bool functionalError, string message) ClassifyError(ProveedorIntegracionExterna proveedor, Exception? exception, ExternalProviderErrorCatalogService errorCatalogService, string message)
    {
        if (exception is ExternalProviderFunctionalException functional)
        {
            var resolved = errorCatalogService.Resolve(proveedor, functional.Code, functional.Message);
            return (resolved.Codigo, resolved.ErrorFuncional, resolved.Mensaje);
        }

        if (exception is TimeoutException)
            return ("TIMEOUT", false, message);

        var genericCode = exception is InvalidOperationException ? null : "ERROR_TECNICO";
        var resolvedGeneric = errorCatalogService.Resolve(proveedor, genericCode, message);
        return (resolvedGeneric.Codigo, resolvedGeneric.ErrorFuncional, resolvedGeneric.Mensaje);
    }

    private sealed class CircuitState
    {
        private int _consecutiveFailures;
        private DateTimeOffset? _openUntil;

        public bool IsOpen(DateTimeOffset now) => _openUntil.HasValue && _openUntil.Value > now;

        public void Reset()
        {
            _consecutiveFailures = 0;
            _openUntil = null;
        }

        public void RegisterFailure(DateTimeOffset now, int threshold, TimeSpan openFor)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= threshold)
                _openUntil = now.Add(openFor);
        }
    }
}
