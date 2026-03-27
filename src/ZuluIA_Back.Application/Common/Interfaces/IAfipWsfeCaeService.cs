namespace ZuluIA_Back.Application.Common.Interfaces;

public sealed record AfipWsfeCaeAlicuotaRequest(short Id, decimal BaseImponible, decimal Importe);

public sealed record AfipWsfeCaeTributoRequest(short Id, string Descripcion, decimal BaseImponible, decimal Alicuota, decimal Importe);

public sealed record SolicitarCaeAfipRequest(
    short PuntoVenta,
    short TipoComprobante,
    long NumeroComprobante,
    DateOnly FechaEmision,
    short Concepto,
    short TipoDocumento,
    long NumeroDocumento,
    decimal ImporteTotal,
    decimal ImporteNeto,
    decimal ImporteNoGravado,
    decimal ImporteExento,
    decimal ImporteTributos,
    decimal ImporteIva,
    string MonedaCodigo,
    decimal MonedaCotizacion,
    IReadOnlyCollection<AfipWsfeCaeAlicuotaRequest> Alicuotas,
    IReadOnlyCollection<AfipWsfeCaeTributoRequest> Tributos);

public sealed record SolicitarCaeAfipResponse(
    string Cae,
    DateOnly FechaVencimiento);

public interface IAfipWsfeCaeService
{
    Task<SolicitarCaeAfipResponse> SolicitarCaeAsync(
        SolicitarCaeAfipRequest request,
        CancellationToken cancellationToken = default);
}