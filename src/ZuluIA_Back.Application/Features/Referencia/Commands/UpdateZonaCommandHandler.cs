using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class UpdateZonaCommandHandler(
    IRepository<Zona> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateZonaCommand, Result>
{
    public async Task<Result> Handle(UpdateZonaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Zona {request.Id} no encontrada.");

        try
        {
            entity.Actualizar(request.Descripcion);
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