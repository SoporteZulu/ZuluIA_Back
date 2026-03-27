using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class UpdatePlanTarjetaCommandHandler(
    IRepository<PlanTarjeta> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdatePlanTarjetaCommand, Result>
{
    public async Task<Result> Handle(UpdatePlanTarjetaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Plan de tarjeta {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(
                request.Descripcion,
                request.CantidadCuotas,
                request.Recargo,
                request.DiasAcreditacion,
                userId: null);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
