using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateCategoriaItemCommandHandler(
    ICategoriaItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCategoriaItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateCategoriaItemCommand request,
        CancellationToken ct)
    {
        if (await repo.ExisteCodigoAsync(request.Codigo, null, ct))
            return Result.Failure<long>(
                $"Ya existe una categoría con el código '{request.Codigo}'.");

        var categoria = CategoriaItem.Crear(
            request.ParentId,
            request.Codigo,
            request.Descripcion,
            request.Nivel,
            request.OrdenNivel,
            currentUser.UserId);

        await repo.AddAsync(categoria, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(categoria.Id);
    }
}