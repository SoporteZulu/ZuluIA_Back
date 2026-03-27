using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class ActivateItemCommandHandler(
    IItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivateItemCommand, Result>
{
    public async Task<Result> Handle(ActivateItemCommand request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);

        if (item is null)
            return Result.Failure($"No se encontró el ítem con ID {request.Id}.");

        item.Activar(currentUser.UserId);
        repo.Update(item);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}