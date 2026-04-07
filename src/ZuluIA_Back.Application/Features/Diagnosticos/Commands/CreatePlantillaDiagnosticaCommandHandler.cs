using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreatePlantillaDiagnosticaCommandHandler(
    IApplicationDbContext db,
    IRepository<PlantillaDiagnostica> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreatePlantillaDiagnosticaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePlantillaDiagnosticaCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.PlantillasDiagnosticas.AsNoTracking().AnyAsync(x => x.Codigo == codigo, ct))
            return Result.Failure<long>($"Ya existe una plantilla diagnóstica con código '{request.Codigo}'.");

        var variableIds = request.Variables.Select(x => x.VariableId).Distinct().ToList();
        var variablesActivas = await db.VariablesDiagnosticas.AsNoTracking()
            .Where(x => variableIds.Contains(x.Id) && x.Activa)
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (variablesActivas.Count != variableIds.Count)
            return Result.Failure<long>("Una o más variables de la plantilla no existen o están inactivas.");

        var plantilla = PlantillaDiagnostica.Crear(request.Codigo, request.Descripcion, request.Observacion, currentUser.UserId);
        foreach (var variable in request.Variables.OrderBy(x => x.Orden))
            plantilla.AgregarVariable(PlantillaDiagnosticaVariable.Crear(0, variable.VariableId, variable.Orden));

        await repo.AddAsync(plantilla, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(plantilla.Id);
    }
}
