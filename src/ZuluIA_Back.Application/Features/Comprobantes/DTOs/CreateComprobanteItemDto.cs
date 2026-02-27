namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public record CreateComprobanteItemDto(
long ItemId,
string Descripcion,
decimal Cantidad,
decimal PrecioUnitario,
decimal DescuentoPct,
long AlicuotaIvaId,
decimal PorcentajeIva,
long? DepositoId,
short Orden,
decimal CantidadBonif = 0
);
