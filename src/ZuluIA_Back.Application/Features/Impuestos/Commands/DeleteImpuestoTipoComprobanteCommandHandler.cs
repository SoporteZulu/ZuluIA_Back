using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class DeleteImpuestoTipoComprobanteCommandHandler(
    IRepository<ImpuestoPorTipoComprobante> asignacionRepo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteImpuestoTipoComprobanteCommand, Result>
{
    public async Task<Result> Handle(DeleteImpuestoTipoComprobanteCommand request, CancellationToken ct)
    {
        var entity = await asignacionRepo.FirstOrDefaultAsync(
            x => x.Id == request.AsignacionId && x.ImpuestoId == request.ImpuestoId,
            ct);

        if (entity is null)
            return Result.Failure("No se encontro la asignacion de tipo comprobante especificada.");

        asignacionRepo.Remove(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
