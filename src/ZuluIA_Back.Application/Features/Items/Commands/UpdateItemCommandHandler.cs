using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemCommandHandler(
    IItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateItemCommand, Result>
{
    public async Task<Result> Handle(UpdateItemCommand request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);

        if (item is null)
            return Result.Failure($"No se encontró el ítem con ID {request.Id}.");

        if (request.PrecioCosto != item.PrecioCosto || request.PrecioVenta != item.PrecioVenta)
            item.ActualizarPrecios(request.PrecioCosto, request.PrecioVenta, currentUser.UserId);

        item.Actualizar(
            request.Descripcion,
            request.DescripcionAdicional,
            request.CodigoBarras,
            request.CategoriaId,
            request.StockMinimo,
            request.StockMaximo,
            request.CodigoAfip,
            currentUser.UserId);

        repo.Update(item);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}