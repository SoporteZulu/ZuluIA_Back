using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class UnassignImpuestoItemCommandHandler(
    IRepository<ImpuestoPorItem> asignacionRepo,
    IUnitOfWork uow)
    : IRequestHandler<UnassignImpuestoItemCommand, Result>
{
    public async Task<Result> Handle(UnassignImpuestoItemCommand request, CancellationToken ct)
    {
        var entity = await asignacionRepo.FirstOrDefaultAsync(
            x => x.ImpuestoId == request.ImpuestoId && x.ItemId == request.ItemId,
            ct);

        if (entity is null)
            return Result.Failure("No se encontro asignacion de impuesto para ese item.");

        asignacionRepo.Remove(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
