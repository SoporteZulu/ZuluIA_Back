using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class SetAtributoItemCommandHandler(
    IRepository<AtributoItem> repo,
    IUnitOfWork uow)
    : IRequestHandler<SetAtributoItemCommand, Result<SetAtributoItemResult>>
{
    public async Task<Result<SetAtributoItemResult>> Handle(SetAtributoItemCommand request, CancellationToken ct)
    {
        var existing = await repo.FirstOrDefaultAsync(
            x => x.ItemId == request.ItemId && x.AtributoId == request.AtributoId,
            ct);

        if (existing is not null)
        {
            existing.ActualizarValor(request.Valor);
            repo.Update(existing);
            await uow.SaveChangesAsync(ct);
            return Result.Success(new SetAtributoItemResult(existing.Id, true));
        }

        AtributoItem entity;
        try
        {
            entity = AtributoItem.Crear(request.ItemId, request.AtributoId, request.Valor);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<SetAtributoItemResult>(ex.Message);
        }

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new SetAtributoItemResult(entity.Id, false));
    }
}