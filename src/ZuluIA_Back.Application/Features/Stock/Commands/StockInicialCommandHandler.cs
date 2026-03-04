using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public class StockInicialCommandHandler(
    StockService stockService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<StockInicialCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        StockInicialCommand request,
        CancellationToken ct)
    {
        if (!request.Items.Any())
            return Result.Failure<int>("Debe especificar al menos un ítem.");

        foreach (var item in request.Items)
        {
            await stockService.AjustarAsync(
                item.ItemId,
                item.DepositoId,
                item.Cantidad,
                request.Observacion ?? "Stock inicial",
                currentUser.UserId,
                ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success(request.Items.Count);
    }
}