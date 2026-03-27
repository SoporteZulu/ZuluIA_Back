using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Services;

public class CartaPorteWorkflowService(
    IApplicationDbContext db,
    IRepository<OrdenCarga> ordenCargaRepo,
    IRepository<CartaPorteEvento> eventoRepo,
    ExternalIntegrationProviderSettingsService providerSettingsService,
    ExternalProviderHttpGateway httpGateway,
    ExternalIntegrationResilienceService resilienceService,
    ICurrentUserService currentUser)
{
    public async Task RegistrarAltaAsync(CartaPorte carta, CancellationToken ct = default)
    {
        await RegistrarEventoAsync(
            carta,
            TipoEventoCartaPorte.Alta,
            null,
            carta.FechaEmision,
            carta.Observacion,
            ct);
    }

    public async Task<OrdenCarga> CrearOrdenCargaAsync(
        long cartaPorteId,
        long? transportistaId,
        DateOnly fechaCarga,
        string origen,
        string destino,
        string? patente,
        string? observacion,
        CancellationToken ct = default)
    {
        var carta = await db.CartasPorte.FirstOrDefaultAsync(x => x.Id == cartaPorteId, ct)
            ?? throw new InvalidOperationException($"No se encontró la carta de porte con ID {cartaPorteId}.");

        string? cuitTransportista = null;
        if (transportistaId.HasValue)
        {
            var transportista = await db.Transportistas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == transportistaId.Value, ct)
                ?? throw new InvalidOperationException($"No se encontró el transportista con ID {transportistaId.Value}.");

            cuitTransportista = transportista.NroCuitTransportista;
        }

        var orden = OrdenCarga.Crear(
            carta.Id,
            transportistaId,
            fechaCarga,
            origen,
            destino,
            patente,
            observacion,
            currentUser.UserId);

        await ordenCargaRepo.AddAsync(orden, ct);
        return orden;
    }

    public async Task AsignarOrdenCargaAsync(
        long cartaPorteId,
        long ordenCargaId,
        long? transportistaId,
        string? cuitTransportista,
        string? observacion,
        DateOnly fechaEvento,
        CancellationToken ct = default)
    {
        var carta = await db.CartasPorte.FirstOrDefaultAsync(x => x.Id == cartaPorteId, ct)
            ?? throw new InvalidOperationException($"No se encontró la carta de porte con ID {cartaPorteId}.");

        var estadoAnterior = carta.Estado;
        carta.AsignarOrdenCarga(ordenCargaId, transportistaId, cuitTransportista, observacion, currentUser.UserId);

        await RegistrarEventoAsync(
            carta,
            TipoEventoCartaPorte.OrdenCargaAsignada,
            estadoAnterior,
            fechaEvento,
            observacion,
            ct);
    }

    public async Task SolicitarCtgAsync(
        long cartaPorteId,
        DateOnly fechaSolicitud,
        string? observacion,
        bool esReintento,
        CancellationToken ct = default)
    {
        var carta = await db.CartasPorte.FirstOrDefaultAsync(x => x.Id == cartaPorteId, ct)
            ?? throw new InvalidOperationException($"No se encontró la carta de porte con ID {cartaPorteId}.");

        var providerSettings = await providerSettingsService.ResolveAsync(ProveedorIntegracionExterna.Ctg, ct);
        var operationSettings = await providerSettingsService.ResolveOperationAsync(
            ProveedorIntegracionExterna.Ctg,
            esReintento ? "SOLICITAR_CTG_REINTENTO" : "SOLICITAR_CTG",
            esReintento ? "ctg/solicitar-reintento" : "ctg/solicitar",
            ct);
        ValidarCredencialesCtg(providerSettings);

        var requestContract = new CtgSolicitudRequestContract(
            carta.Id,
            carta.ComprobanteId,
            carta.CuitRemitente,
            carta.CuitDestinatario,
            carta.CuitTransportista,
            providerSettings.Ambiente,
            providerSettings.Endpoint,
            fechaSolicitud,
            observacion,
            esReintento);

        var requestPayload = System.Text.Json.JsonSerializer.Serialize(requestContract);

        var execution = await resilienceService.ExecuteAsync(
            ProveedorIntegracionExterna.Ctg,
            esReintento ? "SOLICITAR_CTG_REINTENTO" : "SOLICITAR_CTG",
            "CARTA_PORTE",
            carta.Id,
            requestPayload,
            token => EjecutarSolicitudCtgAsync(providerSettings, operationSettings, carta, requestPayload, token),
            ct,
            timeoutMs: operationSettings.TimeoutMs,
            retryCount: operationSettings.Reintentos,
            circuitThreshold: operationSettings.CircuitThreshold,
            circuitOpenFor: operationSettings.CircuitOpenFor);

        if (!execution.IsSuccess)
            throw new InvalidOperationException(execution.Error ?? "Falló la solicitud de CTG.");

        if (execution.Value is not null && !execution.Value.Exitoso)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(execution.Value.Mensaje)
                ? "CTG rechazó la solicitud realizada."
                : execution.Value.Mensaje);

        var estadoAnterior = carta.Estado;
        carta.SolicitarCtg(fechaSolicitud, observacion, currentUser.UserId);

        await RegistrarEventoAsync(
            carta,
            esReintento ? TipoEventoCartaPorte.CtgReintento : TipoEventoCartaPorte.CtgSolicitado,
            estadoAnterior,
            fechaSolicitud,
            observacion,
            ct);
    }

    public async Task ConsultarCtgAsync(
        long cartaPorteId,
        DateOnly fechaConsulta,
        string? nroCtg,
        string? error,
        string? observacion,
        CancellationToken ct = default)
    {
        var carta = await db.CartasPorte.FirstOrDefaultAsync(x => x.Id == cartaPorteId, ct)
            ?? throw new InvalidOperationException($"No se encontró la carta de porte con ID {cartaPorteId}.");

        var providerSettings = await providerSettingsService.ResolveAsync(ProveedorIntegracionExterna.Ctg, ct);
        var operationSettings = await providerSettingsService.ResolveOperationAsync(
            ProveedorIntegracionExterna.Ctg,
            "CONSULTAR_CTG",
            "ctg/consultar",
            ct);
        ValidarCredencialesCtg(providerSettings);

        var requestContract = new CtgConsultaRequestContract(
            carta.Id,
            carta.NroCtg,
            providerSettings.Ambiente,
            providerSettings.Endpoint,
            fechaConsulta,
            nroCtg,
            error,
            observacion);

        var requestPayload = System.Text.Json.JsonSerializer.Serialize(requestContract);

        var execution = await resilienceService.ExecuteAsync(
            ProveedorIntegracionExterna.Ctg,
            "CONSULTAR_CTG",
            "CARTA_PORTE",
            carta.Id,
            requestPayload,
            token => EjecutarConsultaCtgAsync(providerSettings, operationSettings, nroCtg, error, observacion, requestPayload, token),
            ct,
            timeoutMs: operationSettings.TimeoutMs,
            retryCount: operationSettings.Reintentos,
            circuitThreshold: operationSettings.CircuitThreshold,
            circuitOpenFor: operationSettings.CircuitOpenFor,
            responseSerializer: x => System.Text.Json.JsonSerializer.Serialize(x));

        if (!execution.IsSuccess || execution.Value is null)
            throw new InvalidOperationException(execution.Error ?? "Falló la consulta de CTG.");

        var estadoAnterior = carta.Estado;

        var consulta = execution.Value;

        if (!string.IsNullOrWhiteSpace(consulta?.NroCtg))
        {
            carta.AsignarCtg(consulta.NroCtg, currentUser.UserId);
            await RegistrarEventoAsync(
                carta,
                TipoEventoCartaPorte.CtgAsignado,
                estadoAnterior,
                fechaConsulta,
                consulta.Observacion,
                ct);
            return;
        }

        if (!string.IsNullOrWhiteSpace(consulta?.Error))
        {
            carta.RegistrarErrorCtg(consulta.Error, consulta.Observacion, currentUser.UserId);
            await RegistrarEventoAsync(
                carta,
                TipoEventoCartaPorte.CtgError,
                estadoAnterior,
                fechaConsulta,
                consulta.Error,
                ct);
            return;
        }

        await RegistrarEventoAsync(
            carta,
            TipoEventoCartaPorte.CtgConsultado,
            estadoAnterior,
            fechaConsulta,
            observacion,
            ct);
    }

    public async Task ConfirmarAsync(long cartaPorteId, DateOnly fechaEvento, string? observacion, CancellationToken ct = default)
    {
        var carta = await db.CartasPorte.FirstOrDefaultAsync(x => x.Id == cartaPorteId, ct)
            ?? throw new InvalidOperationException($"No se encontró la carta de porte con ID {cartaPorteId}.");

        var estadoAnterior = carta.Estado;
        carta.Confirmar(currentUser.UserId);

        await RegistrarEventoAsync(
            carta,
            TipoEventoCartaPorte.Confirmacion,
            estadoAnterior,
            fechaEvento,
            observacion,
            ct);
    }

    public async Task AnularAsync(long cartaPorteId, DateOnly fechaEvento, string? observacion, CancellationToken ct = default)
    {
        var carta = await db.CartasPorte.FirstOrDefaultAsync(x => x.Id == cartaPorteId, ct)
            ?? throw new InvalidOperationException($"No se encontró la carta de porte con ID {cartaPorteId}.");

        var estadoAnterior = carta.Estado;
        carta.Anular(observacion, currentUser.UserId);

        await RegistrarEventoAsync(
            carta,
            TipoEventoCartaPorte.Anulacion,
            estadoAnterior,
            fechaEvento,
            observacion,
            ct);
    }

    private async Task RegistrarEventoAsync(
        CartaPorte carta,
        TipoEventoCartaPorte tipoEvento,
        EstadoCartaPorte? estadoAnterior,
        DateOnly fechaEvento,
        string? mensaje,
        CancellationToken ct)
    {
        var evento = CartaPorteEvento.Registrar(
            carta.Id,
            carta.OrdenCargaId,
            tipoEvento,
            estadoAnterior,
            carta.Estado,
            fechaEvento,
            string.IsNullOrWhiteSpace(mensaje) ? carta.UltimoErrorCtg ?? carta.Observacion : mensaje,
            carta.NroCtg,
            carta.IntentosCtg,
            currentUser.UserId);

        await eventoRepo.AddAsync(evento, ct);
    }

    private static CtgSolicitudResponseContract ResolverSolicitudCtg(CartaPorte carta)
    {
        if (string.IsNullOrWhiteSpace(carta.CuitTransportista))
            throw new ExternalProviderFunctionalException("CTG requiere CUIT de transportista para solicitar la autorización.", "CUIT_TRANSPORTISTA_REQUERIDO");

        if (string.IsNullOrWhiteSpace(carta.CuitRemitente) || string.IsNullOrWhiteSpace(carta.CuitDestinatario))
            throw new ExternalProviderFunctionalException("CTG requiere CUIT remitente y destinatario válidos.", "CUITS_REQUERIDOS");

        return new CtgSolicitudResponseContract(true, "SOLICITADO", "Solicitud CTG enviada correctamente.");
    }

    private static void ValidarCredencialesCtg(ExternalIntegrationProviderSettings settings)
    {
        var hasCredentials = !string.IsNullOrWhiteSpace(settings.Token)
            || !string.IsNullOrWhiteSpace(settings.ApiKey)
            || (!string.IsNullOrWhiteSpace(settings.UserName) && !string.IsNullOrWhiteSpace(settings.Password));

        if (!hasCredentials)
            throw new InvalidOperationException("CTG requiere token, api key o usuario/clave configurados a nivel proveedor.");
    }

    private async Task<CtgSolicitudResponseContract> EjecutarSolicitudCtgAsync(ExternalIntegrationProviderSettings settings, ExternalIntegrationOperationSettings operationSettings, CartaPorte carta, string requestPayload, CancellationToken ct)
    {
        if (!httpGateway.ShouldUseRealTransport(settings.Endpoint))
            return ResolverSolicitudCtg(carta);

        var response = await httpGateway.PostJsonAsync<CtgSolicitudResponseContract>(settings, operationSettings.Path, requestPayload, ct);
        if (!response.Exitoso)
            throw new ExternalProviderFunctionalException(response.Mensaje ?? "CTG rechazó la solicitud realizada.", response.CodigoError);

        return response;
    }

    private async Task<CtgConsultaResponseContract> EjecutarConsultaCtgAsync(ExternalIntegrationProviderSettings settings, ExternalIntegrationOperationSettings operationSettings, string? nroCtg, string? error, string? observacion, string requestPayload, CancellationToken ct)
    {
        if (!httpGateway.ShouldUseRealTransport(settings.Endpoint))
            return new CtgConsultaResponseContract(nroCtg, error, observacion);

        return await httpGateway.PostJsonAsync<CtgConsultaResponseContract>(settings, operationSettings.Path, requestPayload, ct);
    }
}
