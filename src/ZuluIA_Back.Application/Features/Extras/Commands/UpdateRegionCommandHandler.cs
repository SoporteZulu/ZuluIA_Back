using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class UpdateRegionCommandHandler(
    IRepository<Region> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateRegionCommand, Result>
{
    public async Task<Result> Handle(UpdateRegionCommand request, CancellationToken ct)
    {
        var region = await repo.GetByIdAsync(request.Id, ct);
        if (region is null)
            return Result.Failure($"Region {request.Id} no encontrada.");

        try
        {
            region.Actualizar(
                request.Descripcion,
                request.RegionIntegradoraId,
                request.Orden,
                request.Nivel,
                request.CodigoEstructura,
                request.EsRegionIntegradora,
                request.Observacion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(region);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
