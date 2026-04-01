using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateEstadoPersonaCommandHandler(
    IRepository<EstadoPersonaCatalogo> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateEstadoPersonaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateEstadoPersonaCommand request, CancellationToken ct)
    {
        var descripcion = request.Descripcion.Trim();
        var exists = await repo.ExistsAsync(x => x.Descripcion == descripcion, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un estado general con esa descripción.");

        EstadoPersonaCatalogo entity;
        try
        {
            entity = EstadoPersonaCatalogo.Crear(descripcion, userId: null);
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
