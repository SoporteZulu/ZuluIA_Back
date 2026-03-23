using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class DeactivateImpuestoCommandHandler(
    IRepository<Impuesto> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateImpuestoCommand, Result>
{
    public async Task<Result> Handle(DeactivateImpuestoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Impuesto {request.Id} no encontrado.");

        entity.Desactivar();
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}