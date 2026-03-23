using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateRegionCommandHandler(
    IRepository<Region> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateRegionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateRegionCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await repo.ExistsAsync(r => r.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>($"Ya existe una region con codigo '{request.Codigo}'.");

        Region region;
        try
        {
            region = Region.Crear(
                codigo,
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
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(region, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(region.Id);
    }
}
