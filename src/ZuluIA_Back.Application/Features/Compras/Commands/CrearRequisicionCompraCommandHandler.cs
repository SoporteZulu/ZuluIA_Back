using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class CrearRequisicionCompraCommandHandler(
    IRequisicionCompraRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CrearRequisicionCompraCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearRequisicionCompraCommand request, CancellationToken ct)
    {
        if (!request.Items.Any())
            return Result.Failure<long>("La requisición debe tener al menos un ítem.");

        var req = RequisicionCompra.Crear(
            request.SucursalId,
            request.SolicitanteId,
            request.Fecha,
            request.Descripcion,
            request.Observacion,
            currentUser.UserId);

        foreach (var item in request.Items)
            req.AgregarItem(RequisicionCompraItem.Crear(
                0, item.ItemId, item.Descripcion,
                item.Cantidad, item.UnidadMedida, item.Observacion));

        await repo.AddAsync(req, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(req.Id);
    }
}
