using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ActivateEstadoCivilCommandHandler(
    IRepository<EstadoCivilCatalogo> repo,
    IUnitOfWork uow)
    : IRequestHandler<ActivateEstadoCivilCommand, Result>
{
    public async Task<Result> Handle(ActivateEstadoCivilCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Estado civil {request.Id} no encontrado.");

        entity.Activar(userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
