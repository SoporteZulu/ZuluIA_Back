using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class FinalizarOrdenTrabajoCommandHandler(
    IOrdenTrabajoRepository repo,
    ProduccionService produccionService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<FinalizarOrdenTrabajoCommand, Result>
{
    public async Task<Result> Handle(
        FinalizarOrdenTrabajoCommand request,
        CancellationToken ct)
    {
        var ot = await repo.GetByIdAsync(request.Id, ct);
        if (ot is null)
            return Result.Failure(
                $"No se encontró la OT con ID {request.Id}.");

        var consumos = request.Consumos?
            .GroupBy(x => x.ItemId)
            .ToDictionary(x => x.Key, x => x.Last().CantidadConsumida);

        if (ot.Estado == Domain.Enums.EstadoOrdenTrabajo.Pendiente)
            ot.Iniciar(currentUser.UserId);

        await produccionService.EjecutarProduccionAsync(
            ot,
            request.CantidadProducida,
            consumos,
            currentUser.UserId,
            ct);

        // Finalizar la OT
        ot.Finalizar(request.FechaFinReal, request.CantidadProducida ?? ot.Cantidad, currentUser.UserId);
        repo.Update(ot);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}