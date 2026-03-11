using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public class TransferenciaStockCommandHandler(
    StockService stockService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<TransferenciaStockCommand, Result>
{
    public async Task<Result> Handle(
        TransferenciaStockCommand request,
        CancellationToken ct)
    {
        await stockService.TransferirAsync(
            request.ItemId,
            request.DepositoOrigenId,
            request.DepositoDestinoId,
            request.Cantidad,
            request.Observacion,
            currentUser.UserId,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}