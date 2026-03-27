using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class UpdateImpuestoSucursalCommandHandler(
    IRepository<ImpuestoPorSucursal> asignacionRepo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateImpuestoSucursalCommand, Result<UpdateImpuestoSucursalResult>>
{
    public async Task<Result<UpdateImpuestoSucursalResult>> Handle(UpdateImpuestoSucursalCommand request, CancellationToken ct)
    {
        var entity = await asignacionRepo.FirstOrDefaultAsync(
            x => x.Id == request.AsignacionId && x.ImpuestoId == request.ImpuestoId,
            ct);

        if (entity is null)
            return Result.Failure<UpdateImpuestoSucursalResult>("No se encontro la asignacion de sucursal especificada.");

        entity.Actualizar(request.Descripcion, request.Observacion);
        asignacionRepo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new UpdateImpuestoSucursalResult(entity.Id, entity.Descripcion, entity.Observacion));
    }
}
