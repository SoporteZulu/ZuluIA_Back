using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class UpdateTimbradoCommandHandler(
    IRepository<Timbrado> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateTimbradoCommand, Result>
{
    public async Task<Result> Handle(UpdateTimbradoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Timbrado {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(
                request.FechaInicio,
                request.FechaFin,
                request.NroComprobanteDesde,
                request.NroComprobanteHasta);
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