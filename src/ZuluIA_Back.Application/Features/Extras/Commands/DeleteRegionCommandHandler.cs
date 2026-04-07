using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class DeleteRegionCommandHandler(
    IRepository<Region> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteRegionCommand, Result>
{
    public async Task<Result> Handle(DeleteRegionCommand request, CancellationToken ct)
    {
        var region = await repo.GetByIdAsync(request.Id, ct);
        if (region is null)
            return Result.Failure($"Region {request.Id} no encontrada.");

        var tieneHijos = await repo.ExistsAsync(r => r.RegionIntegradoraId == request.Id, ct);
        if (tieneHijos)
            return Result.Failure("No se puede eliminar una region que tiene sub-regiones.");

        repo.Remove(region);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
