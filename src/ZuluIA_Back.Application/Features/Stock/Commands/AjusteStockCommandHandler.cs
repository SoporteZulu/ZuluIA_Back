using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public class AjusteStockCommandHandler(
    StockService stockService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AjusteStockCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        AjusteStockCommand request,
        CancellationToken ct)
    {
        var movimiento = await stockService.AjustarAsync(
            request.ItemId,
            request.DepositoId,
            request.NuevaCantidad,
            request.Observacion,
            currentUser.UserId,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success(movimiento.Id);
    }
}