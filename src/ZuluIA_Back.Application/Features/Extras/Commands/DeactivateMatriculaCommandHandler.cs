using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class DeactivateMatriculaCommandHandler(
    IRepository<Matricula> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateMatriculaCommand, Result>
{
    public async Task<Result> Handle(DeactivateMatriculaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Matricula {request.Id} no encontrada.");

        entity.Desactivar(userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
