using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class DeactivateTarjetaTipoCommandHandler(
    IRepository<TarjetaTipo> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeactivateTarjetaTipoCommand, Result>
{
    public async Task<Result> Handle(DeactivateTarjetaTipoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarjeta {request.Id} no encontrada.");

        entity.Desactivar(userId: null);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
