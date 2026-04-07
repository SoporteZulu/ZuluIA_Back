using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record ComprobanteItemInput(
    long ItemId,
    string? Descripcion,
    decimal Cantidad,
    long CantidadBonificada,
    long PrecioUnitario,
    decimal DescuentoPct,
    long AlicuotaIvaId,
    long? DepositoId,
    short Orden,
    string? Lote = null,
    string? Serie = null,
    DateOnly? FechaVencimiento = null,
    long? UnidadMedidaId = null,
    string? ObservacionRenglon = null,
    decimal? PrecioListaOriginal = null,
    decimal? ComisionVendedorRenglon = null,
    long? ComprobanteItemOrigenId = null,
    decimal? CantidadDocumentoOrigen = null,
    decimal? PrecioDocumentoOrigen = null
);

public record EmitirComprobanteCommand(
    long? Id, // Si es null, crea; si tiene valor, emite el comprobante existente
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    IReadOnlyList<ComprobanteItemInput> Items,
    bool AfectaStock = true,
    string? Cae = null,
    DateOnly? FechaVtoCae = null
) : IRequest<Result<long>>;
