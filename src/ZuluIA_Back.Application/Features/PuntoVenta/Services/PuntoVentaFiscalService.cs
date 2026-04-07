using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Integraciones.DTOs;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.PuntoVenta.Enums;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Services;

public class PuntoVentaFiscalService(
    IMediator mediator,
    IApplicationDbContext db,
    IRepository<TimbradoFiscal> timbradoRepo,
    IRepository<OperacionPuntoVenta> operacionRepo,
    IRepository<SifenOperacion> sifenRepo,
    IRepository<DeuceOperacion> deuceRepo,
    ExternalIntegrationProviderSettingsService providerSettingsService,
    ExternalProviderHttpGateway httpGateway,
    ExternalIntegrationResilienceService resilienceService,
    ICurrentUserService currentUser)
{
    public async Task<long> RegistrarComprobanteAsync(Commands.RegistrarComprobantePuntoVentaCommand request, CanalOperacionPuntoVenta canal, CancellationToken ct)
    {
        if (!await db.PuntosFacturacion.AsNoTracking().AnyAsync(x => x.Id == request.PuntoFacturacionId && x.Activo, ct))
            throw new InvalidOperationException($"No se encontró el punto de facturación ID {request.PuntoFacturacionId}.");

        var emitido = await mediator.Send(new EmitirComprobanteCommand(
            Id: null,
            SucursalId: request.SucursalId,
            PuntoFacturacionId: request.PuntoFacturacionId,
            TipoComprobanteId: request.TipoComprobanteId,
            Fecha: request.Fecha,
            FechaVencimiento: request.FechaVencimiento,
            TerceroId: request.TerceroId,
            MonedaId: request.MonedaId,
            Cotizacion: request.Cotizacion,
            Percepciones: request.Percepciones,
            Observacion: request.Observacion,
            Items: request.Items,
            AfectaStock: request.AfectaStock), ct);

        if (!emitido.IsSuccess)
            throw new InvalidOperationException(emitido.Error ?? "No se pudo emitir el comprobante.");

        await operacionRepo.AddAsync(OperacionPuntoVenta.Registrar(
            emitido.Value,
            request.SucursalId,
            request.PuntoFacturacionId,
            canal,
            request.Fecha,
            request.ReferenciaExterna,
            request.Observacion,
            currentUser.UserId), ct);

        return emitido.Value;
    }

    public async Task<TimbradoFiscal> RegistrarTimbradoAsync(Commands.RegistrarTimbradoFiscalCommand request, CancellationToken ct)
    {
        var numero = request.NumeroTimbrado.Trim().ToUpperInvariant();
        if (await db.TimbradosFiscales.AsNoTracking().AnyAsync(x => x.NumeroTimbrado == numero, ct))
            throw new InvalidOperationException($"Ya existe un timbrado con número '{request.NumeroTimbrado}'.");

        if (!await db.PuntosFacturacion.AsNoTracking().AnyAsync(x => x.Id == request.PuntoFacturacionId && x.Activo, ct))
            throw new InvalidOperationException($"No se encontró el punto de facturación ID {request.PuntoFacturacionId}.");

        await ValidarSolapamientoTimbradoAsync(null, request.PuntoFacturacionId, request.VigenciaDesde, request.VigenciaHasta, ct);

        var timbrado = TimbradoFiscal.Crear(request.SucursalId, request.PuntoFacturacionId, request.NumeroTimbrado, request.VigenciaDesde, request.VigenciaHasta, request.Observacion, currentUser.UserId);
        await timbradoRepo.AddAsync(timbrado, ct);
        return timbrado;
    }

    public async Task ActualizarTimbradoAsync(Commands.UpdateTimbradoFiscalCommand request, CancellationToken ct)
    {
        var timbrado = await db.TimbradosFiscales.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el timbrado ID {request.Id}.");

        var numero = request.NumeroTimbrado.Trim().ToUpperInvariant();
        if (await db.TimbradosFiscales.AsNoTracking().AnyAsync(x => x.Id != request.Id && x.NumeroTimbrado == numero && !x.IsDeleted, ct))
            throw new InvalidOperationException($"Ya existe un timbrado con número '{request.NumeroTimbrado}'.");

        await ValidarSolapamientoTimbradoAsync(request.Id, timbrado.PuntoFacturacionId, request.VigenciaDesde, request.VigenciaHasta, ct);

        timbrado.Actualizar(request.NumeroTimbrado, request.VigenciaDesde, request.VigenciaHasta, request.Observacion, currentUser.UserId);
        timbradoRepo.Update(timbrado);
    }

    public async Task DesactivarTimbradoAsync(long id, string? observacion, CancellationToken ct)
    {
        var timbrado = await db.TimbradosFiscales.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el timbrado ID {id}.");

        timbrado.Desactivar(observacion, currentUser.UserId);
        timbradoRepo.Update(timbrado);
    }

    public async Task ConciliarSifenAsync(long id, bool confirmar, string? observacion, CancellationToken ct)
    {
        var operacion = await db.SifenOperaciones.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la operación SIFEN ID {id}.");

        if (confirmar)
            operacion.Confirmar(observacion, currentUser.UserId);
        else
            operacion.Rechazar(observacion, currentUser.UserId);

        sifenRepo.Update(operacion);
    }

    public async Task ConciliarDeuceAsync(long id, bool confirmar, string? observacion, CancellationToken ct)
    {
        var operacion = await db.DeuceOperaciones.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la operación Deuce ID {id}.");

        if (confirmar)
            operacion.Confirmar(observacion, currentUser.UserId);
        else
            operacion.Rechazar(observacion, currentUser.UserId);

        deuceRepo.Update(operacion);
    }

    public async Task<long> RegistrarConFiscalAlternativoAsync(Commands.RegistrarComprobanteFiscalAlternativoCommand request, CancellationToken ct)
    {
        var alta = new Commands.RegistrarComprobantePuntoVentaCommand(
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Fecha,
            request.FechaVencimiento,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            request.Percepciones,
            request.Observacion,
            request.AfectaStock,
            request.ReferenciaExterna,
            request.Items);

        var canal = request.Canal;
        var comprobanteId = await RegistrarComprobanteAsync(alta, canal, ct);

        if (request.Flujo == TipoFlujoFiscalAlternativo.Sifen)
        {
            await ProcesarSifenAsync(new Commands.ProcesarComprobanteSifenCommand(
                comprobanteId,
                request.TimbradoFiscalId,
                request.RequestPayload,
                request.ResponsePayload,
                request.CodigoSeguridad,
                request.Observacion,
                request.Confirmada), ct);
        }
        else
        {
            await ProcesarDeuceAsync(new Commands.ProcesarComprobanteDeuceCommand(
                comprobanteId,
                request.ReferenciaExterna ?? $"PV-{comprobanteId}",
                request.RequestPayload,
                request.ResponsePayload,
                request.Observacion,
                request.Confirmada), ct);
        }

        return comprobanteId;
    }

    public async Task<SifenOperacion> ProcesarSifenAsync(Commands.ProcesarComprobanteSifenCommand request, CancellationToken ct)
    {
        var comprobante = await db.Comprobantes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ComprobanteId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el comprobante ID {request.ComprobanteId}.");

        TimbradoFiscal? timbrado = null;
        if (request.TimbradoFiscalId.HasValue)
        {
            timbrado = await db.TimbradosFiscales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.TimbradoFiscalId.Value && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException($"No se encontró el timbrado ID {request.TimbradoFiscalId.Value}.");

            if (!timbrado.VigentePara(comprobante.Fecha))
                throw new InvalidOperationException($"El timbrado {timbrado.NumeroTimbrado} no está vigente para la fecha del comprobante.");
        }

        var puntoFacturacionSifenId = comprobante.PuntoFacturacionId
            ?? throw new InvalidOperationException("El comprobante no posee punto de facturación para SIFEN.");

        var sifenSettings = await providerSettingsService.ResolveAsync(ProveedorIntegracionExterna.Sifen, ct);
        var sifenOperationSettings = await providerSettingsService.ResolveOperationAsync(
            ProveedorIntegracionExterna.Sifen,
            "PROCESAR_COMPROBANTE",
            "sifen/comprobantes",
            ct);
        ValidarCredencialesProveedor(sifenSettings, "SIFEN");

        var requestPayload = request.RequestPayload ?? System.Text.Json.JsonSerializer.Serialize(new SifenProcesarRequestContract(
            comprobante.Id,
            comprobante.SucursalId,
            puntoFacturacionSifenId,
            request.TimbradoFiscalId,
            request.CodigoSeguridad,
            sifenSettings.Ambiente,
            sifenSettings.Endpoint,
            request.Observacion));

        var execution = await resilienceService.ExecuteAsync(
            ProveedorIntegracionExterna.Sifen,
            "PROCESAR_COMPROBANTE",
            "COMPROBANTE",
            comprobante.Id,
            requestPayload,
            token => EjecutarOperacionSifenAsync(sifenSettings, sifenOperationSettings, request, requestPayload, token),
            ct,
            timeoutMs: sifenOperationSettings.TimeoutMs,
            retryCount: sifenOperationSettings.Reintentos,
            circuitThreshold: sifenOperationSettings.CircuitThreshold,
            circuitOpenFor: sifenOperationSettings.CircuitOpenFor,
            responseSerializer: x => x.ResponsePayload);

        if (!execution.IsSuccess || execution.Value is null)
            throw new InvalidOperationException(execution.Error ?? "Falló la integración SIFEN.");

        if (!execution.Value.Confirmada)
            throw new InvalidOperationException("SIFEN rechazó el comprobante informado.");

        var operacion = SifenOperacion.Registrar(
            comprobante.Id,
            comprobante.SucursalId,
            puntoFacturacionSifenId,
            request.TimbradoFiscalId,
            requestPayload,
            execution.ResponsePayload,
            request.CodigoSeguridad,
            request.Observacion,
            request.Confirmada,
            currentUser.UserId);

        await sifenRepo.AddAsync(operacion, ct);
        return operacion;
    }

    public async Task<DeuceOperacion> ProcesarDeuceAsync(Commands.ProcesarComprobanteDeuceCommand request, CancellationToken ct)
    {
        var comprobante = await db.Comprobantes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ComprobanteId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el comprobante ID {request.ComprobanteId}.");

        var puntoFacturacionDeuceId = comprobante.PuntoFacturacionId
            ?? throw new InvalidOperationException("El comprobante no posee punto de facturación para Deuce.");

        var deuceSettings = await providerSettingsService.ResolveAsync(ProveedorIntegracionExterna.Deuce, ct);
        var deuceOperationSettings = await providerSettingsService.ResolveOperationAsync(
            ProveedorIntegracionExterna.Deuce,
            "PROCESAR_COMPROBANTE",
            "deuce/comprobantes",
            ct);
        ValidarCredencialesProveedor(deuceSettings, "DEUCE");

        var requestPayload = request.RequestPayload ?? System.Text.Json.JsonSerializer.Serialize(new DeuceProcesarRequestContract(
            comprobante.Id,
            comprobante.SucursalId,
            puntoFacturacionDeuceId,
            request.ReferenciaExterna,
            deuceSettings.Ambiente,
            deuceSettings.Endpoint,
            request.Observacion));

        var execution = await resilienceService.ExecuteAsync(
            ProveedorIntegracionExterna.Deuce,
            "PROCESAR_COMPROBANTE",
            "COMPROBANTE",
            comprobante.Id,
            requestPayload,
            token => EjecutarOperacionDeuceAsync(deuceSettings, deuceOperationSettings, request, requestPayload, token),
            ct,
            timeoutMs: deuceOperationSettings.TimeoutMs,
            retryCount: deuceOperationSettings.Reintentos,
            circuitThreshold: deuceOperationSettings.CircuitThreshold,
            circuitOpenFor: deuceOperationSettings.CircuitOpenFor,
            responseSerializer: x => x.ResponsePayload);

        if (!execution.IsSuccess || execution.Value is null)
            throw new InvalidOperationException(execution.Error ?? "Falló la integración Deuce.");

        if (!execution.Value.Confirmada)
            throw new InvalidOperationException("Deuce rechazó el comprobante informado.");

        var operacion = DeuceOperacion.Registrar(
            comprobante.Id,
            comprobante.SucursalId,
            puntoFacturacionDeuceId,
            request.ReferenciaExterna,
            requestPayload,
            execution.ResponsePayload,
            request.Observacion,
            request.Confirmada,
            currentUser.UserId);

        await deuceRepo.AddAsync(operacion, ct);
        return operacion;
    }

    private async Task ValidarSolapamientoTimbradoAsync(long? timbradoId, long puntoFacturacionId, DateOnly vigenciaDesde, DateOnly vigenciaHasta, CancellationToken ct)
    {
        var existentes = await db.TimbradosFiscales.AsNoTracking()
            .Where(x => x.Id != timbradoId && x.PuntoFacturacionId == puntoFacturacionId && x.Activo && !x.IsDeleted)
            .ToListAsync(ct);

        if (existentes.Any(x => x.SuperponeCon(vigenciaDesde, vigenciaHasta)))
            throw new InvalidOperationException("Ya existe un timbrado activo con vigencia superpuesta para el punto de facturación.");
    }

    private static SifenProcesarResponseContract ResolverOperacionSifen(Commands.ProcesarComprobanteSifenCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoSeguridad))
            throw new ExternalProviderFunctionalException("SIFEN requiere código de seguridad para procesar el comprobante.", "CODIGO_SEGURIDAD_REQUERIDO");

        if (!request.Confirmada)
            throw new ExternalProviderFunctionalException("SIFEN rechazó el comprobante informado.", "SIFEN_RECHAZADO");

        return new SifenProcesarResponseContract(request.Confirmada, request.CodigoSeguridad!, request.ResponsePayload ?? "{\"estado\":\"OK\"}", null, null);
    }

    private static DeuceProcesarResponseContract ResolverOperacionDeuce(Commands.ProcesarComprobanteDeuceCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.ReferenciaExterna))
            throw new ExternalProviderFunctionalException("Deuce requiere referencia externa para procesar el comprobante.", "REFERENCIA_REQUERIDA");

        if (!request.Confirmada)
            throw new ExternalProviderFunctionalException("Deuce rechazó el comprobante informado.", "DEUCE_RECHAZADO");

        return new DeuceProcesarResponseContract(request.Confirmada, request.ReferenciaExterna, request.ResponsePayload ?? "{\"estado\":\"OK\"}", null, null);
    }

    private static void ValidarCredencialesProveedor(ExternalIntegrationProviderSettings settings, string proveedor)
    {
        var hasCredentials = !string.IsNullOrWhiteSpace(settings.Token)
            || !string.IsNullOrWhiteSpace(settings.ApiKey)
            || (!string.IsNullOrWhiteSpace(settings.UserName) && !string.IsNullOrWhiteSpace(settings.Password));

        if (!hasCredentials)
            throw new InvalidOperationException($"{proveedor} requiere token, api key o usuario/clave configurados a nivel proveedor.");
    }

    private async Task<SifenProcesarResponseContract> EjecutarOperacionSifenAsync(ExternalIntegrationProviderSettings settings, ExternalIntegrationOperationSettings operationSettings, Commands.ProcesarComprobanteSifenCommand request, string requestPayload, CancellationToken ct)
    {
        if (!httpGateway.ShouldUseRealTransport(settings.Endpoint))
            return ResolverOperacionSifen(request);

        var response = await httpGateway.PostJsonAsync<SifenProcesarResponseContract>(settings, operationSettings.Path, requestPayload, ct);
        if (!response.Confirmada)
            throw new ExternalProviderFunctionalException(response.Mensaje ?? "SIFEN rechazó el comprobante informado.", response.CodigoError);

        return response;
    }

    private async Task<DeuceProcesarResponseContract> EjecutarOperacionDeuceAsync(ExternalIntegrationProviderSettings settings, ExternalIntegrationOperationSettings operationSettings, Commands.ProcesarComprobanteDeuceCommand request, string requestPayload, CancellationToken ct)
    {
        if (!httpGateway.ShouldUseRealTransport(settings.Endpoint))
            return ResolverOperacionDeuce(request);

        var response = await httpGateway.PostJsonAsync<DeuceProcesarResponseContract>(settings, operationSettings.Path, requestPayload, ct);
        if (!response.Confirmada)
            throw new ExternalProviderFunctionalException(response.Mensaje ?? "Deuce rechazó el comprobante informado.", response.CodigoError);

        return response;
    }
}
