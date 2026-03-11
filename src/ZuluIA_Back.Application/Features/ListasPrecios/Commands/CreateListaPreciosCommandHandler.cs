using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public class CreateListaPreciosCommandHandler(
    IListaPreciosRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateListaPreciosCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateListaPreciosCommand request,
        CancellationToken ct)
    {
        var lista = ListaPrecios.Crear(
            request.Descripcion,
            request.MonedaId,
            request.VigenciaDesde,
            request.VigenciaHasta,
            currentUser.UserId);

        await repo.AddAsync(lista, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(lista.Id);
    }
}