using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateEstadoProveedorCommandHandler(
    IRepository<EstadoProveedor> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateEstadoProveedorCommand, Result>
{
    public async Task<Result> Handle(UpdateEstadoProveedorCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Estado de proveedor {request.Id} no encontrado.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await repo.ExistsAsync(x => x.Id != request.Id && x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure("Ya existe un estado de proveedor con ese codigo.");

        try
        {
            entity.Actualizar(request.Descripcion, request.Bloquea, userId: null);
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
