using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemPreciosCommandHandler(
    IItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateItemPreciosCommand, Result>
{
    public async Task<Result> Handle(
        UpdateItemPreciosCommand request,
        CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);
        if (item is null)
            return Result.Failure($"No se encontró el ítem con ID {request.Id}.");

        item.ActualizarPrecios(
            request.PrecioCosto,
            request.PrecioVenta,
            currentUser.UserId);

        repo.Update(item);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}