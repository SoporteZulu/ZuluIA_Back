using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public class UpdateListaPreciosCommandHandler(
    IListaPreciosRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateListaPreciosCommand, Result>
{
    public async Task<Result> Handle(
        UpdateListaPreciosCommand request,
        CancellationToken ct)
    {
        var lista = await repo.GetByIdAsync(request.Id, ct);
        if (lista is null)
            return Result.Failure($"No se encontró la lista de precios con ID {request.Id}.");

        lista.Actualizar(
            request.Descripcion,
            request.MonedaId,
            request.VigenciaDesde,
            request.VigenciaHasta,
            currentUser.UserId,
            request.EsPorDefecto,
            request.ListaPadreId,
            request.Prioridad,
            request.Observaciones);

        repo.Update(lista);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}