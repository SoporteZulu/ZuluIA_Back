using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class DeactivateEstadoClienteCommandHandler(
    IRepository<EstadoCliente> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateEstadoClienteCommand, Result>
{
    public async Task<Result> Handle(DeactivateEstadoClienteCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Estado de cliente {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
