using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreateAspectoDiagnosticoCommandHandler(
    IApplicationDbContext db,
    IRepository<AspectoDiagnostico> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateAspectoDiagnosticoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateAspectoDiagnosticoCommand request, CancellationToken ct)
    {
        if (!await db.RegionesDiagnosticas.AsNoTracking().AnyAsync(x => x.Id == request.RegionId, ct))
            return Result.Failure<long>($"No se encontró la región diagnóstica ID {request.RegionId}.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.AspectosDiagnostico.AsNoTracking().AnyAsync(x => x.RegionId == request.RegionId && x.Codigo == codigo, ct))
            return Result.Failure<long>($"Ya existe un aspecto con código '{request.Codigo}' para la región indicada.");

        var aspecto = AspectoDiagnostico.Crear(request.RegionId, request.Codigo, request.Descripcion, request.Peso, currentUser.UserId);
        await repo.AddAsync(aspecto, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(aspecto.Id);
    }
}
