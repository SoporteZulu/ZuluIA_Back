using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PlanesPago.Commands;

public class UpdatePlanPagoCommandHandler(
    IPlanPagoRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdatePlanPagoCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePlanPagoCommand request,
        CancellationToken ct)
    {
        var plan = await repo.GetByIdAsync(request.Id, ct);
        if (plan is null)
            return Result.Failure($"No se encontró el plan de pago con ID {request.Id}.");

        plan.Actualizar(request.Descripcion, request.CantidadCuotas, request.InteresPct);
        repo.Update(plan);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}