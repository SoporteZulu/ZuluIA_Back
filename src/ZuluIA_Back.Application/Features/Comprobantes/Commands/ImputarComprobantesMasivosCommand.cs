using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

/// <summary>
/// Imputa varios pares de comprobantes (origen → destino) en una sola transacción.
/// Equivale a frmImputacionesVentasMasivas / frmImputacionesComprasMasivas del VB6.
/// </summary>
public record ImputarComprobantesMasivosCommand(
    DateOnly Fecha,
    IReadOnlyList<ImputacionMasivaItemDto> Items
) : IRequest<Result<IReadOnlyList<long>>>;

public record ImputacionMasivaItemDto(
    long ComprobanteOrigenId,
    long ComprobanteDestinoId,
    decimal Importe
);
