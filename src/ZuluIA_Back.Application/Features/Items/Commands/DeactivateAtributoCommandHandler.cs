using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class DeactivateAtributoCommandHandler(
    IRepository<Atributo> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateAtributoCommand, Result>
{
    public async Task<Result> Handle(DeactivateAtributoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Atributo {request.Id} no encontrado.");

        entity.Desactivar();
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}