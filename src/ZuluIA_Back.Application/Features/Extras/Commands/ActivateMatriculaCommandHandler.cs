using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class ActivateMatriculaCommandHandler(
    IRepository<Matricula> repo,
    IUnitOfWork uow)
    : IRequestHandler<ActivateMatriculaCommand, Result>
{
    public async Task<Result> Handle(ActivateMatriculaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Matricula {request.Id} no encontrada.");

        entity.Activar(userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
