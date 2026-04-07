using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateEstadoCivilCommandHandler(
    IRepository<EstadoCivilCatalogo> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateEstadoCivilCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateEstadoCivilCommand request, CancellationToken ct)
    {
        var descripcion = request.Descripcion.Trim();
        var exists = await repo.ExistsAsync(x => x.Descripcion == descripcion, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un estado civil con esa descripción.");

        EstadoCivilCatalogo entity;
        try
        {
            entity = EstadoCivilCatalogo.Crear(descripcion, userId: null);
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
