using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemConfiguracionVentasCommandHandler(
    IItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateItemConfiguracionVentasCommand, Result>
{
    public async Task<Result> Handle(
        UpdateItemConfiguracionVentasCommand request, 
        CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.ItemId, ct);
        if (item is null)
            return Result.Failure($"No se encontró el ítem con ID {request.ItemId}.");

        try
        {
            item.ValidarEdicionPermitida();
            item.ActualizarConfiguracionVentas(
                request.AplicaVentas,
                request.AplicaCompras,
                request.PorcentajeMaximoDescuento,
                request.EsRpt,
                currentUser.UserId);

            repo.Update(item);
            await uow.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
