using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ActivateTimbradoCommandHandler(
    IRepository<Timbrado> repo,
    IUnitOfWork uow)
    : IRequestHandler<ActivateTimbradoCommand, Result>
{
    public async Task<Result> Handle(ActivateTimbradoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Timbrado {request.Id} no encontrado.");

        entity.Activar();
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}