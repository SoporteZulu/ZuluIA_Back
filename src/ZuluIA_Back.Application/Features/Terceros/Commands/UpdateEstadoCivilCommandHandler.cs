using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateEstadoCivilCommandHandler(
    IRepository<EstadoCivilCatalogo> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateEstadoCivilCommand, Result>
{
    public async Task<Result> Handle(UpdateEstadoCivilCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Estado civil {request.Id} no encontrado.");

        var descripcion = request.Descripcion.Trim();
        var exists = await repo.ExistsAsync(x => x.Id != request.Id && x.Descripcion == descripcion, ct);
        if (exists)
            return Result.Failure("Ya existe un estado civil con esa descripción.");

        try
        {
            entity.Actualizar(descripcion, userId: null);
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
