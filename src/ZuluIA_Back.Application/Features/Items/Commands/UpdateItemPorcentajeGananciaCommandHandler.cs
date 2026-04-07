using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemPorcentajeGananciaCommandHandler(
    IItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateItemPorcentajeGananciaCommand, Result>
{
    public async Task<Result> Handle(
        UpdateItemPorcentajeGananciaCommand request, 
        CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.ItemId, ct);
        if (item is null)
            return Result.Failure($"No se encontró el ítem con ID {request.ItemId}.");

        try
        {
            item.ValidarEdicionPermitida();
            item.ActualizarPorcentajeGanancia(request.PorcentajeGanancia, currentUser.UserId);

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
