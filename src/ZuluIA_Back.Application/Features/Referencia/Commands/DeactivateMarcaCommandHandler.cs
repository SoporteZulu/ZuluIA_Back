using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class DeactivateMarcaCommandHandler(
    IRepository<Marca> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateMarcaCommand, Result>
{
    public async Task<Result> Handle(DeactivateMarcaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Marca {request.Id} no encontrada.");

        entity.Desactivar();
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}