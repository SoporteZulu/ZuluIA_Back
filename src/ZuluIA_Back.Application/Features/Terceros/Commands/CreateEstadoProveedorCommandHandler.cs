using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateEstadoProveedorCommandHandler(
    IRepository<EstadoProveedor> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateEstadoProveedorCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateEstadoProveedorCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await repo.ExistsAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un estado de proveedor con ese codigo.");

        EstadoProveedor entity;
        try
        {
            entity = EstadoProveedor.Crear(codigo, request.Descripcion, request.Bloquea, userId: null);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}
