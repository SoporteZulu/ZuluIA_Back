using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class ActivateUnidadMedidaCommandHandler(
    IRepository<UnidadMedida> repo,
    IUnitOfWork uow)
    : IRequestHandler<ActivateUnidadMedidaCommand, Result>
{
    public async Task<Result> Handle(ActivateUnidadMedidaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Unidad de medida {request.Id} no encontrada.");

        entity.Activar();
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}