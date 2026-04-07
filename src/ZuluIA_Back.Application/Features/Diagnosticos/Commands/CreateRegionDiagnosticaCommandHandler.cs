using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreateRegionDiagnosticaCommandHandler(
    IApplicationDbContext db,
    IRepository<RegionDiagnostica> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateRegionDiagnosticaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateRegionDiagnosticaCommand request, CancellationToken ct)
    {
        if (await db.RegionesDiagnosticas.AsNoTracking().AnyAsync(x => x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct))
            return Result.Failure<long>($"Ya existe una región diagnóstica con código '{request.Codigo}'.");

        var region = RegionDiagnostica.Crear(request.Codigo, request.Descripcion, currentUser.UserId);
        await repo.AddAsync(region, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(region.Id);
    }
}
