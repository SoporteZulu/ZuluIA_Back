using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public class ActivateListaPreciosCommandHandler(
    IListaPreciosRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivateListaPreciosCommand, Result>
{
    public async Task<Result> Handle(
        ActivateListaPreciosCommand request,
        CancellationToken ct)
    {
        var lista = await repo.GetByIdAsync(request.Id, ct);
        if (lista is null)
            return Result.Failure($"No se encontró la lista de precios con ID {request.Id}.");

        lista.Activar(currentUser.UserId);
        repo.Update(lista);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}