using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class DeleteAtributoItemCommandHandler(
    IRepository<AtributoItem> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteAtributoItemCommand, Result>
{
    public async Task<Result> Handle(DeleteAtributoItemCommand request, CancellationToken ct)
    {
        var entity = await repo.FirstOrDefaultAsync(
            x => x.ItemId == request.ItemId && x.AtributoId == request.AtributoId,
            ct);

        if (entity is null)
            return Result.Failure("No se encontro el atributo del item especificado.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}