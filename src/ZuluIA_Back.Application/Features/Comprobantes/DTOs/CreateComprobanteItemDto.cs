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
long CantidadBonif = 0
);
