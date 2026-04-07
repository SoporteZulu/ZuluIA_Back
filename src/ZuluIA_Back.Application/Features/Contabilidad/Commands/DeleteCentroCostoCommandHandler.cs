using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class DeleteCentroCostoCommandHandler(
    IRepository<CentroCosto> repo,
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<DeleteCentroCostoCommand, Result>
{
    public async Task<Result> Handle(DeleteCentroCostoCommand request, CancellationToken ct)
    {
        var cc = await repo.GetByIdAsync(request.Id, ct);
        if (cc is null)
            return Result.Failure($"No se encontro el centro de costo con ID {request.Id}.");

        var tieneLineas = await db.AsientosLineas
            .AnyAsync(x => x.CentroCostoId == request.Id, ct);

        if (tieneLineas)
            return Result.Failure("No se puede desactivar un centro de costo con asientos registrados.");

        cc.Desactivar();
        repo.Update(cc);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}