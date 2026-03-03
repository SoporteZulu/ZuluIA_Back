using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public class RemoveItemDeListaCommandHandler(
    IListaPreciosRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<RemoveItemDeListaCommand, Result>
{
    public async Task<Result> Handle(
        RemoveItemDeListaCommand request,
        CancellationToken ct)
    {
        var lista = await repo.GetByIdConItemsAsync(request.ListaId, ct);
        if (lista is null)
            return Result.Failure($"No se encontró la lista de precios con ID {request.ListaId}.");

        var removido = lista.RemoverItem(request.ItemId);
        if (!removido)
            return Result.Failure($"El ítem con ID {request.ItemId} no existe en la lista.");

        repo.Update(lista);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}