using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class ActivatePlanTarjetaCommandHandler(
    IRepository<PlanTarjeta> repo,
    IUnitOfWork uow)
    : IRequestHandler<ActivatePlanTarjetaCommand, Result>
{
    public async Task<Result> Handle(ActivatePlanTarjetaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Plan de tarjeta {request.Id} no encontrado.");

        entity.Activar(userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
