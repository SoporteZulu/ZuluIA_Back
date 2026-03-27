using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateCategoriaProveedorCommandHandler(
    IRepository<CategoriaProveedor> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateCategoriaProveedorCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoriaProveedorCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Categoria de proveedor {request.Id} no encontrada.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await repo.ExistsAsync(x => x.Id != request.Id && x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure("Ya existe una categoria de proveedor con ese codigo.");

        try
        {
            entity.Actualizar(codigo, request.Descripcion, userId: null);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
