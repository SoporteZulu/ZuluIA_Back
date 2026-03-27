using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class DeactivateEstadoProveedorCommandHandler(
    IRepository<EstadoProveedor> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateEstadoProveedorCommand, Result>
{
    public async Task<Result> Handle(DeactivateEstadoProveedorCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Estado de proveedor {request.Id} no encontrado.");

        entity.Desactivar(userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
