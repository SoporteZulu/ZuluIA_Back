using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Application.Common.Interfaces;

public sealed record PrepararEnvioSifenRequest(
    long ComprobanteId,
    string RucEmisor,
    string RazonSocialEmisor,
    string? DireccionEmisor,
    string DocumentoReceptor,
    string RazonSocialReceptor,
    string? DireccionReceptor,
    string CodigoTipoComprobante,
    string DescripcionTipoComprobante,
    short? PuntoExpedicion,
    short Prefijo,
    long NumeroComprobante,
    DateOnly FechaEmision,
    decimal Total,
    decimal NetoGravado,
    decimal Iva,
    decimal NetoNoGravado,
    decimal Percepciones,
    int CantidadItems,
    long? TimbradoId,
    string? NroTimbrado,
    string? Observacion);

public sealed record ConsultarEstadoSifenRequest(
    long ComprobanteId,
    string? TrackingId,
    string? Cdc,
    string? NumeroLote);

public interface IParaguaySifenService
{
    Task<PreparacionSifenParaguayDto> PrepararEnvioAsync(
        PrepararEnvioSifenRequest request,
        CancellationToken cancellationToken = default);

    Task<ResultadoEnvioSifenParaguayDto> EnviarAsync(
        PrepararEnvioSifenRequest request,
        CancellationToken cancellationToken = default);

    Task<ResultadoEnvioSifenParaguayDto> ConsultarEstadoAsync(
        ConsultarEstadoSifenRequest request,
        CancellationToken cancellationToken = default);
}