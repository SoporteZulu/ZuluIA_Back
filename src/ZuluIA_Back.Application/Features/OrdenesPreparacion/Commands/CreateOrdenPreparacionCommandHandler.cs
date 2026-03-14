using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class CreateOrdenPreparacionCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<CreateOrdenPreparacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateOrdenPreparacionCommand request,
        CancellationToken ct)
    {
        if (!request.Detalles.Any())
            return Result.Failure<long>("La orden de preparación debe tener al menos un detalle.");

        var orden = OrdenPreparacion.Crear(
            request.SucursalId,
            request.ComprobanteOrigenId,
            request.TerceroId,
            request.Fecha,
            request.Observacion,
            userId: null);

        // Agregamos detalles después de persistir para tener el ID
        db.OrdenesPreparacion.Add(orden);
        await uow.SaveChangesAsync(ct);

        foreach (var d in request.Detalles)
            orden.AgregarDetalle(d.ItemId, d.DepositoId, d.Cantidad, d.Observacion);

        await uow.SaveChangesAsync(ct);

        return Result.Success(orden.Id);
    }
}
