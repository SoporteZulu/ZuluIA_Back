using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class UpdateImpuestoCommandHandler(
    IRepository<Impuesto> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateImpuestoCommand, Result>
{
    public async Task<Result> Handle(UpdateImpuestoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Impuesto {request.Id} no encontrado.");

        entity.Actualizar(
            request.Descripcion,
            request.Alicuota,
            request.MinimoBaseCalculo,
            request.Tipo,
            request.Observacion);

        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}