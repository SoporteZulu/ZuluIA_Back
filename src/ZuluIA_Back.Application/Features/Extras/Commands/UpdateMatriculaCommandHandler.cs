using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class UpdateMatriculaCommandHandler(
    IRepository<Matricula> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateMatriculaCommand, Result>
{
    public async Task<Result> Handle(UpdateMatriculaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Matricula {request.Id} no encontrada.");

        entity.Actualizar(request.Descripcion, request.FechaVencimiento, userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
