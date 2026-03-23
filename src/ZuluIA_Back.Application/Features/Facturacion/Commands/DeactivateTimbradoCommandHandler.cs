using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class DeactivateTimbradoCommandHandler(
    IRepository<Timbrado> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateTimbradoCommand, Result>
{
    public async Task<Result> Handle(DeactivateTimbradoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Timbrado {request.Id} no encontrado.");

        entity.Desactivar();
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}