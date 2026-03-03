using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PlanesPago.Commands;

public class CreatePlanPagoCommandHandler(
    IPlanPagoRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<CreatePlanPagoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreatePlanPagoCommand request,
        CancellationToken ct)
    {
        var plan = PlanPago.Crear(
            request.Descripcion,
            request.CantidadCuotas,
            request.InteresPct);

        await repo.AddAsync(plan, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(plan.Id);
    }
}