using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class ActivateMenuItemCommandHandler(
    IMenuRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<ActivateMenuItemCommand, Result>
{
    public async Task<Result> Handle(ActivateMenuItemCommand request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);

        if (item is null)
            return Result.Failure($"No se encontro el item de menu con ID {request.Id}.");

        item.Activar();
        repo.Update(item);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}