using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public class UpsertItemEnListaCommandHandler(
    IListaPreciosRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<UpsertItemEnListaCommand, Result>
{
    public async Task<Result> Handle(
        UpsertItemEnListaCommand request,
        CancellationToken ct)
    {
        var lista = await repo.GetByIdConItemsAsync(request.ListaId, ct);
        if (lista is null)
            return Result.Failure($"No se encontró la lista de precios con ID {request.ListaId}.");

        lista.UpsertItem(request.ItemId, request.Precio, request.DescuentoPct);

        repo.Update(lista);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}