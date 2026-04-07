using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class DeactivateCategoriaProveedorCommandHandler(
    IRepository<CategoriaProveedor> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateCategoriaProveedorCommand, Result>
{
    public async Task<Result> Handle(DeactivateCategoriaProveedorCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Categoria de proveedor {request.Id} no encontrada.");

        entity.Desactivar(userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
