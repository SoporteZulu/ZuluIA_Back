using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class UpdateUnidadMedidaCommandHandler(
    IRepository<UnidadMedida> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateUnidadMedidaCommand, Result>
{
    public async Task<Result> Handle(UpdateUnidadMedidaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Unidad de medida {request.Id} no encontrada.");

        try
        {
            entity.Actualizar(
                request.Descripcion,
                request.Disminutivo,
                request.Multiplicador,
                request.EsUnidadBase,
                request.UnidadBaseId);
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