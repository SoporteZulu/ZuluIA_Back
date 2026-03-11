using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public record AjusteStockCommand(
    long ItemId,
    long DepositoId,
    decimal NuevaCantidad,
    string? Observacion
) : IRequest<Result<long>>;