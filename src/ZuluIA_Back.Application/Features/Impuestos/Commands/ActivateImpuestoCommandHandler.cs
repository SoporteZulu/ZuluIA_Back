using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class ActivateImpuestoCommandHandler(
    IRepository<Impuesto> repo,
    IUnitOfWork uow)
    : IRequestHandler<ActivateImpuestoCommand, Result>
{
    public async Task<Result> Handle(ActivateImpuestoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Impuesto {request.Id} no encontrado.");

        entity.Activar();
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}