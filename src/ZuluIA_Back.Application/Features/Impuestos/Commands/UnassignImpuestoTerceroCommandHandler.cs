using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class UnassignImpuestoTerceroCommandHandler(
    IRepository<ImpuestoPorPersona> asignacionRepo,
    IUnitOfWork uow)
    : IRequestHandler<UnassignImpuestoTerceroCommand, Result>
{
    public async Task<Result> Handle(UnassignImpuestoTerceroCommand request, CancellationToken ct)
    {
        var entity = await asignacionRepo.FirstOrDefaultAsync(
            x => x.ImpuestoId == request.ImpuestoId && x.TerceroId == request.TerceroId,
            ct);

        if (entity is null)
            return Result.Failure("No se encontro asignacion de impuesto para ese tercero.");

        asignacionRepo.Remove(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
