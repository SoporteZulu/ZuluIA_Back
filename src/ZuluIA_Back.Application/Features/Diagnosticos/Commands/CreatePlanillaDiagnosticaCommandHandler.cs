using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreatePlanillaDiagnosticaCommandHandler(
    IApplicationDbContext db,
    IRepository<PlanillaDiagnostica> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreatePlanillaDiagnosticaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePlanillaDiagnosticaCommand request, CancellationToken ct)
    {
        if (!await db.PlantillasDiagnosticas.AsNoTracking().AnyAsync(x => x.Id == request.PlantillaId && x.Activa, ct))
            return Result.Failure<long>($"No se encontró la plantilla diagnóstica ID {request.PlantillaId}.");

        var planilla = PlanillaDiagnostica.Crear(request.PlantillaId, request.Nombre, request.Fecha, request.Observacion, currentUser.UserId);
        await repo.AddAsync(planilla, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(planilla.Id);
    }
}
