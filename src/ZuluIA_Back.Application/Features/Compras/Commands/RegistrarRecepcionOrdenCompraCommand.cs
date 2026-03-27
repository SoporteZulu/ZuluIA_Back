using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public record RegistrarRecepcionOrdenCompraCommand(
    long OrdenCompraId,
    DateOnly FechaRecepcion,
    decimal CantidadRecibida,
    long? TipoComprobanteRemitoId,
    bool RemitoValorizado,
    string? Observacion
) : IRequest<Result<long>>;
