namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public record CreateComprobanteItemDto(
    long ItemId,
    string Descripcion,
    decimal Cantidad,
    long PrecioUnitario,
    long DescuentoPct,
    long AlicuotaIvaId,
    long PorcentajeIva,
    long? DepositoId,
    short Orden,
    long CantidadBonif = 0,
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
