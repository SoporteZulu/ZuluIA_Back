using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record UpdateItemPreciosCommand(
    long Id,
    decimal PrecioCosto,
    decimal PrecioVenta
) : IRequest<Result>;