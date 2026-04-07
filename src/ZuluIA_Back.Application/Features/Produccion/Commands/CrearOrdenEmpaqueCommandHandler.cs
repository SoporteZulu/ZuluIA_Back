using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class CrearOrdenEmpaqueCommandHandler(
    IApplicationDbContext db,
    IRepository<OrdenEmpaque> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CrearOrdenEmpaqueCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearOrdenEmpaqueCommand request, CancellationToken ct)
    {
        var ot = await db.OrdenesTrabajo.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.OrdenTrabajoId, ct);

        if (ot is null)
            return Result.Failure<long>($"No se encontró la OT con ID {request.OrdenTrabajoId}.");

        var orden = OrdenEmpaque.Crear(
            request.OrdenTrabajoId,
            request.ItemId,
            request.DepositoId,
            request.Fecha,
            request.Cantidad,
            request.Lote,
            request.Observacion,
            currentUser.UserId);

        await repo.AddAsync(orden, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(orden.Id);
    }
}
